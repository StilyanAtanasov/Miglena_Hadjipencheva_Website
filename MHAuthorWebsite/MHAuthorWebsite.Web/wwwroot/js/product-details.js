"use strict";

import { initQuill } from "./editor.js";
import { pushNotification, showPopupAsync } from "./notification.js";
import { calculateStarsFill } from "./elements/stars.js";
import { openModal, replaceBody } from "./elements/modal.js";
import { reactToComment } from "./react-to-product-comment.js";
import { formatLocalDates } from "./time-zone-manager.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  // --- Discount End Time Counter ---
  const timer = document.getElementById(`countdown-timer`);
  if (timer) {
    const endDate = new Date(timer.dataset.endDate);

    function updateCountdown() {
      const now = new Date();
      const diff = endDate - now;

      if (diff <= 0) {
        document.getElementById(`days`).textContent = 0;
        document.getElementById(`hours`).textContent = 0;
        document.getElementById(`minutes`).textContent = 0;
        document.getElementById(`seconds`).textContent = 0;
        return;
      }

      const totalSeconds = Math.floor(diff / 1000);

      const days = Math.floor(totalSeconds / 86400);
      const hours = Math.floor((totalSeconds % 86400) / 3600);
      const minutes = Math.floor((totalSeconds % 3600) / 60);
      const seconds = totalSeconds % 60;

      document.getElementById(`days`).textContent = days.toString().padStart(2, `0`);
      document.getElementById(`hours`).textContent = hours.toString().padStart(2, `0`);
      document.getElementById(`minutes`).textContent = minutes.toString().padStart(2, `0`);
      document.getElementById(`seconds`).textContent = seconds.toString().padStart(2, `0`);
    }

    updateCountdown();
    setInterval(updateCountdown, 1000);
  }

  // --- Comments ---
  let currentCommentsPage = 1;
  let currentRepliesPage = 1;
  let currentRatingFilter = null;
  const moreCommentsBtnEl = document.getElementById(`more-comments-btn`);
  const productId = document.getElementById(`product-main-info`).dataset.productId;
  const commentsContainer = document.getElementById(`comments`);

  if (commentsContainer) {
    const noResultsContainer = document.getElementById(`no-comment-results`);
    const isRateLimitedForReplies = commentsContainer.dataset.isRateLimitedForReplies == `True`;

    // - Listeners -
    commentsContainer.addEventListener(`click`, async function (e) {
      const loadRepliesBtn = e.target.closest(`.load-replies-btn`);
      if (loadRepliesBtn) loadCommentRepliesAsync(productId, loadRepliesBtn.dataset.commentId, currentRepliesPage + 1, loadRepliesBtn);

      const reactionBtn = e.target.closest(`.react-btn`);
      if (reactionBtn) reactToComment(reactionBtn);

      const replyBtn = e.target.closest(`.reply-btn`);
      if (replyBtn && isRateLimitedForReplies) {
        e.preventDefault();
        showPopupAsync({
          title: `Не сега...`,
          text: `Вие добавихте прекалено много отговори за кратък период. Моля, опитайте пак по-късно!`,
          icon: `warning`,
          confirmButtonText: `OK`,
          allowOutsideClick: true,
        });
      }

      // Load comment details
      const imagesContainer = e.target.closest(`.comment .images`);
      if (imagesContainer) loadCommentDetailsAsync(imagesContainer.dataset.commentId);

      // Delete comment
      const deleteBtn = e.target.closest(`.delete-btn`);
      if (deleteBtn) {
        await showPopupAsync({
          icon: `warning`,
          title: `Изтриване на коментар`,
          text: `Коментарът не може да бъде възстановен!`,
          onConfirm: deleteCommentAsync,
          onConfirmArgs: [deleteBtn.dataset.commentId, deleteBtn.dataset.productId, deleteBtn.closest(`.comment`)],
          showCancelButton: true,
          allowOutsideClick: true,
        });
      }
    });

    async function loadCommentDetailsAsync(commentId) {
      const response = await fetch(`/ProductComment/Details?commentId=${commentId}`);

      if (response.ok) {
        const html = await response.text();
        replaceBody(html);
        formatLocalDates();
        openModal();
      } else {
        pushNotification(`Възникна грешка при зареждане на коментара!`, `error`);
      }
    }

    async function deleteCommentAsync(commentId, productId, commentElement) {
      const response = await fetch(`/ProductComment/Delete?commentId=${commentId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        if (!commentElement.classList.contains(`reply`)) {
          const nextElement = commentElement.nextElementSibling;
          if (nextElement && nextElement.tagName === `HR`) nextElement.remove();

          const newAverateRating = await (await fetch(`/ProductComment/GetAverageRating?productId=${productId}`)).json();

          document.getElementById(`average-rating-number`).textContent = newAverateRating.toFixed(1);
          document.querySelector(`.ratings-count span`).textContent = +document.querySelector(`.ratings-count span`).textContent - 1;

          const rating = +commentElement.dataset.rating;
          const bar = document.querySelector(`.rating-bar[data-rating="${rating}"]`);
          const newCount = +bar.dataset.count - 1;

          bar.querySelector(`.rating-count`).textContent = `(${newCount})`;
          bar.dataset.count = newCount;

          const ratingMaxValue = document.querySelector(`.quick-stats`).dataset.ratingMaxValue;
          document.querySelector(`.quick-stats .stars`).dataset.percent = (newAverateRating / ratingMaxValue) * 100;

          calculateStarsFill();
          fillCommentStats();
        }

        commentElement.remove();
        showPopupAsync({ title: `Коментарът е изтрит успешно!`, confirmButtonText: `OK` });
      } else pushNotification(`Възникна грешка при изтриването на коментара!`, `error`);
    }

    moreCommentsBtnEl && moreCommentsBtnEl.addEventListener(`click`, () => loadCommentsAsync(productId, currentCommentsPage + 1, currentRatingFilter));

    // - Load comments -
    async function loadCommentsAsync(productId, page, ratingFilter) {
      const response = await fetch(`/ProductComment/LoadComments?productId=${productId}&page=${page}${ratingFilter ? "&ratingFilter=" + ratingFilter : ""}`);

      if (response.ok) {
        const data = await response.json();

        currentRatingFilter = ratingFilter;
        currentCommentsPage = page;

        if (currentCommentsPage === 1) {
          commentsContainer.innerHTML = ``;
          currentRepliesPage = 1;
        }

        noResultsContainer.classList.add(`hidden`);
        commentsContainer.insertAdjacentHTML(`beforeend`, data.comments);
        moreCommentsBtnEl && moreCommentsBtnEl.classList.toggle(`hidden`, !data.hasMoreComments);
        !ratingFilter && ratingBarElements.forEach(el => el.classList.remove(`faded`));

        if (!data.comments.replaceAll(`\r`, ``).replaceAll(`\n`, ``)) return noResultsContainer.classList.remove(`hidden`);
        calculateStarsFill();
        formatLocalDates();
      } else {
        pushNotification(`Грешка при зареждането на коментарите!`, `error`);
      }
    }

    // - Load comments with rating filter -
    // - Fill average rating stars -
    const ratingStatsSection = document.getElementById(`rating-stats`);
    const allRatingsCount = +ratingStatsSection.dataset.count;
    const ratingBarElements = ratingStatsSection.querySelectorAll(`.rating-bar`);

    function fillCommentStats() {
      ratingBarElements.forEach(b => {
        const ratingsCount = b.dataset.count;
        const rating = +b.dataset.rating;

        b.querySelector(`.bar-container .bar-fill`).style.width = `${(ratingsCount / allRatingsCount) * 100}%`;
        b.addEventListener(`click`, function () {
          if (currentRatingFilter == rating) return loadCommentsAsync(productId, 1, null);

          loadCommentsAsync(productId, 1, rating);
          ratingBarElements.forEach(el => el.classList.add(`faded`));
          b.classList.remove(`faded`);
        });
      });
    }

    fillCommentStats();

    // - Load replies -
    async function loadCommentRepliesAsync(productId, commentId, page, loadBtn) {
      const response = await fetch(`/ProductComment/LoadReplies?productId=${productId}&commentId=${commentId}&page=${page}`);

      if (response.ok) {
        const data = await response.json();

        currentRepliesPage = page;

        loadBtn.insertAdjacentHTML(`beforebegin`, data.replies);
        formatLocalDates();
        loadBtn.classList.toggle(`hidden`, !data.hasMoreReplies);
      } else {
        pushNotification(`Грешка при зареждането на отговорите!`, `error`);
      }
    }
  }

  // --- Like button ---
  document.getElementById(`like-button`).addEventListener(`click`, async function (e) {
    const itemId = e.target.closest(`button`).dataset.productId;

    const response = await fetch(`/Product/ToggleLike/${itemId}`, {
      method: "POST",
      headers: {
        RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
      },
    });

    if (response.ok) {
      const isAdded = document.getElementById(`like-button`).classList.toggle(`liked`);

      pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
    } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
    else pushNotification(`Възникна неочаквана грешка!`, `error`);
  });
});

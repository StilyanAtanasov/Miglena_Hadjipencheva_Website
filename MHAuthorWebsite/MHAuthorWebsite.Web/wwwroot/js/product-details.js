"use strict";

import { initQuill } from "./editor.js";
import { pushNotification } from "./notification.js";
import { calculateStarsFill } from "./elements/stars.js";
import { openModal, replaceBody } from "./elements/modal.js";
import { reactToComment } from "./react-to-product-comment.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  // --- Comments ---
  let currentCommentsPage = 1;
  let currentRepliesPage = 1;
  let currentRatingFilter = null;
  const moreCommentsBtnEl = document.getElementById(`more-comments-btn`);
  const productId = moreCommentsBtnEl.dataset.productId;
  const commentsContainer = document.getElementById(`comments`);
  const noResultsContainer = document.getElementById(`no-comment-results`);

  // - Listeners -
  commentsContainer.addEventListener(`click`, function (e) {
    const loadRepliesBtn = e.target.closest(`.load-replies-btn`);
    if (loadRepliesBtn) loadCommentReplies(productId, loadRepliesBtn.dataset.commentId, currentRepliesPage + 1, loadRepliesBtn);

    const reactionBtn = e.target.closest(`.react-btn`);
    if (reactionBtn) reactToComment(reactionBtn);

    // Load comment details
    const imagesContainer = e.target.closest(`.comment .images`);
    if (imagesContainer) loadProductDetails(imagesContainer.dataset.commentId);
  });

  async function loadProductDetails(commentId) {
    const response = await fetch(`/ProductComment/Details?commentId=${commentId}`);

    if (response.ok) {
      const html = await response.text();
      replaceBody(html);
      openModal();
    } else {
      pushNotification(`Възникна грешка при зареждане на коментара!`, `error`);
    }
  }

  moreCommentsBtnEl.addEventListener(`click`, () => loadComments(productId, currentCommentsPage + 1, currentRatingFilter));

  // - Load comments -
  async function loadComments(productId, page, ratingFilter) {
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
      moreCommentsBtnEl.classList.toggle(`hidden`, !data.hasMoreComments);
      !ratingFilter && ratingBarElements.forEach(el => el.classList.remove(`faded`));

      if (!data.comments.replaceAll(`\r`, ``).replaceAll(`\n`, ``)) return noResultsContainer.classList.remove(`hidden`);
      calculateStarsFill();
    } else {
      pushNotification(`Грешка при зареждането на коментарите!`, `error`);
    }
  }

  // - Load comments with rating filter -
  // - Fill average rating stars -
  const ratingStatsSection = document.getElementById(`rating-stats`);
  const allRatingsCount = +ratingStatsSection.dataset.count;
  const ratingBarElements = ratingStatsSection.querySelectorAll(`.rating-bar`);

  ratingBarElements.forEach(b => {
    const ratingsCount = b.dataset.count;
    const rating = +b.dataset.rating;

    b.querySelector(`.bar-container .bar-fill`).style.width = `${(ratingsCount / allRatingsCount) * 100}%`;
    b.addEventListener(`click`, function () {
      if (currentRatingFilter == rating) return loadComments(productId, 1, null);

      loadComments(productId, 1, rating);
      ratingBarElements.forEach(el => el.classList.add(`faded`));
      b.classList.remove(`faded`);
    });
  });

  // - Load replies -
  async function loadCommentReplies(productId, commentId, page, loadBtn) {
    const response = await fetch(`/ProductComment/LoadReplies?productId=${productId}&commentId=${commentId}&page=${page}`);

    if (response.ok) {
      const data = await response.json();

      currentRepliesPage = page;

      loadBtn.insertAdjacentHTML(`beforebegin`, data.replies);
      loadBtn.classList.toggle(`hidden`, !data.hasMoreReplies);
    } else {
      pushNotification(`Грешка при зареждането на отговорите!`, `error`);
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
      const isAdded = e.target.classList.toggle(`liked`);

      pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
    } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
    else pushNotification(`Възникна неочаквана грешка!`, `error`);
  });
});

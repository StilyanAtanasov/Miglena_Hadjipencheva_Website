import { initQuill } from "./editor.js";
import { pushNotification } from "./notification.js";
import { calculateStarsFill } from "./elements/stars.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  // --- Comments ---
  const ratingStatsSection = document.getElementById(`rating-stats`);
  const ratingsCount = +ratingStatsSection.dataset.count;

  ratingStatsSection.querySelectorAll(`.rating-bar`).forEach(b => {
    const ratingCount = b.dataset.count;
    b.querySelector(`.bar-container .bar-fill`).style.width = `${(ratingCount / ratingsCount) * 100}%`;
  });

  let currentCommentsPage = 1;
  const moreCommentsBtnEl = document.getElementById(`more-comments-btn`);
  const productId = moreCommentsBtnEl.dataset.productId;

  moreCommentsBtnEl.addEventListener(`click`, async function () {
    const response = await fetch(`/ProductComment/LoadComments?productId=${productId}&page=${++currentCommentsPage}`, {
      method: "POST",
      headers: {
        RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
      },
    });

    if (response.ok) {
      const data = await response.json();
      document.getElementById(`comments`).insertAdjacentHTML(`beforeend`, data.comments);
      !data.hasMoreComments && (moreCommentsBtnEl.style.display = `none`);
      calculateStarsFill();
    } else {
      pushNotification(`Грешка при зареждането на коментарите!`, `error`);
    }
  });

  let currentRepliesPage = 1;
  document.querySelectorAll(`.load-replies-btn`).forEach(b =>
    b.addEventListener(`click`, async function () {
      const response = await fetch(`/ProductComment/LoadReplies?productId=${productId}&commentId=${b.dataset.commentId}&page=${++currentRepliesPage}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        const data = await response.json();
        b.insertAdjacentHTML(`beforebegin`, data.replies);
        !data.hasMoreReplies && (b.style.display = `none`);
      } else {
        pushNotification(`Грешка при зареждането на отговорите!`, `error`);
      }
    })
  );

  // --- Comment reactions ---
  document.querySelectorAll(`.comment-reactions button`).forEach(b => {
    b.addEventListener(`click`, async function () {
      const reactionsBox = b.closest(`.comment-reactions`);

      const reactionType = +b.dataset.reactionType;
      const commentId = reactionsBox.dataset.commentId;

      if ((!reactionType && reactionType !== 0) || !commentId) pushNotification(`Възникна неочаквана грешка!`, `error`);

      const response = await fetch(`/ProductComment/ReactToComment/`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
        body: JSON.stringify({ commentId, reactionType }),
      });

      if (response.ok) {
        const reactions = await response.json();
        reactions.forEach(r => {
          reactionsBox.querySelector(`[data-reaction-type="${r.reaction}"] .reaction-count`).textContent = r.count;
        });

        b.querySelector(`i`).classList.toggle(`fa-solid`);
        b.querySelector(`i`).classList.toggle(`fa-regular`);
        reactionsBox.querySelectorAll(`button i`).forEach(i => {
          if (!b.contains(i)) {
            i.classList.replace(`fa-solid`, `fa-regular`);
          }
        });
      } else if (response.status === 403) {
        pushNotification(`Не може да реагирате на свой коментар!`, `warning`);
      } else pushNotification(`Възникна неочаквана грешка!`, `error`);
    });
  });

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

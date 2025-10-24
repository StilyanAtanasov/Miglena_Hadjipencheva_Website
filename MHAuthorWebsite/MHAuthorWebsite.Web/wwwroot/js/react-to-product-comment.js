"use strict";

import { pushNotification } from "./notification.js";

export async function reactToComment(reactionBtn) {
  const reactionsBox = reactionBtn.closest(`.comment-reactions`);

  const reactionType = +reactionBtn.dataset.reactionType;
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

    reactionBtn.querySelector(`i`).classList.toggle(`fa-solid`);
    reactionBtn.querySelector(`i`).classList.toggle(`fa-regular`);
    reactionsBox.querySelectorAll(`button i`).forEach(i => {
      if (!reactionBtn.contains(i)) {
        i.classList.replace(`fa-solid`, `fa-regular`);
      }
    });
  } else if (response.status === 403) {
    pushNotification(`Не може да реагирате на свой коментар!`, `warning`);
  } else pushNotification(`Възникна неочаквана грешка!`, `error`);
}

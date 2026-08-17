"use strict";

import { pushNotification } from "./notification.js";
import { injectLoader } from "./elements/loader.js";

export async function reactToComment(reactionBtn) {
  if (reactionBtn.disabled) return;

  const reactionsBox = reactionBtn.closest(`.comment-reactions`);
  const reactionType = +reactionBtn.dataset.reactionType;
  const commentId = reactionsBox.dataset.commentId;

  if ((!reactionType && reactionType !== 0) || !commentId) {
    pushNotification(`Възникна неочаквана грешка!`, `error`);
    return;
  }

  reactionBtn.disabled = true;
  const buttonLoader = injectLoader(reactionBtn, { size: `small` });

  try {
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

      const icon = reactionBtn.querySelector(`i`);
      const wasActive = !icon.classList.contains(`regular`);

      reactionsBox.querySelectorAll(`button i`).forEach(i => i.classList.add(`regular`));
      icon.classList.toggle(`regular`, wasActive);
    } else if (response.status === 403) {
      pushNotification(`Не може да реагирате на свой коментар!`, `warning`);
    } else {
      pushNotification(`Възникна неочаквана грешка!`, `error`);
    }
  } catch {
    pushNotification(`Възникна неочаквана грешка!`, `error`);
  } finally {
    buttonLoader.close();
    reactionBtn.disabled = false;
  }
}

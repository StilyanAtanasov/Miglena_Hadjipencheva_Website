"use strict";

import { reactToComment } from "./react-to-product-comment.js";

window.addEventListener(`DOMContentLoaded`, function () {
  this.document.getElementById(`modal`).addEventListener(`click`, function (e) {
    const imagePreview = e.target.closest(`.preview-thumb`);
    if (imagePreview) {
      document.getElementById(`main-comment-image`).src = imagePreview.dataset.originalUrl;
      document.querySelectorAll(`.preview-thumb`).forEach(t => t.classList.remove(`active`));
      imagePreview.classList.add(`active`);
    }

    const reactionBtn = e.target.closest(`.react-btn`);
    if (reactionBtn) reactToComment(reactionBtn);
  });
});

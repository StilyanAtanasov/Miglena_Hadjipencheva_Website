"use strict";

document.addEventListener(`click`, e => {
  if (e.target.classList.contains(`preview-thumb`)) {
    const mainImage = document.getElementById(`main-comment-image`);
    mainImage.src = e.target.src;
  }
});

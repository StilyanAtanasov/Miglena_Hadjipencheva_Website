import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);
});

// --- Slider ---
let currentSliderState = 1;
const imagesCount = document.getElementById(`images-container`).dataset.imagesCount;
if (imagesCount > 1) {
  const handleDotsClick = e => e.target.classList.contains(`dots__dot`) && slideTo((currentSliderState = Number(e.target.dataset.slide)));

  function slideTo(slide, animate = true) {
    const slides = document.querySelectorAll(".slide");

    slides.forEach((s, i) => {
      if (!animate) {
        s.style.transition = "none";
      } else {
        s.style.transition = "";
      }

      s.style.transform = `translateX(${(slide - (i + 1)) * -100}%)`;
    });

    document.querySelectorAll(".dots__dot").forEach(d => d.classList.remove(`dots__dot--active`));
    document.querySelector(`.dots__dot[data-slide="${slide}"]`).classList.add(`dots__dot--active`);
  }

  function changeSliderState(changeBy) {
    currentSliderState = currentSliderState + changeBy;
    if (currentSliderState > imagesCount) currentSliderState = 1;
    else if (currentSliderState < 1) currentSliderState = imagesCount;

    return currentSliderState;
  }

  slideTo(1, false);

  // - Listeners
  document.getElementById(`slider__btn--left`).addEventListener(`click`, () => slideTo(changeSliderState(-1)));
  document.getElementById(`slider__btn--right`).addEventListener(`click`, () => slideTo(changeSliderState(1)));
  document.getElementById(`dots-box`).addEventListener(`click`, handleDotsClick);
}

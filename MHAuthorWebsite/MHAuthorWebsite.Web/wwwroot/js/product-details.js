import { initQuill } from "./editor.js";
import { pushNotification } from "./notification.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  document.querySelectorAll(`[data-role="remove-item"]`).forEach(b =>
    b.addEventListener(`click`, async function () {
      const itemId = b.dataset.itemId;

      const response = await fetch(`/Product/ToggleLike/${itemId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        const isAdded = b.classList.toggle(`liked`);

        pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
      } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
      else pushNotification(`Възникна неочаквана грешка!`, `error`);
    })
  );
});

// --- Slider ---
let currentSliderState = 1;
const imagesCount = document.getElementById(`images-container`).dataset.imagesCount;
if (imagesCount > 1) {
  const handleDotsClick = e => e.target.classList.contains(`dots__dot`) && slideTo((currentSliderState = Number(e.target.dataset.slide)));

  function slideTo(slide, animate = true) {
    const slides = document.querySelectorAll(".slide");

    slides.forEach(s => {
      if (!animate) {
        s.style.transition = "none";
      } else {
        s.style.transition = "";
      }

      s.style.transform = `translateX(${(slide - 1) * -100}%)`;
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

  // - Listeners
  document.getElementById(`slider__btn--left`).addEventListener(`click`, () => slideTo(changeSliderState(-1)));
  document.getElementById(`slider__btn--right`).addEventListener(`click`, () => slideTo(changeSliderState(1)));
  document.getElementById(`dots-box`).addEventListener(`click`, handleDotsClick);
}

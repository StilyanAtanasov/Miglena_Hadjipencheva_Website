document.addEventListener(`DOMContentLoaded`, async function () {
  let currentSliderState = 1;
  const slidesCount = +document.getElementById(`slider`).dataset.slidesCount;

  if (slidesCount > 1) {
    function createDots() {
      const dotsBox = document.getElementById(`dots-box`);
      for (let i = 0; i < slidesCount; i++) dotsBox.insertAdjacentHTML(`beforeend`, `<span class="dots__dot" data-slide="${i + 1}"></span>`);
    }

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
      if (currentSliderState > slidesCount) currentSliderState = 1;
      else if (currentSliderState < 1) currentSliderState = slidesCount;

      return currentSliderState;
    }

    createDots();
    slideTo(1);

    document.getElementById(`slider__btn--left`).addEventListener(`click`, () => slideTo(changeSliderState(-1)));
    document.getElementById(`slider__btn--right`).addEventListener(`click`, () => slideTo(changeSliderState(1)));
    document.getElementById(`dots-box`).addEventListener(`click`, handleDotsClick);
  }
});

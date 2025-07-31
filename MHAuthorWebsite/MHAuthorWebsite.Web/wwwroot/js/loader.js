const loaderElement = document.getElementById(`loader`);
const bodyElement = document.body;

function addLoader() {
  loaderElement.classList.remove(`hide`);
  loaderElement.classList.remove(`hidden`);
}

function removeLoader() {
  loaderElement.classList.add(`hide`);

  loaderElement.addEventListener(`animationend`, () => loaderElement.classList.add(`hidden`));
}

window.addEventListener(`load`, function () {
  bodyElement.classList.add(`animate`);
  removeLoader();
});

document.addEventListener(`DOMContentLoaded`, function () {
  const triggerLoadingUnlimitedEls = document.querySelectorAll(`[data-trigger="load-unlimited"]`);
  triggerLoadingUnlimitedEls.forEach(el => el.addEventListener(`click`, addLoader));
});

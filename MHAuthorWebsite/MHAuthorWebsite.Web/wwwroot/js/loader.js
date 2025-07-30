const loaderElement = document.getElementById(`loader`);

function addLoader() {
  loaderElement.classList.remove(`hide`);
  loaderElement.classList.remove(`hidden`);
}

function removeLoader() {
  loaderElement.classList.add(`hide`);

  loaderElement.addEventListener(`animationend`, () => loaderElement.classList.add(`hidden`));
}

window.addEventListener(`load`, removeLoader);

document.addEventListener(`DOMContentLoaded`, function () {
  const triggerLoadingUnlimitedEls = document.querySelectorAll(`[data-trigger="load-unlimited"]`);
  triggerLoadingUnlimitedEls.forEach(el => el.addEventListener(`click`, addLoader));
});

const loadingScreen = document.getElementById(`loading-screen`);
const bodyElement = document.body;

function addLoader() {
  loadingScreen.classList.remove(`hide`);
  loadingScreen.classList.remove(`hidden`);
}

function removeLoader() {
  loadingScreen.classList.add(`hide`);

  loadingScreen.addEventListener(`animationend`, () => loadingScreen.classList.add(`hidden`));
}

window.addEventListener(`load`, function () {
  bodyElement.classList.add(`animate`);
  removeLoader();
});

document.addEventListener(`DOMContentLoaded`, function () {
  const triggerLoadingUnlimitedEls = document.querySelectorAll(`[data-trigger="load-unlimited"]`);
  triggerLoadingUnlimitedEls.forEach(el => el.addEventListener(`click`, addLoader));
});

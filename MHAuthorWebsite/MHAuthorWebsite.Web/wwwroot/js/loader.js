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

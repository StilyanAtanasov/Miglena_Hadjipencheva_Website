"use strict";

const BUTTON_ID = `scroll-to-top-btn`;
const HEIGHT_THRESHOLD_MULTIPLIER = 1.5;

function getPageHeight() {
  return Math.max(document.body?.scrollHeight ?? 0, document.documentElement?.scrollHeight ?? 0);
}

function exceedsThreshold() {
  return getPageHeight() > window.innerHeight * HEIGHT_THRESHOLD_MULTIPLIER;
}

function isPastDisplayOffset() {
  return window.scrollY > window.innerHeight * HEIGHT_THRESHOLD_MULTIPLIER;
}

function createButton() {
  const button = document.createElement(`button`);
  button.id = BUTTON_ID;
  button.type = `button`;
  button.className = `scroll-to-top-btn`;
  button.setAttribute(`aria-label`, `Scroll to top`);
  button.innerHTML = `<span class="material-symbols-outlined" aria-hidden="true">arrow_upward</span>`;

  button.addEventListener(`click`, () => {
    window.scrollTo({ top: 0, behavior: `smooth` });
  });

  return button;
}

function ensureButton() {
  let button = document.getElementById(BUTTON_ID);

  if (!button) {
    button = createButton();
    document.body.appendChild(button);
  }

  return button;
}

function hideButton() {
  const button = document.getElementById(BUTTON_ID);
  button?.classList.remove(`is-visible`);
}

export function reloadScrollToTopAssessment() {
  if (!exceedsThreshold()) {
    hideButton();
    return;
  }

  const button = ensureButton();

  if (isPastDisplayOffset()) {
    button.classList.add(`is-visible`);
    return;
  }

  button.classList.remove(`is-visible`);
}

if (document.readyState === `loading`) {
  document.addEventListener(`DOMContentLoaded`, reloadScrollToTopAssessment);
} else {
  reloadScrollToTopAssessment();
}

window.addEventListener(`load`, reloadScrollToTopAssessment);
window.addEventListener(`resize`, reloadScrollToTopAssessment);
window.addEventListener(`scroll`, reloadScrollToTopAssessment);

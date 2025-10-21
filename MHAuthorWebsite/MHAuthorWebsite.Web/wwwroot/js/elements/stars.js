"use strict";

export const calculateStarsFill = () => document.querySelectorAll(`.stars`).forEach(s => (s.querySelector(`.stars-row.stars-filled`).style.width = `${+s.dataset.percent}%`));

document.addEventListener(`DOMContentLoaded`, () => calculateStarsFill());

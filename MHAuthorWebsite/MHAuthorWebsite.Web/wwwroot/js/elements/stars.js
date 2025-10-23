"use strict";

function distanceFromStarCenter(percent, totalStars = 5) {
  const step = 100 / totalStars;
  const centers = Array.from({ length: totalStars }, (_, i) => step / 2 + i * step);

  const closestCenter = centers.reduce((prev, curr) => (Math.abs(curr - percent) < Math.abs(prev - percent) ? curr : prev));

  const halfStep = step / 2;
  const distance = Math.abs(percent - closestCenter);
  const normalized = Math.min(distance / halfStep, 1); // cap at 1

  return normalized; // 0 = center, 1 = furthest
}

export const calculateStarsFill = () => {
  const starsContainers = document.querySelectorAll(`.stars`);

  starsContainers.forEach(s => {
    const percent = +s.dataset.percent;
    const gapRem = 0.5;
    const gapPx = parseFloat(getComputedStyle(s.querySelector(`.stars-row`)).gap);
    const totalWidth = s.offsetWidth;

    const distFromStarCenter = distanceFromStarCenter(percent, 5);
    const differencePercent = +((gapPx / totalWidth) * 200 * distFromStarCenter).toFixed(2);
    const finalPercent = distFromStarCenter < 0.9 ? (percent % 20 > 10 ? percent - differencePercent : percent + differencePercent) : percent;

    s.querySelector(`.stars-row.stars-filled`).style.width = `${finalPercent}%`;
  });
};

document.addEventListener(`DOMContentLoaded`, () => calculateStarsFill());

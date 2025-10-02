import { initQuill } from "./editor.js";
import { pushNotification } from "./notification.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  // --- Comments ---
  const ratingStatsSection = document.getElementById(`rating-stats`);
  const ratingsCount = +ratingStatsSection.dataset.count;

  ratingStatsSection.querySelectorAll(`.rating-bar`).forEach(b => {
    const ratingCount = b.dataset.count;
    b.querySelector(`.bar-container .bar-fill`).style.width = `${(ratingCount / ratingsCount) * 100}%`;
  });

  // --- Like button ---
  document.getElementById(`like-button`).addEventListener(`click`, async function (e) {
    const itemId = e.target.closest(`button`).dataset.productId;

    const response = await fetch(`/Product/ToggleLike/${itemId}`, {
      method: "POST",
      headers: {
        RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
      },
    });

    if (response.ok) {
      const isAdded = e.target.classList.toggle(`liked`);

      pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
    } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
    else pushNotification(`Възникна неочаквана грешка!`, `error`);
  });
});

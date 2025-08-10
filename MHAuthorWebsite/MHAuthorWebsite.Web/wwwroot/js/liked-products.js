"use strict";

import { pushNotification } from "./notification.js";

let count;

document.addEventListener(`DOMContentLoaded`, function () {
  const likedProducts = document.querySelectorAll(`[data-role="remove-item"]`);
  count = likedProducts.length;

  likedProducts.forEach(b =>
    b.addEventListener(`click`, async function () {
      const itemId = b.dataset.itemId;

      const response = await fetch(`/Product/ToggleLike/${itemId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        b.closest(`.product-card`).remove();
        const line = document.getElementById(`line-${itemId}`);
        if (line != null) line.remove();

        if (--count === 0) document.getElementById(`zero-liked-message`).classList.remove(`hidden`);

        pushNotification("Продуктът е премахнат успешно!", "success");
      } else pushNotification("Грешка при премахването на продукта!", "error");
    })
  );
});

"use strict";

import { pushNotification } from "./notification.js";

document.addEventListener(`DOMContentLoaded`, function () {
  document.querySelectorAll(`[data-role="remove-item"]`).forEach(b =>
    b.addEventListener(`click`, async function (e) {
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

        pushNotification("Продуктът е премахнат от успешно!", "success");
      } else pushNotification("Грешка при премахването на продукта!", "error");
    })
  );
});

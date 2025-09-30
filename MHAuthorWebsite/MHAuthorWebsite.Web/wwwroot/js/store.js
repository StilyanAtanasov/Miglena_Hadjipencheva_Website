"use strict";

import { pushNotification } from "./notification.js";

document.addEventListener(`DOMContentLoaded`, function () {
  document.getElementById(`order-by-select`).addEventListener(`change`, async function () {
    const orderType = this.value;
    window.location.href = `/Product/AllProducts?orderType=${orderType}`;
  });

  document.querySelectorAll(`[data-role="remove-item"]`).forEach(b =>
    b.addEventListener(`click`, async function () {
      const itemId = b.dataset.itemId;

      const response = await fetch(`/Product/ToggleLike/${itemId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });
      console.log(response);
      if (response.ok) {
        const isAdded = b.classList.toggle(`liked`);

        pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
      } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
      else pushNotification(`Възникна неочаквана грешка!`, `error`);
    })
  );
});

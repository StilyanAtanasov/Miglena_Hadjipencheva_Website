"use strict";

import { pushNotification } from "./notification.js";

document.querySelectorAll(`[data-role="add-to-cart"]`).forEach(b =>
  b.addEventListener(`click`, async function () {
    const itemId = b.dataset.itemId;

    let quantity = +document.getElementById(`quantity`).value;
    if (!quantity) quantity = 1;

    const response = await fetch(`/Cart/Add`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
      },
      body: JSON.stringify({
        productId: itemId,
        quantity,
      }),
    });

    if (response.ok) pushNotification(`Продуктът добавен в количката!`, `success`);
    else if (response.status === 401) pushNotification(`Влезте в системата, за да добавите продукта в количката!`, `warning`);
    else pushNotification(`Възникна неочаквана грешка!`, `error`);
  })
);

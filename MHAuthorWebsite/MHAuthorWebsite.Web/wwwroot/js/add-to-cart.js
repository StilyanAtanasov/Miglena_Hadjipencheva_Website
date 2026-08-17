"use strict";

import { pushNotification } from "./notification.js";
import { injectLoader } from "./elements/loader.js";

document.querySelectorAll(`[data-role="add-to-cart"]`).forEach(b =>
  b.addEventListener(`click`, async function () {
    if (b.disabled) return;

    const itemId = b.dataset.itemId;

    let quantity = +document.getElementById(`quantity`)?.value ?? 0;
    if (!quantity) quantity = 1;

    b.disabled = true;
    const buttonLoader = injectLoader(b, { size: `small` });

    try {
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
      else if (response.status === 400) pushNotification(Object.values(await response.json())[0], `warning`);
      else if (response.status === 401) pushNotification(`Влезте в системата, за да добавите продукта в количката!`, `warning`);
      else pushNotification(`Възникна неочаквана грешка!`, `error`);
    } finally {
      buttonLoader.close();
      b.disabled = false;
    }
  }),
);

"use strict";

import { pushNotification } from "./notification.js";

let productsCount;

document.addEventListener(`DOMContentLoaded`, function () {
  const totalPriceElement = document.querySelector(`#price-sum`);

  const quantityInputs = document.querySelectorAll(`[data-role="quantity-input"]`);
  productsCount = quantityInputs.length;

  quantityInputs.forEach(i =>
    i.addEventListener(`change`, async function () {
      const itemId = i.dataset.itemId;
      const quantity = parseInt(i.value);

      const response = await fetch(`Cart/UpdateQuantity`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
        body: JSON.stringify({ itemId, quantity }),
      });

      if (response.ok) {
        const data = await response.json();

        data.lineTotal != null && (document.querySelector(`#line-total-${itemId} .sum-price`).textContent = `${data.lineTotal}`);
        data.cartTotal != null && (totalPriceElement.textContent = `${data.cartTotal}`);
      } else alert(`Грешка при обновяване на количеството.`);
    })
  );

  document.querySelectorAll(`[data-role="remove-item"]`).forEach(b =>
    b.addEventListener(`click`, async function (e) {
      e.preventDefault();
      e.stopPropagation();

      const itemId = b.dataset.itemId;

      const response = await fetch(`Cart/Remove/${itemId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        let cartItemElement = b.closest(`tr`);
        if (cartItemElement != null) {
          const itemsSumPrice = +cartItemElement.querySelector(`.sum-price`).textContent.replace(`,`, `.`);
          totalPriceElement.textContent = (+totalPriceElement.textContent.replace(`,`, `.`) - itemsSumPrice).toFixed(2).replace(`.`, `,`);

          if (--productsCount === 0) {
            document.getElementById(`valid-items-section`).remove();
            document.getElementById(`cart-summary`).remove();
            document.getElementById(`no-products-message`).classList.remove(`hidden`);
          }
        } else cartItemElement = b.closest(`div`);

        cartItemElement.remove();

        pushNotification("Продуктът е премахнат от количката!", "success");
      } else pushNotification("Грешка при премахването на продукта!", "error");
    })
  );
});

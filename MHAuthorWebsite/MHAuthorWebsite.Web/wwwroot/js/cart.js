"use strict";

import { pushNotification } from "./notification.js";
import { formatBgNumber, parseBgNumber } from "./common.js";

let productsCount;
const levToEurRate = parseBgNumber(document.querySelector(`.page-wrapper`).dataset.levToEurRate);

document.addEventListener(`DOMContentLoaded`, function () {
  const totalPriceElement = document.querySelector(`#price-sum`);
  const totalPriceEurElement = document.querySelector(`#price-sum-eur`);

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

        document.querySelector(`#line-total-${itemId} .sum-price`).textContent = `${data.lineTotal}`;
        document.querySelector(`#line-total-${itemId} .sum-price-eur`).textContent = `${formatBgNumber(parseBgNumber(data.lineTotal) * levToEurRate)}`;

        totalPriceElement.textContent = `${data.cartTotal}`;
        totalPriceEurElement.textContent = `${formatBgNumber(parseBgNumber(data.cartTotal) * levToEurRate)}`;
      } else alert(`Грешка при обновяване на количеството.`);
    })
  );

  document.querySelectorAll(`[data-role="is-selected-input"]`).forEach(i =>
    i.addEventListener(`change`, async function () {
      const itemId = i.dataset.itemId;
      const isSelected = i.checked;

      const response = await fetch(`Cart/UpdateIsSelected`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
        body: JSON.stringify({ itemId, isSelected }),
      });

      if (response.ok) {
        const selectedItems = [...document.querySelectorAll(`tbody tr`)].filter(i => i.querySelector(`[data-role="is-selected-input"]`).checked === true);

        const newPriceLev = selectedItems.reduce((partialSum, i) => partialSum + parseFloat(i.querySelector(`.sum-price`).textContent), 0);

        totalPriceElement.textContent = formatBgNumber(newPriceLev);
        totalPriceEurElement.textContent = `${formatBgNumber(newPriceLev * levToEurRate)}`;
      } else alert(`Грешка при обновяване на селектираните продукти!`);
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
          const itemsSumPrice = parseBgNumber(cartItemElement.querySelector(`.sum-price`).textContent);
          totalPriceElement.textContent = formatBgNumber(parseBgNumber(totalPriceElement.textContent) - itemsSumPrice);
          totalPriceEurElement.textContent = `${formatBgNumber(parseBgNumber(totalPriceElement.textContent) * levToEurRate)}`;

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

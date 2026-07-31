"use strict";

import { pushNotification } from "./notification.js";
import { formatBgNumber, parseBgNumber } from "./common.js";
import { calcFreeDelivery } from "./elements/free-delivery.js";

let productsCount;
const eurToLevRate = parseBgNumber(document.querySelector(`.page-wrapper`).dataset.eurToLevRate);
const freeShippingThresholdEur = parseBgNumber(document.querySelector(`.page-wrapper`).dataset.freeShippingThresholdEur);

document.addEventListener(`DOMContentLoaded`, function () {
  const grandTotalPriceElement = document.querySelector(`#grand-total`);
  const grandTotalPriceBgnElement = document.querySelector(`#grand-total-bgn`);
  const totalPriceElement = document.querySelector(`#total`);
  const totalPriceBgnElement = document.querySelector(`#total-bgn`);
  const discountElement = document.querySelector(`#discount-global`);
  const discountBgnElement = document.querySelector(`#discount-global-bgn`);

  window.addEventListener(`DOMContentLoaded`, () => setTimeout(() => calcFreeDelivery(parseBgNumber(grandTotalPriceElement.textContent), freeShippingThresholdEur), 350));

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
        document.querySelector(`#line-total-${itemId} .sum-price-bgn`).textContent = `${formatBgNumber(parseBgNumber(data.lineTotal) * eurToLevRate)}`;

        updateCartSummary(data.cartTotal);
      } else if (response.status === 400) pushNotification(Object.values(await response.json())[0], `warning`);
      else alert(`Грешка при обновяване на количеството.`);
    }),
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

      if (response.ok) updateCartSummary();
      else alert(`Грешка при обновяване на селектираните продукти!`);
    }),
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
          if (--productsCount === 0) {
            document.getElementById(`valid-items-section`).remove();
            document.getElementById(`cart-summary`).remove();
            document.getElementById(`no-products-message`).classList.remove(`hidden`);
          }
        } else cartItemElement = b.closest(`div`);

        cartItemElement.remove();

        updateCartSummary();

        pushNotification(`Продуктът е премахнат от количката!`, `success`);
      } else pushNotification(`Грешка при премахването на продукта!`, `error`);
    }),
  );

  function updateCartSummary(cartTotal) {
    const selectedItems = [...document.querySelectorAll(`tbody tr`)].filter(i => i.querySelector(`[data-role="is-selected-input"]`).checked === true);

    const newPriceEur = cartTotal == null ? selectedItems.reduce((partialSum, i) => partialSum + parseBgNumber(i.querySelector(`.sum-price`).textContent), 0) : parseBgNumber(cartTotal);
    grandTotalPriceElement.textContent = formatBgNumber(newPriceEur);
    grandTotalPriceBgnElement.textContent = `${formatBgNumber(newPriceEur * eurToLevRate)}`;

    const oldPriceEur = selectedItems.reduce(
      (partialSum, i) => partialSum + parseBgNumber(i.querySelector(`.unit-price:not(.discounted-price) .unit-price-value`).textContent) * i.querySelector(`.quantity-input .input`).value,
      0,
    );

    totalPriceElement.textContent = formatBgNumber(oldPriceEur);
    totalPriceBgnElement.textContent = `${formatBgNumber(oldPriceEur * eurToLevRate)}`;

    const discount = oldPriceEur - newPriceEur;
    discountElement.textContent = formatBgNumber(discount);
    discountBgnElement.textContent = `${formatBgNumber(discount * eurToLevRate)}`;

    calcFreeDelivery(newPriceEur, freeShippingThresholdEur);
  }
});

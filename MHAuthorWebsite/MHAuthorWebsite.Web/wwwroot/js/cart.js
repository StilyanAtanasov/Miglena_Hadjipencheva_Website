"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  const totalPriceElement = document.querySelector(`#price-sum`);

  const inputs = document.querySelectorAll(`[data-role="quantity-input"]`);
  inputs.forEach(i =>
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

        data.lineTotal != null && (document.querySelector(`#line-total-${itemId}`).textContent = `${data.lineTotal} лв.`);
        data.cartTotal != null && (totalPriceElement.textContent = `${data.cartTotal} лв.`);
      } else alert(`Грешка при обновяване на количеството.`);
    })
  );

  const deleteButtons = document.querySelectorAll(`[data-role="remove-item"]`);
  deleteButtons.forEach(b =>
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
        const cartItemElement = b.closest(`tr`);
        const itemsSumPrice = +cartItemElement.querySelector(`.sum-price`).textContent.replace(`,`, `.`);

        totalPriceElement.textContent = (+totalPriceElement.textContent.replace(`,`, `.`) - itemsSumPrice).toFixed(2).replace(`.`, `,`);

        cartItemElement.remove();
      } else alert(`Грешка при премахването на продукта от количката!`);
    })
  );
});

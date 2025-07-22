"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  const inputs = document.querySelectorAll(`.quantity-input`);
  inputs.forEach((i) =>
    i.addEventListener(`change`, async function () {
      const itemId = i.dataset.itemId;
      const quantity = parseInt(i.value);

      const response = await fetch(`Cart/UpdateQuantity`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector(
            'input[name="__RequestVerificationToken"]'
          ).value,
        },
        body: JSON.stringify({ itemId, quantity }),
      });

      if (response.ok) {
        const data = await response.json();

        data.lineTotal != null &&
          (document.querySelector(
            `#line-total-${itemId}`
          ).textContent = `${data.lineTotal} лв.`);
        data.cartTotal != null &&
          (document.querySelector(
            `#price-sum`
          ).textContent = `${data.cartTotal} лв.`);
      } else alert(`Грешка при обновяване на количеството.`);
    })
  );
});

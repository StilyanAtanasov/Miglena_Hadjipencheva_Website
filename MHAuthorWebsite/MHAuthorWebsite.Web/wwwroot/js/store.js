"use strict";

new TomSelect("#order-by-select", {
  create: false,
  plugins: ["dropdown_input"],
});

document
  .getElementById(`order-by-select`)
  .addEventListener(`change`, async function () {
    const orderType = this.value;
    window.location.href = `/Product/AllProducts?orderType=${orderType}`;
  });

"use strict";

import { pushNotification } from "./notification.js";

const form = document.getElementById(`confirm-form`);
const currency = form.dataset.currency || `BGN`;
const frameUrl = form.dataset.econtCalcUrl;
const econtFrame = document.getElementById(`econt-frame`);
const customerInfoIdInput = document.getElementById(`customerInfoId`);
const shippingPriceInput = document.getElementById(`shippingPrice`);
const shippingCurrencyInput = document.getElementById(`shippingCurrency`);
const subtotalEl = document.getElementById(`subtotal`);
const shippingEl = document.getElementById(`shipping`);
const grandEl = document.getElementById(`grand`);

function setIframeSrc() {
  const url = new URL(frameUrl);

  url.searchParams.set(`id_shop`, form.dataset.shopId);
  url.searchParams.set(`order_currency`, currency);
  url.searchParams.set(`order_total`, grandEl.textContent);
  url.searchParams.set(`order_weight`, form.dataset.totalWeight);
  url.searchParams.set(`customer_company`, form.dataset.userName);
  url.searchParams.set(`customer_name`, form.dataset.userName);
  url.searchParams.set(`customer_email`, form.dataset.userEmail);
  url.searchParams.set(`customer_phone`, form.dataset.userPhone);

  econtFrame.src = url.toString();
}

function parseCartSubtotal() {
  const rows = Array.from(document.querySelectorAll(`#cart-body tr`));
  const sum = rows.reduce((acc, tr) => acc + Number(parseFloat(tr.dataset.totalPrice, 0).toFixed(2)), 0);
  return Number.isFinite(sum) ? sum : 0;
}

function updateTotals() {
  const sub = parseCartSubtotal();
  const ship = Number(shippingPriceInput.value || 0);
  subtotalEl.textContent = sub.toFixed(2);
  shippingEl.textContent = ship.toFixed(2);
  grandEl.textContent = (sub + ship).toFixed(2);
}

window.addEventListener(
  `message`,
  function (message) {
    const data = message && message.data ? message.data : null;
    if (!data) return;

    if (data.shipment_error && data.shipment_error !== ``) {
      pushNotification(`Грешка при изчесляването на цената за доставка. Моля опитайте по-късно!`, `error`);
      return;
    }

    const price = data.shipping_price_cod;
    shippingPriceInput.value = Number(price || 0).toFixed(2);
    shippingCurrencyInput.value = data.shipping_price_currency || currency;
    customerInfoIdInput.value = data.id || ``;

    updateTotals();
  },
  false
);

form.addEventListener(`submit`, function (e) {
  if (!customerInfoIdInput.value) {
    e.preventDefault();
    pushNotification(`Моля попълнете формата за доставка.`, `warning`);
    return;
  }
  updateTotals();
});

setIframeSrc();

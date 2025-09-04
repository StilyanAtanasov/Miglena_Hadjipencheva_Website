"use strict";

import { pushNotification } from "./notification.js";

const form = document.getElementById(`confirm-form`);
const currency = form.dataset.currency || `BGN`;
const frameUrl = form.dataset.econtCalcUrl;
const econtFrame = document.getElementById(`econt-frame`);
const subtotalEl = document.getElementById(`subtotal`);
const shippingEl = document.getElementById(`shipping`);
const grandEl = document.getElementById(`grand`);

class EcontDeliveryDetails {
  constructor(data = {}) {
    this.id = data.id || null;
    this.name = data.name || null;
    this.face = data.face || null;
    this.phone = data.phone || null;
    this.email = data.email || null;
    this.countryCode = data.id_country || null;
    this.cityName = data.city_name || null;
    this.postCode = data.post_code || null;
    this.officeCode = data.office_code || null;
    this.zipCode = data.zip || null;
    this.address = data.address || null;
    this.priorityFrom = data.priority_from || null;
    this.priorityTo = data.priority_to || null;
    this.shippingPrice = Number(data.shipping_price_cod || 0);
  }
}

let econtDeliveryDetails = null;

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
  const ship = Number(econtDeliveryDetails?.shippingPrice || 0);
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

    econtDeliveryDetails = new EcontDeliveryDetails(data);
    updateTotals();
  },
  false
);

form.addEventListener(`submit`, async function (e) {
  e.preventDefault();

  if (!econtDeliveryDetails) {
    pushNotification(`Моля попълнете формата за доставка.`, `warning`);
    return;
  }

  const payload = JSON.stringify(econtDeliveryDetails);

  const response = await fetch(form.action, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
    },
    body: payload,
  });

  if (response.ok) {
    pushNotification(`Поръчката Ви е успешно приета и се обработва!`, `success`);
  } else {
    pushNotification(`Грешка при създаването на поръчка!`, `error`);
  }
});

setIframeSrc();

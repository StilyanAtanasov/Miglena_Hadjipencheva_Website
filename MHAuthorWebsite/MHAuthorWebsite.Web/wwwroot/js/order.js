"use strict";

import { pushNotification } from "./notification.js";
import { formatBgNumber, parseBgNumber } from "./common.js";
import { calcFreeDelivery } from "./elements/free-delivery.js";
import { injectLoader } from "./elements/loader.js";

const form = document.getElementById(`confirm-form`);
const currency = form.dataset.currency || `EUR`;
const frameUrl = form.dataset.econtCalcUrl;
const econtFrame = document.getElementById(`econt-frame`);
const subtotal = parseBgNumber(document.getElementById(`subtotal`).textContent);
const subtotalBgn = parseBgNumber(document.getElementById(`subtotal-bgn`).textContent);
const discount = parseBgNumber(document.getElementById(`discount`).textContent);
const discountBgn = parseBgNumber(document.getElementById(`discount-bgn`).textContent);
const grandEl = document.getElementById(`grand`);
const grandBgnEl = document.getElementById(`grand-bgn`);
const eurToLevRate = parseBgNumber(document.querySelector(`.page-wrapper`).dataset.eurToLevRate);
const freeShippingThresholdEur = parseBgNumber(document.querySelector(`.page-wrapper`).dataset.freeShippingThresholdEur);
const shippingPricesEl = document.getElementById(`shipping-prices`);
const placeOrderBtn = document.getElementById(`placeOrderBtn`);
let isSubmitting = false;

window.addEventListener(`DOMContentLoaded`, () => setTimeout(() => calcFreeDelivery(parseBgNumber(grandEl.textContent), freeShippingThresholdEur), 700));

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
    this.shippingPrice = parseBgNumber(data.shipping_price_cod || 0);
  }
}

let econtDeliveryDetails = null;

function setIframeSrc() {
  const url = new URL(frameUrl);

  url.searchParams.set(`id_shop`, form.dataset.shopId);
  url.searchParams.set(`order_currency`, currency);
  url.searchParams.set(`order_total`, parseBgNumber(grandEl.textContent));
  url.searchParams.set(`order_weight`, parseBgNumber(form.dataset.totalWeight));
  url.searchParams.set(`customer_company`, form.dataset.userName);
  url.searchParams.set(`customer_name`, form.dataset.userName);
  url.searchParams.set(`customer_email`, form.dataset.userEmail);
  url.searchParams.set(`customer_phone`, form.dataset.userPhone);

  econtFrame.src = url.toString();
}

function updateTotals() {
  const shippingEur = parseBgNumber(econtDeliveryDetails?.shippingPrice || 0);
  const shippingBgn = shippingEur * eurToLevRate;

  const orderSubtotalEur = parseBgNumber(grandEl.textContent);
  if (econtDeliveryDetails && orderSubtotalEur >= freeShippingThresholdEur && econtDeliveryDetails.shippingPrice != 0) {
    pushNotification(`Грешка при изчисляването на цената! Моля, опитайте по-късно!`, `error`);
    return;
  }

  if (orderSubtotalEur >= freeShippingThresholdEur && econtDeliveryDetails.shippingPrice == 0) {
    shippingPricesEl.style.color = "var(--color-success)";
    shippingPricesEl.innerHTML = `БЕЗПЛАТНО`;
  } else {
    shippingPricesEl.innerHTML = `
    <span id="shipping">${formatBgNumber(shippingEur)}</span>
    <span>€ / </span>
    <span id="shipping-bgn">${formatBgNumber(shippingBgn)}</span>
    <span> лв.</span>`;
  }

  grandEl.textContent = formatBgNumber(subtotal - discount + shippingEur);
  grandBgnEl.textContent = formatBgNumber(subtotalBgn - discountBgn + shippingBgn);
}

window.addEventListener(
  `message`,
  function (message) {
    const data = message && message.data ? message.data : null;
    if (!data) return;

    if (data.shipment_error && data.shipment_error !== ``) {
      pushNotification(`Грешка при изчисляването на цената за доставка. Моля опитайте по-късно!`, `error`);
      return;
    }

    econtDeliveryDetails = new EcontDeliveryDetails(data);
    updateTotals();
  },
  false,
);

form.addEventListener(`submit`, async function (e) {
  e.preventDefault();

  if (isSubmitting) return;

  if (!econtDeliveryDetails) {
    pushNotification(`Моля попълнете формата за доставка.`, `warning`);
    return;
  }

  isSubmitting = true;
  placeOrderBtn.disabled = true;
  const buttonLoader = injectLoader(placeOrderBtn, { size: `small` });

  try {
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
    const orderId = await response.json();
    window.location = `/Order/OrderAccepted?orderId=${orderId}`;
  } else {
    pushNotification(`Грешка при създаването на поръчка!`, `error`);
  }
  } catch {
    pushNotification(`Възникна грешка при създаването на поръчката!`, `error`);
  } finally {
    buttonLoader.close();
    placeOrderBtn.disabled = false;
    isSubmitting = false;
  }
});

setIframeSrc();

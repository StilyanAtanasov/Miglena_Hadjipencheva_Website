const freeDeliverySectionEl = document.getElementById(`free-delivery-section`);
const message = freeDeliverySectionEl.querySelector(`.message`);
const freeDeliveryBarFill = document.getElementById(`free-delivery-bar-fill`);

export function calcFreeDelivery(orderTotalEur, freeDeliveryTresholdEur) {
  const fillPercent = +((+orderTotalEur / +freeDeliveryTresholdEur) * 100).toFixed(2);
  const isEligible = fillPercent >= 100;

  if (isEligible) message.innerHTML = `<i class="fa-solid fa-clipboard-check heading"></i> БЕЗПЛАТНА ДОСТАВКА`;
  else message.innerHTML = `<i class="fa-solid fa-clipboard-list heading"></i> Още ${(freeDeliveryTresholdEur - orderTotalEur).toFixed(2)} евро до безплатна доставка`;

  message.classList.toggle(`success`, isEligible);
  freeDeliveryBarFill.style.width = `${Math.min(fillPercent, 100)}%`;
}

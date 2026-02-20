"use strict";

import { pushNotification } from "../notification.js";

document.addEventListener(`DOMContentLoaded`, function () {
  const form = document.getElementById(`email-preferences-form`);
  const tokenInput = form ? form.querySelector(`input[name="__RequestVerificationToken"]`) : null;
  const toggleNodes = document.querySelectorAll(`.js-preference-toggle`);

  if (!form || !tokenInput || toggleNodes.length === 0) return;

  toggleNodes.forEach(toggleNode => {
    toggleNode.addEventListener(`change`, async function () {
      const toggle = this;
      const previousValue = !toggle.checked;
      const handler = toggle.dataset.handler;
      const type = toggle.dataset.notificationType;
      const label = toggle.closest(`.email-preference-row`)?.querySelector(`.js-preference-label`);

      if (!handler || !label) return;

      toggle.disabled = true;

      try {
        const payload = new URLSearchParams();
        payload.append(`isEnabled`, String(toggle.checked));
        if (type) payload.append(`notificationType`, type);

        const response = await fetch(`?handler=${handler}`, {
          method: `POST`,
          headers: {
            "Content-Type": `application/x-www-form-urlencoded; charset=UTF-8`,
            RequestVerificationToken: tokenInput.value,
          },
          body: payload.toString(),
        });

        const result = await response.json();
        if (!response.ok || !result.success) {
          throw new Error(result.message || `Възникна грешка при запазване на предпочитанията.`);
        }

        label.textContent = result.isEnabled ? `Активиран` : `Спрян`;
        pushNotification(result.message, `success`);
      } catch (error) {
        toggle.checked = previousValue;
        label.textContent = previousValue ? `Активиран` : `Спрян`;
        pushNotification(error.message || `Възникна грешка при запазване на предпочитанията.`, `error`);
      } finally {
        toggle.disabled = false;
      }
    });
  });
});

"use strict";

import { pushNotification } from "./notification.js";

const contactForm = document.getElementById(`contact-form`);

contactForm.addEventListener(`submit`, async function (e) {
  e.preventDefault();

  if (!$(contactForm).valid()) return;

  const result = await fetch(contactForm.getAttribute(`action`), {
    method: `POST`,
    headers: {
      RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
    },
    body: new FormData(contactForm),
  });

  if (result.ok) {
    contactForm.reset();
    pushNotification(`Съобщението е изпратено успешно!`);
  } else pushNotification(`Възникна грешка. Опитайте отново.`, `error`);
});

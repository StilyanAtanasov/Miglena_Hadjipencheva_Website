"use strict";

import { pushNotification } from "./notification.js";

document.getElementById(`sendBtn`).addEventListener(`click`, async () => {
  const nameEl = document.getElementById(`name`);
  const emailEl = document.getElementById(`email`);
  const subjectEl = document.getElementById(`subject`);
  const messageEl = document.getElementById(`message`);

  const result = await fetch(`/Contacts/SendEmail`, {
    method: `POST`,
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
    },
    body: JSON.stringify({
      name: nameEl.value,
      email: emailEl.value,
      subject: subjectEl.value,
      message: messageEl.value,
    }),
  });

  if (result.ok) {
    nameEl.value = emailEl.value = subjectEl.value = messageEl.value = ``;
    pushNotification(`Съобщението е изпратено успешно!`);
  } else pushNotification(`Възникна грешка. Опитайте отново.`, `error`);
});

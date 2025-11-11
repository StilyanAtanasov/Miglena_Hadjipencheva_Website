"use strict";

import { pushNotification } from "../notification.js";

document.querySelectorAll(`[data-action="assign"]`).forEach(b => {
  b.addEventListener(`click`, async function () {
    const roleName = b.dataset.roleName;
    const userId = b.dataset.userId;

    const response = await fetch(`/Admin/AdminUserManagement/AssignRole?userId=${userId}&roleName=${roleName}`, {
      method: "POST",
      headers: {
        RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
      },
    });

    response.ok ? pushNotification(`Ролята е добавена към потребителя`) : pushNotification(`Възникна грешка!`, `error`);
  });
});

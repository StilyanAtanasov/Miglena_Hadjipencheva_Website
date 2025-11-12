"use strict";

import { pushNotification, showPopupAsync } from "../notification.js";
import { openModal, replaceBody } from "../elements/modal.js";

document.addEventListener("DOMContentLoaded", function () {
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

  async function assignRole(userId, roleName) {
    const response = await fetch(`/Admin/AdminUserManagement/AssignRole`, {
      method: "POST",
      headers: {
        RequestVerificationToken: token,
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: new URLSearchParams({ userId, roleName }),
    });

    response.ok ? pushNotification(`Ролята е успешно добавена`) : pushNotification(`Възникна грешка!`, `error`);
  }

  async function toggleBan(userId, button) {
    const response = await fetch(`/Admin/AdminUserManagement/ToggleIsBanned`, {
      method: "POST",
      headers: {
        RequestVerificationToken: token,
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: new URLSearchParams({ userId }),
    });

    if (response.ok) {
      const isBanned = await response.json();
      button.textContent = isBanned ? "Unban" : "Ban";
      pushNotification(isBanned ? `Потребителят е блокиран` : `Потребителят е деблокиран`);
    } else {
      pushNotification(`Възникна грешка!`, `error`);
    }
  }

  async function openUserDetails(userId) {
    const response = await fetch(`/Admin/AdminUserManagement/UserDetails?userId=${userId}`, {
      method: "GET",
      headers: { RequestVerificationToken: token },
    });

    if (!response.ok) {
      pushNotification(`Възникна грешка при зареждане на детайлите`, "error");
      return;
    }

    const html = await response.text();
    openModal();
    replaceBody(html);
  }

  // --- Assign admin role with confirmation ---
  document.querySelectorAll(`[data-action="assign"]`).forEach(b => {
    b.addEventListener("click", async function () {
      const userId = b.dataset.userId;
      const roleName = b.dataset.roleName;

      await showPopupAsync({
        icon: `warning`,
        title: `Добавяне на роля`,
        text: `Сигурни ли сте, че искате да дадете роля Admin на този потребител?`,
        showCancelButton: true,
        confirmButtonColor: `#3085d6`,
        cancelButtonColor: `#d33`,
        onConfirm: assignRole,
        onConfirmArgs: [userId, roleName],
      });
    });
  });

  // --- Toggle Ban / Unban with confirmation ---
  document.querySelectorAll(`[data-action="ban"]`).forEach(b => {
    b.addEventListener("click", async function () {
      const userId = b.dataset.userId;

      await showPopupAsync({
        icon: `warning`,
        title: `Блокиране на потребител`,
        text: `Сигурни ли сте, че искате да промените статуса на блокиране?`,
        showCancelButton: true,
        confirmButtonColor: `#3085d6`,
        cancelButtonColor: `#d33`,
        onConfirm: toggleBan,
        onConfirmArgs: [userId, b],
      });
    });
  });

  // --- View details (modal) ---
  document.querySelectorAll(`[data-action="details"]`).forEach(b => {
    b.addEventListener(`click`, async function () {
      const userId = b.dataset.userId;
      await openUserDetails(userId);
    });
  });
});

"use strict";

import { pushNotification, showPopupAsync } from "../notification.js";
import { openModal, replaceBody } from "../elements/modal.js";
import { formatLocalDates } from "../time-zone-manager.js";
import { SearchBarHandler } from "../elements/search-bar.js";

document.addEventListener("DOMContentLoaded", function () {
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

  new SearchBarHandler({
    inputSelector: "#user-search-input",
    formSelector: "#user-search-form",
    targetSelector: "#users-body",
    url: "/Admin/AdminUserManagement/ManageUsers",
    param: "search",
    debounceTimeoutMilliseconds: 500,
  });

  async function assignRole(userId, roleName, button) {
    const response = await fetch(`/Admin/AdminUserManagement/AssignRole`, {
      method: "POST",
      headers: {
        RequestVerificationToken: token,
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: new URLSearchParams({ userId, roleName }),
    });

    if (response.ok) {
      const tr = button.closest(`tr`);

      tr.querySelector(`.name-row .name`).insertAdjacentHTML(`afterend`, `<span class="badge admin-badge flex-row" title="Администратор"><i class="fa-regular fa-user-shield"></i> Админ</span>`);

      tr.querySelector(`.action-btns [data-action="assign"]`).remove();
      tr.querySelector(`.action-btns [data-action="ban"]`).remove();

      await showPopupAsync({
        icon: `success`,
        title: `Готово!`,
        text: `Успешно зададохте този потребител като администратор!`,
        confirmButtonColor: `rgb(58, 5, 58)`,
      });
    } else
      await showPopupAsync({
        icon: `error`,
        title: `Грешка!`,
        text: `Възникна неочаквана грешка! Моля, свържете се със системния администратор!`,
        confirmButtonColor: `rgb(58, 5, 58)`,
      });
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
      button.innerHTML = !isBanned ? `<i class="fa-regular fa-user-lock"></i> Блокирай` : `<i class="fa-regular fa-user-unlock"></i> Деблокирай`;

      if (isBanned) {
        const nameRow = button.closest(`tr`).querySelector(`.name-row`);
        nameRow.innerHTML = nameRow.innerHTML + `<span class="badge banned-badge flex-row" title="Блокиран потребител"><i class="fa-regular fa-user-lock"></i> Блокиран</span>`;
      } else button.closest(`tr`).querySelector(`.name-row .badge.banned-badge`).remove();

      await showPopupAsync({
        icon: `success`,
        title: `Готово!`,
        text: `Успешно ${isBanned ? "блокирахте" : "деблокирахте"} този потребител!`,
        confirmButtonColor: `rgb(58, 5, 58)`,
      });
    } else
      await showPopupAsync({
        icon: `error`,
        title: `Грешка!`,
        text: `Възникна неочаквана грешка! Моля, свържете се със системния администратор!`,
        confirmButtonColor: `rgb(58, 5, 58)`,
      });
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
    replaceBody(html);
    formatLocalDates();
    openModal();
  }

  // --- Assign admin role with confirmation ---
  document.querySelectorAll(`[data-action="assign"]`).forEach(b => {
    b.addEventListener("click", async function () {
      const userId = b.dataset.userId;
      const roleName = b.dataset.roleName;

      await showPopupAsync({
        icon: `warning`,
        title: `Добавяне на роля`,
        text: `Сигурни ли сте, че искате да дадете роля Admin на този потребител? Действието е необратимо!`,
        showCancelButton: true,
        confirmButtonColor: `rgb(39, 103, 231)`,
        cancelButtonColor: `rgb(255, 73, 73)`,
        onConfirm: assignRole,
        onConfirmArgs: [userId, roleName, b],
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
        confirmButtonColor: `rgb(39, 103, 231)`,
        cancelButtonColor: `rgb(255, 73, 73)`,
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

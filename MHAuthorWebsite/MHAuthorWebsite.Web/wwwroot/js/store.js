"use strict";

import { pushNotification } from "./notification.js";
import { SearchBarHandler } from "./elements/search-bar.js";
import { PaginationHandler } from "./elements/pagination-handler.js";

document.addEventListener(`DOMContentLoaded`, function () {
  const searchForm = document.querySelector("#product-search-form");

  if (searchForm) {
    new SearchBarHandler({
      inputSelector: "#product-search-input",
      formSelector: "#product-search-form",
      targetSelector: "#products-page",
      url: "/Product/AllProducts",
      param: "search",
      debounceTimeoutMilliseconds: 500,
      resetParams: [`page`],
    });
  }

  const paginationContainer = document.querySelector("#products-page");
  if (paginationContainer) {
    new PaginationHandler({
      containerSelector: "#products-page",
      url: "/Product/AllProducts",
    });
  }

  document.getElementById(`order-by-select`)?.addEventListener(`change`, async function () {
    const orderType = this.value;

    const currentUrlParams = new URLSearchParams(window.location.search);
    currentUrlParams.set(`orderType`, orderType);
    currentUrlParams.set(`page`, 1);

    window.location.href = `/Product/AllProducts?${currentUrlParams}`;
  });

  document.querySelectorAll(`[data-role="like-item"]`).forEach(b =>
    b.addEventListener(`click`, async function () {
      const itemId = b.dataset.itemId;

      const response = await fetch(`/Product/ToggleLike/${itemId}`, {
        method: "POST",
        headers: {
          RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
      });

      if (response.ok) {
        const isAdded = b.classList.toggle(`liked`);

        pushNotification(isAdded ? `Продуктът е харесан успешно!` : `Продуктът е премахнат от харесани!`, `success`);
      } else if (response.status === 401) pushNotification(`Влезте в системата, за да харесате продукт!`, `warning`);
      else pushNotification(`Възникна неочаквана грешка!`, `error`);
    }),
  );
});

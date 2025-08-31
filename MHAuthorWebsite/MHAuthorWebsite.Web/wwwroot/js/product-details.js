import { initQuill } from "./editor.js";
import { pushNotification } from "./notification.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);

  document.querySelectorAll(`[data-role="remove-item"]`).forEach(b =>
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
      } else if (response.status === 401) pushNotification(`Взете в системата, за да харесате продукт!`, `warning`);
      else pushNotification(`Възникна неочаквана грешка!`, `error`);
    })
  );
});

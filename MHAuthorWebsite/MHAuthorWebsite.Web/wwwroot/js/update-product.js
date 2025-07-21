document.addEventListener(`DOMContentLoaded`, function () {
  const descriptionInput = document.querySelector(`#descriptionInput`);

  // --- Editor logic ---
  class Counter {
    constructor(quill, options) {
      const container = document.querySelector(options.container);
      quill.on(Quill.events.TEXT_CHANGE, () => {
        container.innerText = this.calculate();
      });
    }

    calculate() {
      const text = quill.getText().trim();
      const words = text.split(/\s+/).length;
      const chars = text.length;

      return `${words} дум${words === 1 ? "а" : "и"}, ${chars} символ${chars === 1 ? "" : "а"}!`;
    }
  }

  Quill.register("modules/counter", Counter);

  const quill = new Quill("#description-editor", {
    theme: "snow",
    modules: {
      counter: {
        container: "#counter",
      },
    },
  });

  quill.root.innerHTML = descriptionInput.value;

  quill.on(Quill.events.TEXT_CHANGE, () => {
    const plainText = quill.getText().trim();
    const descriptionInput = document.querySelector(`#descriptionInput`);
    const descriptionError = document.querySelector(`#description-input-error`);
    const maxLength = parseInt(descriptionInput.dataset.textMaxLength);

    if (plainText.length > maxLength) {
      descriptionError.textContent = `Описанието не може да е повече от ${maxLength} символа.`;
      return;
    } else {
      descriptionError.textContent = ``;
    }
  });

  // --- Form logic ---
  document
    .querySelector("#updateProductForm")
    .addEventListener("submit", function (e) {
      const plainText = quill.getText().trim();
      const descriptionInput = document.querySelector(`#descriptionInput`);
      const maxLength = parseInt(descriptionInput.dataset.textMaxLength);

      if (plainText.length > maxLength) {
        e.preventDefault();
        return;
      }

      const quillContent = quill.root.innerHTML.trim();
      descriptionInput.value = quillContent;
    });
});

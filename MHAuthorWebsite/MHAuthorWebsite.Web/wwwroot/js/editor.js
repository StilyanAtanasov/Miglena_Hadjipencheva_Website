"use strict";

class Counter {
  constructor(quill, options) {
    const container = document.querySelector(options.container);
    quill.on(Quill.events.TEXT_CHANGE, () => {
      this.calculate(quill, container);
    });
  }

  calculate(quill, container) {
    const text = quill.getText().trim();
    const words = text.length === 0 ? 0 : text.split(/\s+/).length;
    const chars = text.length;

    container.innerText = `${words} дум${
      words === 1 ? "а" : "и"
    }, ${chars} символ${chars === 1 ? "" : "а"}!`;

    const descriptionInput = document.querySelector(`#descriptionInput`);
    const descriptionError = document.querySelector(`#description-input-error`);
    const maxLength = parseInt(descriptionInput.dataset.textMaxLength);

    if (text.length > maxLength) {
      descriptionError.textContent = `Описанието не може да е повече от ${maxLength} символа.`;
    } else {
      descriptionError.textContent = ``;
    }
  }
}

export async function initQuill() {
  return new Promise((resolve) => {
    const descriptionInput = document.querySelector(`#descriptionInput`);

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
    resolve(quill);
  });
}

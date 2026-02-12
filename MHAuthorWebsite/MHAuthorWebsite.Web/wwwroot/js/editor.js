"use strict";

class Counter {
  constructor(quill, options) {
    const container = document.querySelector(options.container);
    quill.on(Quill.events.TEXT_CHANGE, () => {
      this.calculate(quill, container);
    });

    this.hasError = false;

    this.descriptionInput = document.querySelector(`#descriptionInput`);
    this.descriptionError = document.querySelector(`#description-input-error`);

    this.minLength = parseInt(this.descriptionInput?.dataset.textMinLength);
    this.maxLength = parseInt(this.descriptionInput?.dataset.textMaxLength);

    this.isErrorHandlingConfigured = !!(this.descriptionInput && this.descriptionError && !isNaN(this.minLength) && !isNaN(this.maxLength));
    if (!this.isErrorHandlingConfigured)
      console.warn(
        `Counter module: Error handling is not properly configured. Please ensure that the description input and error elements exist and have valid data attributes for min and max length.`,
      );

    this.calculate(quill, container);
  }

  calculate(quill, container) {
    const text = quill.getText().trim();
    const words = text.length === 0 ? 0 : text.split(/\s+/).length;
    const chars = text.length;

    container.innerText = `${words} дум${words === 1 ? `а` : `и`}, ${chars} символ${chars === 1 ? `` : `а`}!`;

    if (!this.isErrorHandlingConfigured) return;

    if (text.length < this.minLength) this.descriptionError.textContent = `Описанието не може да е по-кратко от ${this.minLength} символа.`;
    else if (text.length > this.maxLength) this.descriptionError.textContent = `Описанието не може да е повече от ${this.maxLength} символа.`;
    else this.descriptionError.textContent = ``;

    this.hasError = this.descriptionError.textContent !== ``;
  }
}

export async function initQuill(isEnabled = false, counter = false) {
  return new Promise(resolve => {
    const descriptionInput = document.querySelector(`#descriptionInput`);

    counter && Quill.register("modules/counter", Counter);

    const Font = Quill.import("formats/font");
    Font.whitelist = ["sofia-sans-condensed", "sans-serif", "serif", "monospace"];
    Quill.register(Font, true);

    const quill = new Quill("#description-editor", {
      theme: "snow",
      modules: {
        syntax: true,
        toolbar: isEnabled ? "#toolbar-container" : false,
        ...(counter && {
          counter: {
            container: "#counter",
          },
        }),
      },
    });

    quill.enable(isEnabled);
    const description = descriptionInput.value;
    if (description != undefined && description != null && description !== "") quill.setContents(JSON.parse(descriptionInput.value));

    resolve(quill);
  });
}

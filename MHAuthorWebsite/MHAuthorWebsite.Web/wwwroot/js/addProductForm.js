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

    return `${words} дум${words === 1 ? "а" : "и"}, ${chars} символ${
      chars === 1 ? "" : "а"
    }!`;
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
document.addEventListener("DOMContentLoaded", function () {
  const categorySelect = document.getElementById("selectProductType");
  RetrieveAttributes();

  categorySelect.addEventListener("change", RetrieveAttributes);

  quill.root.innerHTML = descriptionInput.value;

  document
    .querySelector("#addProductForm")
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

function RetrieveAttributes() {
  const attributesContainer = document.getElementById(
    "productTypeAttributesContainer"
  );

  const selectedId = this.value;

  if (!selectedId) {
    attributesContainer.innerHTML = "";
    return;
  }

  fetch(`/AdminProduct/GetCategoryTypeAttributes/${selectedId}`, {
    headers: {
      "X-Requested-With": "XMLHttpRequest",
    },
  })
    .then((response) => {
      if (!response.ok) throw new Error("Грешка при зареждане на атрибутите.");
      return response.text();
    })
    .then((html) => {
      attributesContainer.innerHTML = html;

      const $form = $("#addProductForm");
      $form.unbind();
      $form.removeData("validator");
      $form.removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse($form);
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}

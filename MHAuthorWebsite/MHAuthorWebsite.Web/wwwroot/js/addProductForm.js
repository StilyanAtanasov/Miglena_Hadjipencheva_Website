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

// --- Form logic ---
document.addEventListener("DOMContentLoaded", function () {
  const categorySelect = document.getElementById("selectProductType");
  RetrieveAttributes();

  categorySelect.addEventListener("change", RetrieveAttributes);

  document
    .querySelector("#addProductForm")
    .addEventListener("submit", function () {
      const quillContent = quill.root.innerHTML.trim();
      document.querySelector(`#descriptionInput`).value = quillContent;
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

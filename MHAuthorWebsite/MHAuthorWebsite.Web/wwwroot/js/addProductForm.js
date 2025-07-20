// --- Form logic ---
document.addEventListener("DOMContentLoaded", function () {
  const categorySelect = document.getElementById("selectProductType");
  RetrieveAttributes();

  categorySelect.addEventListener("change", RetrieveAttributes);
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

// --- Editor logic ---
const quill = new Quill("#description-editor", {
  theme: "snow",
});

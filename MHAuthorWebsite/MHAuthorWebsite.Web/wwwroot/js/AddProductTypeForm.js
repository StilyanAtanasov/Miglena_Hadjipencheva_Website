document.addEventListener("DOMContentLoaded", function () {
  const hasPropsCheckbox = document.getElementById("HasAdditionalProperties");
  const attributeSection = document.getElementById("attributeSection");
  const container = document.getElementById("attributeTemplates");
  const templateWrapper = document.getElementById("attribute-template");
  let index = parseInt(container.dataset.attributeIndex || "0");

  if (hasPropsCheckbox) {
    attributeSection.style.display = hasPropsCheckbox.checked
      ? "block"
      : "none";

    hasPropsCheckbox.addEventListener("change", function () {
      attributeSection.style.display = this.checked ? "block" : "none";
    });
  }

  const addButton = document.getElementById("addAttribute");
  if (addButton) {
    addButton.addEventListener("click", function () {
      let newTemplate = templateWrapper.innerHTML
        .replace(/\[0\]/g, `[${index}]`)
        .replace(/_0__/g, `_${index}__`);

      container.insertAdjacentHTML("beforeend", newTemplate);
      rebindValidators();

      index++;
    });
  }
});

document.addEventListener("click", function (e) {
  if (e.target && e.target.classList.contains("remove-attribute")) {
    e.target.closest(".attribute-definition").remove();
  }
});

function rebindValidators() {
  const $form = $("#AddProductTypeForm");
  $form.unbind();
  $form.removeData("validator");
  $form.removeData("unobtrusiveValidation");
  $.validator.unobtrusive.parse($form);
}

$("#AddProductTypeForm").on("submit", function (e) {
  if (!$(this).valid()) {
    e.preventDefault();
    return;
  }
});

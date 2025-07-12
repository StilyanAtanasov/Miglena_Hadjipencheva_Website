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
      if (index == 0) addDefinitionFields();
    });
  }

  const addButton = document.getElementById("addAttribute");
  if (addButton) addButton.addEventListener("click", addDefinitionFields);

  document.addEventListener("click", function (e) {
    if (e.target && e.target.classList.contains("remove-attribute")) {
      e.target.closest(".attribute-definition").remove();
      index--;
    }
  });

  $("#AddProductTypeForm").on("submit", function (e) {
    if (!$(this).valid()) {
      e.preventDefault();
      return;
    }
  });

  function rebindValidators() {
    const $form = $("#AddProductTypeForm");
    $form.unbind();
    $form.removeData("validator");
    $form.removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse($form);
  }

  function addDefinitionFields() {
    templateWrapper.innerHTML;
    templateHtml = templateHtml.replace(/__INDEX__/g, index);
    container.insertAdjacentHTML("beforeend", templateHtml);

    rebindValidators();

    index++;
  }
});

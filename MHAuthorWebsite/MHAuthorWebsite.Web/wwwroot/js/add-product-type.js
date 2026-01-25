"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  const hasPropsCheckbox = document.getElementById(`HasAdditionalProperties`);
  const attributeSection = document.getElementById(`attributeSection`);
  const container = document.getElementById(`attributeTemplates`);
  const templateWrapper = document.getElementById(`attribute-template`);
  let index = parseInt(container.dataset.attributeIndex || `0`);
  let activeAttributes = index;

  if (hasPropsCheckbox) {
    attributeSection.style.display = hasPropsCheckbox.checked ? `block` : `none`;

    hasPropsCheckbox.addEventListener(`change`, function () {
      attributeSection.style.display = this.checked ? `block` : `none`;
      if (this.checked && activeAttributes == 0) addDefinitionField();
    });
  }

  const addButton = document.getElementById(`addAttribute`);
  if (addButton) addButton.addEventListener(`click`, addDefinitionField);

  document.addEventListener(`click`, function (e) {
    if (e.target && e.target.classList.contains(`remove-attribute`)) {
      e.target.closest(`.attribute-definition`).remove();

      if (--activeAttributes == 0) {
        hasPropsCheckbox.checked = false;
        hasPropsCheckbox.dispatchEvent(new Event("change"));
      }
    }
  });

  $(`#AddProductTypeForm`).on(`submit`, function (e) {
    if (!$(this).valid()) {
      e.preventDefault();
      return;
    }
  });

  function rebindValidators() {
    const $form = $(`#AddProductTypeForm`);
    $form.unbind();
    $form.removeData(`validator`);
    $form.removeData(`unobtrusiveValidation`);
    $.validator.unobtrusive.parse($form);
  }

  function addDefinitionField() {
    let templateHtml = templateWrapper.innerHTML;
    templateHtml = templateHtml.replace(/__INDEX__/g, index);
    container.insertAdjacentHTML(`beforeend`, templateHtml);

    rebindValidators();

    index++;
    activeAttributes++;
  }

  // --- Data Type List ---
  container.addEventListener(`input`, function (e) {
    if (e.target && e.target.classList.contains(`data-type-select`)) {
      const section = e.target.closest(`.attribute-definition`).querySelector(`.predefined-values-section`);
      section.style.display = e.target.value === `4` ? `flex` : `none`;
    }

    if (e.target && e.target.classList.contains(`values-visual-input`)) {
      const input = e.target;
      if (input.value.includes(`,`)) processTags(input);
    }
  });

  function processTags(input) {
    const parent = input.closest(`.attribute-definition`);
    const tagsContainer = parent.querySelector(`.tags-container`);
    const hiddenContainer = parent.querySelector(`.values-hidden-container`);
    const prefix = parent.querySelector(`input[name="Attributes.Index"]`).value;

    let parts = input.value
      .split(`,`)
      .map(p => p.trim())
      .filter(p => p !== ``);
    input.value = ``;

    parts.forEach(tagText => {
      const tagHtml = `<span class="tag-badge">${tagText} <span class="remove-tag" data-val="${tagText}"><i class="fa-solid fa-xmark"></i></span></span>`;
      tagsContainer.insertAdjacentHTML(`beforeend`, tagHtml);
    });

    updateHiddenInputs(prefix, tagsContainer, hiddenContainer);
  }

  function updateHiddenInputs(prefix, tagsContainer, hiddenContainer) {
    const allTags = Array.from(tagsContainer.querySelectorAll(`.tag-badge`)).map(t => t.innerText.replace(` ×`, ``).trim());

    const baseName = `Attributes[${prefix}].PredefinedValues`;
    hiddenContainer.innerHTML = allTags.map((val, i) => `<input type="hidden" name="${baseName}[${i}]" value="${val}" />`).join(``);
  }

  container.addEventListener(`click`, function (e) {
    if (e.target && e.target.closest(`.remove-tag`)) {
      const parent = e.target.closest(`.attribute-definition`);
      const prefix = parent.querySelector(`input[name="Attributes.Index"]`).value;

      e.target.closest(`.remove-tag`).parentElement.remove();
      updateHiddenInputs(parent, prefix, parent.querySelector(`.tags-container`), parent.querySelector(`.values-hidden-container`));
    }
  });
});

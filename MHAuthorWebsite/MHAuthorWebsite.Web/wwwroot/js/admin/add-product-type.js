"use strict";

import { initTomSelect } from "../elements/select.js";

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

  initializeExistingAttributes();

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
    initTomSelect(container.querySelector(`.attribute-definition:nth-child(${index + 1}) .data-type-select`));
    initializeExistingAttributes();

    index++;
    activeAttributes++;
  }

  function initializeExistingAttributes() {
    if (!container) return;

    container.querySelectorAll(`.attribute-definition`).forEach(def => {
      initTomSelect(def.querySelector(`.data-type-select`));
      syncPredefinedSection(def);
      hydrateTagsFromHiddenInputs(def);
    });
  }

  function syncPredefinedSection(def) {
    const select = def.querySelector(`.data-type-select`);
    const section = def.querySelector(`.predefined-values-section`);
    if (!select || !section) return;

    section.style.display = select.value === `4` ? `flex` : `none`;
  }

  function appendTag(tagsContainer, value) {
    const badge = document.createElement(`span`);
    badge.classList.add(`tag-badge`);
    badge.append(document.createTextNode(`${value} `));

    const remove = document.createElement(`span`);
    remove.classList.add(`remove-tag`);
    remove.dataset.val = value;
    remove.innerHTML = `<i class="fa-solid fa-xmark"></i>`;

    badge.appendChild(remove);
    tagsContainer.appendChild(badge);
  }

  function hydrateTagsFromHiddenInputs(def) {
    const tagsContainer = def.querySelector(`.tags-container`);
    const hiddenContainer = def.querySelector(`.values-hidden-container`);
    const prefixInput = def.querySelector(`input[name="Attributes.Index"]`);

    if (!tagsContainer || !hiddenContainer || !prefixInput) return;
    if (tagsContainer.querySelector(`.tag-badge`)) return;

    const hiddenInputs = Array.from(hiddenContainer.querySelectorAll(`input[type="hidden"]`));
    if (hiddenInputs.length === 0) return;

    hiddenInputs.forEach(i => appendTag(tagsContainer, i.value));
    updateHiddenInputs(prefixInput.value, tagsContainer, hiddenContainer);
  }

  // --- Data Type List ---
  container.addEventListener(`change`, function (e) {
    if (e.target && e.target.classList.contains(`data-type-select`)) {
      const def = e.target.closest(`.attribute-definition`);
      syncPredefinedSection(def);
    }
  });

  container.addEventListener(`input`, function (e) {
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
      appendTag(tagsContainer, tagText);
    });

    updateHiddenInputs(prefix, tagsContainer, hiddenContainer);
  }

  function updateHiddenInputs(prefix, tagsContainer, hiddenContainer) {
    const allTags = Array.from(tagsContainer.querySelectorAll(`.remove-tag`))
      .map(t => t.dataset.val)
      .filter(v => !!v);
    const baseName = `Attributes[${prefix}].PredefinedValues`;

    hiddenContainer.innerHTML = ``;
    allTags.forEach((val, i) => {
      const input = document.createElement(`input`);
      input.type = `hidden`;
      input.name = `${baseName}[${i}]`;
      input.value = val;
      hiddenContainer.appendChild(input);
    });
  }

  container.addEventListener(`click`, function (e) {
    if (e.target && e.target.closest(`.remove-tag`)) {
      const parent = e.target.closest(`.attribute-definition`);
      const prefix = parent.querySelector(`input[name="Attributes.Index"]`).value;

      e.target.closest(`.remove-tag`).parentElement.remove();
      updateHiddenInputs(prefix, parent.querySelector(`.tags-container`), parent.querySelector(`.values-hidden-container`));
    }
  });
});

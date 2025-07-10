document.addEventListener("DOMContentLoaded", function () {
  const hasPropsCheckbox = document.getElementById("HasAdditionalProperties");
  const attributeSection = document.getElementById("attributeSection");
  const container = document.getElementById("attributeTemplates");
  const enumOptions = container.dataset.enumOptions;
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
      const template = `
            <div id="attribute-${index}" class="attribute-definition mb-3">
                <button type="button" class="btn-close remove-attribute" aria-label="Close"></button>

                <label>Ключ:</label>
                <input name="Attributes[${index}].Key" class="form-control" data-val="true"/>
                <span class="text-danger field-validation-valid" data-valmsg-for="Attributes[${index}].Key" data-valmsg-replace="true"></span>

                <label>Етикет:</label>
                <input name="Attributes[${index}].Label" class="form-control" data-val="true"/>
                <span class="text-danger field-validation-valid" data-valmsg-for="Attributes[${index}].Label" data-valmsg-replace="true"></span>

                <label>Тип:</label>
                <select name="Attributes[${index}].DataType" class="form-control" data-val="true">
                    ${enumOptions}
                </select>
                <span class="text-danger field-validation-valid" data-valmsg-for="Attributes[${index}].DataType" data-valmsg-replace="true"></span>

                <label>Предефинирани стойности?</label>
                <input type="checkbox" name="Attributes[${index}].HasPredefinedValue" />
                <span class="text-danger field-validation-valid" data-valmsg-for="Attributes[${index}].HasPredefinedValue" data-valmsg-replace="true"></span>

                <label>Задължително?</label>
                <input type="checkbox" name="Attributes[${index}].IsRequired" />
                <span class="text-danger field-validation-valid" data-valmsg-for="Attributes[${index}].IsRequired" data-valmsg-replace="true"></span>
            </div>
        `;
      container.insertAdjacentHTML("beforeend", template);

      $.validator.unobtrusive.parse(`#attribute-${index}`);

      index++;
    });
  }
});

document.addEventListener("click", function (e) {
  if (e.target && e.target.classList.contains("remove-attribute")) {
    e.target.closest(".attribute-definition").remove();
  }
});

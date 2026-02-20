"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  const form = document.getElementById(`legalDocumentForm`);
  const container = document.getElementById(`nodes-container`);
  const addBtn = document.getElementById(`add-node-btn`);
  const template = document.getElementById(`legal-node-template`);

  if (!form || !container || !addBtn || !template) return;

  const editors = new Map();
  let draggedItem = null;
  const hasQuill = typeof Quill !== `undefined`;
  const defaultDelta = JSON.stringify({ ops: [{ insert: `\n` }] });

  function buildToolbarOptions() {
    return [
      [{ font: [] }, { size: [] }],
      [`bold`, `italic`, `underline`, `strike`],
      [{ color: [] }, { background: [] }],
      [{ script: `sub` }, { script: `super` }],
      [{ header: 1 }, { header: 2 }, `blockquote`, `code-block`],
      [{ list: `ordered` }, { list: `bullet` }, { indent: `-1` }, { indent: `+1` }],
      [{ direction: `rtl` }, { align: [] }],
      [`link`, `video`, `formula`],
      [`clean`],
    ];
  }

  function initEditorForItem(item) {
    const editorContainer = item.querySelector(`.js-node-editor`);
    const deltaInput = item.querySelector(`.node-delta-input`);
    if (!editorContainer || !deltaInput) return;

    if (!deltaInput.value) deltaInput.value = defaultDelta;
    if (!hasQuill) {
      editors.set(item, null);
      return;
    }

    const quill = new Quill(editorContainer, {
      theme: "snow",
      modules: {
        syntax: true,
        toolbar: buildToolbarOptions(),
      },
    });

    if (deltaInput.value) {
      try {
        quill.setContents(JSON.parse(deltaInput.value));
      } catch {
        quill.setText(``);
      }
    }

    editors.set(item, quill);
  }

  function getNextIndex() {
    return container.querySelectorAll(`.legal-node-item`).length;
  }

  function updateNodeOrderNumbers() {
    const items = container.querySelectorAll(`.legal-node-item`);
    items.forEach((item, index) => {
      const orderNumber = index + 1;
      const numberLabel = item.querySelector(`.node-order-number`);
      const numberInput = item.querySelector(`.node-number-input`);
      if (numberLabel) numberLabel.textContent = String(orderNumber);
      if (numberInput) numberInput.value = String(orderNumber);
    });
  }

  function reindexNodeNames() {
    const items = container.querySelectorAll(`.legal-node-item`);
    items.forEach((item, index) => {
      item.setAttribute(`data-node-index`, String(index));

      const numberInput = item.querySelector(`.node-number-input`);
      const titleInput = item.querySelector(`.node-title-input`);
      const deltaInput = item.querySelector(`.node-delta-input`);

      if (numberInput) numberInput.setAttribute(`name`, `Nodes[${index}].Number`);
      if (titleInput) titleInput.setAttribute(`name`, `Nodes[${index}].Title`);
      if (deltaInput) deltaInput.setAttribute(`name`, `Nodes[${index}].ContentDelta`);
    });

    updateNodeOrderNumbers();
  }

  function wireRemoveButtons(root) {
    const removeButtons = root.querySelectorAll(`.remove-node-btn`);
    removeButtons.forEach(btn => {
      btn.addEventListener(`click`, function () {
        const item = this.closest(`.legal-node-item`);
        if (!item) return;
        editors.delete(item);
        item.remove();
        reindexNodeNames();
      });
    });
  }

  function getDragAfterElement(y) {
    const elements = [...container.querySelectorAll(`.legal-node-item:not(.dragging)`)];
    return elements.reduce(
      (closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) {
          return { offset, element: child };
        }
        return closest;
      },
      { offset: Number.NEGATIVE_INFINITY, element: null },
    ).element;
  }

  function wireDragAndDrop(root) {
    const items = root.querySelectorAll(`.legal-node-item`);
    items.forEach(item => {
      item.addEventListener(`dragstart`, function () {
        draggedItem = item;
        item.classList.add(`dragging`);
      });

      item.addEventListener(`dragend`, function () {
        item.classList.remove(`dragging`);
        draggedItem = null;
        reindexNodeNames();
      });
    });
  }

  container.addEventListener(`dragover`, function (e) {
    e.preventDefault();
    if (!draggedItem) return;
    const afterElement = getDragAfterElement(e.clientY);
    if (!afterElement) {
      container.appendChild(draggedItem);
      return;
    }
    container.insertBefore(draggedItem, afterElement);
  });

  function addNewNode() {
    const index = getNextIndex();
    const html = template.innerHTML.replaceAll(`__index__`, String(index));
    const wrapper = document.createElement(`div`);
    wrapper.innerHTML = html.trim();
    const item = wrapper.firstElementChild;
    if (!item) return;

    container.appendChild(item);
    initEditorForItem(item);
    wireRemoveButtons(item);
    wireDragAndDrop(item);
    reindexNodeNames();
  }

  function validateAndSerialize() {
    const items = container.querySelectorAll(`.legal-node-item`);
    let hasError = false;

    items.forEach(item => {
      const errorNode = item.querySelector(`.node-error`);
      const titleInput = item.querySelector(`.node-title-input`);
      const deltaInput = item.querySelector(`.node-delta-input`);
      const quill = editors.get(item);

      if (!errorNode || !titleInput || !deltaInput) return;

      errorNode.textContent = ``;
      const titleValue = titleInput.value.trim();
      const textLength = quill ? quill.getText().trim().length : 1;

      if (!titleValue || titleValue.length < 3) {
        errorNode.textContent = `Заглавието трябва да е поне 3 символа.`;
        hasError = true;
        return;
      }

      if (textLength === 0) {
        errorNode.textContent = `Съдържанието на раздела е задължително.`;
        hasError = true;
        return;
      }

      if (quill) deltaInput.value = JSON.stringify(quill.getContents());
      if (!deltaInput.value) deltaInput.value = defaultDelta;
    });

    return !hasError;
  }

  const initialItems = container.querySelectorAll(`.legal-node-item`);
  initialItems.forEach(item => initEditorForItem(item));
  wireRemoveButtons(document);
  wireDragAndDrop(document);
  reindexNodeNames();

  addBtn.addEventListener(`click`, addNewNode);
  form.addEventListener(`submit`, function (e) {
    reindexNodeNames();

    if (!validateAndSerialize()) {
      e.preventDefault();
      return;
    }

    reindexNodeNames();
  });
});

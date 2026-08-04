"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  const nodes = document.querySelectorAll(`.js-legal-node-editor`);
  if (!nodes.length || typeof Quill === `undefined`) return;

  nodes.forEach(node => {
    const rawDelta = node.getAttribute(`data-delta`);
    if (!rawDelta) return;

    const quill = new Quill(node, {
      theme: "snow",
      modules: {
        toolbar: false,
        syntax: true,
      },
    });

    quill.enable(false);

    try {
      quill.setContents(JSON.parse(rawDelta));
    } catch {
      quill.setText(`Невалидно съдържание.`);
    }
  });
});

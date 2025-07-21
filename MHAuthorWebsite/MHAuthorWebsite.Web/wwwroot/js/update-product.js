document.addEventListener(`DOMContentLoaded`, function () {
  const descriptionInput = document.querySelector(`#descriptionInput`);

  // --- Editor logic ---
  const quill = new Quill("#description-editor", {
    theme: "snow",
  });

  quill.root.innerHTML = descriptionInput.value;

  // --- Form logic ---
  document
    .querySelector("#updateProductForm")
    .addEventListener("submit", function () {
      const quillContent = quill.root.innerHTML.trim();
      descriptionInput.value = quillContent;
    });
});

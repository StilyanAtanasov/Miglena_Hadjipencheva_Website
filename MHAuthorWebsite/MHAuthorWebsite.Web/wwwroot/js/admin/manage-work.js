import { initQuill } from "../editor.js";

document.addEventListener("DOMContentLoaded", async function () {
  const quill = await initQuill(true, false);

  const coverUploadBtn = document.getElementById(`coverUploadBtn`);
  const coverUpload = document.getElementById("coverUpload");
  const coverPreview = document.getElementById("coverPreview");

  if (coverUploadBtn && coverUpload) {
    coverUploadBtn.addEventListener(`click`, () => coverUpload.click());
  }

  // --- Image Preview Logic ---
  if (coverUpload && coverPreview) {
    coverUpload.addEventListener("change", function () {
      const file = this.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
          coverPreview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 100%; max-height: 100%; object-fit: contain;">`;
        };
        reader.readAsDataURL(file);
      }
    });
  }

  // --- Form & Content Sync Logic ---
  const form = document.querySelector("form");
  const hiddenContentInput = document.getElementById("descriptionInput");

  if (quill && hiddenContentInput) {
    quill.on("text-change", function () {
      const delta = quill.getContents();
      hiddenContentInput.value = JSON.stringify(delta);

      // Trigger validation on the hidden input if jQuery validation is present
      if (window.jQuery && window.jQuery.validator) {
        window.jQuery(hiddenContentInput).valid();
      }
    });

    if (hiddenContentInput.value && hiddenContentInput.value.startsWith("{")) {
      try {
        const initialDelta = JSON.parse(hiddenContentInput.value);
        quill.setContents(initialDelta);
      } catch (e) {
        console.error("Error parsing initial delta:", e);
      }
    }

    if (form) {
      form.addEventListener("submit", function (e) {
        const delta = quill.getContents();
        hiddenContentInput.value = JSON.stringify(delta);
      });
    }
  }
});

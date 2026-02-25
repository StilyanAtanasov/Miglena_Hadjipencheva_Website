import { initQuill } from "../editor.js";
import { validateImageSizes, MAX_IMAGE_SIZE_BYTES } from "../utils/image-size-validation.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const quill = await initQuill(true, true);

  const coverUploadBtn = document.getElementById(`coverUploadBtn`);
  const coverUpload = document.getElementById(`coverUpload`);
  const coverPreview = document.getElementById(`coverPreview`);
  const coverUploadError = document.getElementById(`coverUploadError`);

  if (coverUploadBtn && coverUpload) coverUploadBtn.addEventListener(`click`, () => coverUpload.click());

  // --- Image Preview + Size Validation ---
  if (coverUpload && coverPreview) {
    coverUpload.addEventListener(`change`, function () {
      const file = this.files[0];
      if (!file) return;

      coverUploadError.textContent = ``;

      if (file.size > MAX_IMAGE_SIZE_BYTES) {
        validateImageSizes([file], coverUploadError);
        this.value = ``;
        return;
      }

      const reader = new FileReader();
      reader.onload = function (e) {
        coverPreview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 100%; max-height: 100%; object-fit: contain;">`;
      };

      reader.readAsDataURL(file);
    });
  }

  // --- Form & Content Sync Logic ---
  const form = document.querySelector(`form`);
  const hiddenContentInput = document.getElementById(`descriptionInput`);

  if (quill && hiddenContentInput && form) {
    form.addEventListener(`submit`, function (e) {
      // Block submit if an oversized file is somehow still selected
      if (coverUpload && coverUpload.files.length > 0) {
        const file = coverUpload.files[0];
        if (file.size > MAX_IMAGE_SIZE_BYTES) {
          e.preventDefault();
          validateImageSizes([file], coverUploadError);
          coverUpload.scrollIntoView({ behavior: `smooth`, block: `center` });
          return;
        }
      }

      const delta = quill.getContents();

      const counterModule = quill.getModule("counter");
      if (counterModule && counterModule.hasError) {
        e.preventDefault();
        quill.container.scrollIntoView({ behavior: `smooth`, block: `center` });
        return;
      }

      // Require cover image on Add form (coverUpload exists and is required)
      const isCoverRequired = coverUpload && coverUpload.required !== false && !document.getElementById(`CurrentCoverImageUrl`);
      if (isCoverRequired && coverUpload.files.length < 1) {
        e.preventDefault();
        coverUploadError.textContent = "Моля, качете корица за произведението.";
        return;
      }

      hiddenContentInput.value = JSON.stringify(delta);
    });
  }
});

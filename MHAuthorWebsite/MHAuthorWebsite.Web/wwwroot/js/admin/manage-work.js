import { initQuill } from "../editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const quill = await initQuill(true, true);

  const coverUploadBtn = document.getElementById(`coverUploadBtn`);
  const coverUpload = document.getElementById(`coverUpload`);
  const coverPreview = document.getElementById(`coverPreview`);
  const coverUploadError = document.getElementById(`coverUploadError`);

  if (coverUploadBtn && coverUpload) coverUploadBtn.addEventListener(`click`, () => coverUpload.click());

  // --- Image Preview Logic ---
  if (coverUpload && coverPreview) {
    coverUpload.addEventListener(`change`, function () {
      const file = this.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
          coverPreview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 100%; max-height: 100%; object-fit: contain;">`;
        };

        reader.readAsDataURL(file);
      }

      coverUploadError.textContent = ``;
    });
  }

  // --- Form & Content Sync Logic ---
  const form = document.querySelector(`form`);
  const hiddenContentInput = document.getElementById(`descriptionInput`);

  if (quill && hiddenContentInput && form) {
    form.addEventListener(`submit`, function (e) {
      const delta = quill.getContents();

      const counterModule = quill.getModule("counter");
      if (counterModule && counterModule.hasError) {
        e.preventDefault();
        quill.container.scrollIntoView({ behavior: `smooth`, block: `center` });
        return;
      }

      if (coverUpload && coverUpload.files.length < 1) {
        e.preventDefault();
        coverUploadError.textContent = "Моля, качете корица за произведението.";
        return;
      }

      hiddenContentInput.value = JSON.stringify(delta);
    });
  }
});

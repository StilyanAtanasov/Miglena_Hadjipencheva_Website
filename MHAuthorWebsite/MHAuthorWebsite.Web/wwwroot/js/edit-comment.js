"use strict";

import { pushNotification } from "./notification.js";
import { validateImageSizes, MAX_IMAGE_SIZE_BYTES } from "./utils/image-size-validation.js";

const imageInput = document.getElementById(`imageInput`);
const previewContainer = document.getElementById(`previewContainer`);
const imageErrorField = document.getElementById(`image-error`);
const maxImages = +imageErrorField.dataset.maxImages;
const form = document.getElementById(`edit-comment-form`);

const imgContainerClassName = `image-container`;
const removedImagesInputName = `RemovedImagesUrls`;

let selectedFiles = [];
let removedImages = [];

imageInput?.addEventListener(`change`, function () {
  const files = Array.from(this.files);

  // Count check
  if (selectedFiles.length + files.length > maxImages) {
    imageErrorField.innerText = `Можете да качите максимум още ${maxImages} снимки!`;
    this.value = ``;
    updateFileInput();
    return;
  }

  // Size check
  if (!validateImageSizes(files, imageErrorField)) {
    this.value = ``;
    updateFileInput();
    return;
  }

  imageErrorField.textContent = ``;

  files.forEach(file => {
    if (!file.type.startsWith(`image/`)) return;

    const reader = new FileReader();
    reader.onload = function (e) {
      const imgWrapper = createFileImageWrapper(e.target.result, file);
      previewContainer.appendChild(imgWrapper);

      if (previewContainer.innerHTML.trim() != ``) previewContainer.classList.remove(`hidden`);
    };

    selectedFiles.push(file);
    reader.readAsDataURL(file);
  });

  this.value = ``;
  updateFileInput();
});

// --- Submit guard ---
if (form) {
  form.addEventListener(`submit`, function (e) {
    if (selectedFiles.some(f => f.size > MAX_IMAGE_SIZE_BYTES)) {
      e.preventDefault();
      imageErrorField.textContent = `Всяко изображение трябва да е до 10 MB.`;
      imageErrorField.scrollIntoView({ behavior: `smooth`, block: `center` });
    }
  });
}

function updateFileInput() {
  const dataTransfer = new DataTransfer();
  selectedFiles.forEach(f => dataTransfer.items.add(f));
  imageInput.files = dataTransfer.files;
}

function remove(file, imgWrapper) {
  try {
    const index = selectedFiles.indexOf(file);
    if (index > -1) {
      selectedFiles.splice(index, 1);
      imgWrapper.remove();
      updateFileInput();
    } else {
      // - Handle existing images removal
      const imgUrl = imgWrapper.dataset.id;
      if (imgUrl && !removedImages.includes(imgUrl)) {
        removedImages.push(imgUrl);
        addHiddenRemovedImageInput(imgUrl);
        imgWrapper.remove();
      }
    }

    if (previewContainer.innerHTML.trim() == ``) previewContainer.classList.add(`hidden`);
  } catch {
    pushNotification(`Възникна неочаквана грешка!`, `error`);
  }
}

function createFileImageWrapper(src, fileOrUrl) {
  const imgWrapper = document.createElement(`div`);
  imgWrapper.classList.add(imgContainerClassName);

  const img = document.createElement(`img`);
  img.src = src;

  const removeBtn = document.createElement(`button`);
  removeBtn.type = `button`;
  removeBtn.classList = `removeBtn`;
  removeBtn.innerHTML = `<i class="fa-solid fa-file-slash"></i>`;
  removeBtn.addEventListener(`click`, () => remove(fileOrUrl, imgWrapper));

  imgWrapper.appendChild(img);
  imgWrapper.appendChild(removeBtn);

  return imgWrapper;
}

function addHiddenRemovedImageInput(id) {
  const input = document.createElement(`input`);
  input.type = `hidden`;
  input.name = removedImagesInputName;
  input.value = id;
  form.appendChild(input);
}

// - Add listener for all present images
document.querySelectorAll(`#previewContainer .image-container`).forEach(c => {
  c.addEventListener(`click`, () => remove(null, c));
});

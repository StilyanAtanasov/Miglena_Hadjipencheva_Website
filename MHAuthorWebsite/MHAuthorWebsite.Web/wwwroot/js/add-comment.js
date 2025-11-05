"use strict";

import { pushNotification } from "./notification.js";

const imageInput = document.getElementById(`imageInput`);
const previewContainer = document.getElementById(`previewContainer`);
const imageErrorField = document.getElementById(`image-error`);
const maxImages = imageErrorField.dataset.maxImages;

const imgContainerClassName = `image-container`;

let selectedFiles = [];

imageInput.addEventListener(`change`, function () {
  const files = Array.from(this.files);

  if (selectedFiles.length + files.length > maxImages) {
    imageErrorField.innerText = `Можете да качите максимум ${maxImages} снимки!`;
    this.value = ``; // Allow re-selecting same files
    updateFileInput();
    return;
  }

  files.forEach(file => {
    if (!file.type.startsWith(`image/`)) return;

    const reader = new FileReader();
    reader.onload = function (e) {
      const imgWrapper = document.createElement(`div`);
      imgWrapper.classList.add(imgContainerClassName);

      const img = document.createElement(`img`);
      img.src = e.target.result;

      const removeBtn = document.createElement(`button`);
      removeBtn.type = `button`;
      removeBtn.classList = `removeBtn`;
      removeBtn.innerHTML = `<i class="fa-solid fa-file-slash"></i>`;
      removeBtn.addEventListener(`click`, () => remove(file, imgWrapper));

      imgWrapper.appendChild(img);
      imgWrapper.appendChild(removeBtn);
      previewContainer.appendChild(imgWrapper);
    };

    selectedFiles.push(file);
    reader.readAsDataURL(file);
  });

  this.value = ``; // Allow re-selecting same files
  updateFileInput();
});

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
    }
  } catch {
    return pushNotification(`Възникна неочаквана грешка!`, `error`);
  }
}

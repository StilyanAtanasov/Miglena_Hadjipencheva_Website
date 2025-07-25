"use strict";

import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function (e) {
  const categorySelect = document.getElementById(`selectProductType`);
  const quill = await initQuill(true, true);

  RetrieveAttributes();
  categorySelect.addEventListener(`change`, RetrieveAttributes);

  document.querySelector(`#addProductForm`).addEventListener(`submit`, function (e) {
    const plainText = quill.getText().trim();

    const descriptionInput = document.querySelector(`#descriptionInput`);
    const maxLength = parseInt(descriptionInput.dataset.textMaxLength);

    if (plainText.length > maxLength) {
      e.preventDefault();
      return;
    }

    const delta = quill.getContents();
    descriptionInput.value = JSON.stringify(delta);
  });
});

function RetrieveAttributes() {
  const attributesContainer = document.getElementById(`productTypeAttributesContainer`);
  const selectElement = document.getElementById(`selectProductType`);

  const selectedId = selectElement.value;

  if (!selectedId) {
    attributesContainer.innerHTML = ``;
    return;
  }

  fetch(`/AdminProduct/GetCategoryTypeAttributes/${selectedId}`, {
    headers: {
      "X-Requested-With": `XMLHttpRequest`,
    },
  })
    .then(response => {
      if (!response.ok) throw new Error(`Грешка при зареждане на атрибутите.`);
      return response.text();
    })
    .then(html => {
      attributesContainer.innerHTML = html;

      const $form = $(`#addProductForm`);
      $form.unbind();
      $form.removeData(`validator`);
      $form.removeData(`unobtrusiveValidation`);
      $.validator.unobtrusive.parse($form);
    })
    .catch(error => {
      console.error(`Error:`, error);
    });
}

// --- Image preview ---
const imageInput = document.getElementById(`imageInput`);
const previewContainer = document.getElementById(`previewContainer`);
const imageErrorField = document.getElementById(`image-error`);
const maxImages = imageErrorField.dataset.maxImages;

const TitleImageIdField = document.getElementById(`title-image-id`);
let nextIndex = 0;
let currentTitleImage;

const imgContainerClassName = `image-container`;
const titleImgClassName = `title-img`;
const titleImgContainerClassName = `title-img-container`;

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
      img.dataset.index = nextIndex++;

      if (nextIndex === 1) {
        img.classList.add(titleImgClassName);
        currentTitleImage = img;
      }

      const removeBtn = document.createElement(`button`);
      removeBtn.type = `button`;
      removeBtn.classList = `removeBtn`;
      removeBtn.innerHTML = `<i class="fa-solid fa-file-slash"></i>`;
      removeBtn.addEventListener(`click`, remove);

      const makeTitleBtn = document.createElement(`button`);
      makeTitleBtn.type = `button`;
      makeTitleBtn.classList = `makeTitleBtn`;
      makeTitleBtn.innerHTML = `<i class="fa-solid fa-star-sharp"></i>`;
      makeTitleBtn.addEventListener(`click`, makeTitle);

      imgWrapper.appendChild(img);
      imgWrapper.appendChild(removeBtn);
      imgWrapper.appendChild(makeTitleBtn);
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

function remove() {
  try {
    const index = selectedFiles.indexOf(file);
    if (index > -1) {
      selectedFiles.splice(index, 1);
      nextIndex--; // FIX
    }

    imgWrapper.remove();
    updateFileInput();
  } catch {
    return console.log(`error`); // TODO
  }
}

function makeTitle(e) {
  try {
    const clicked = e.target.closest(`.image-container`).querySelector(`img`);
    if (clicked === null || clicked === undefined) return;

    const newIndex = clicked.dataset.index;
    if (newIndex === null || newIndex === undefined || newIndex > nextIndex - 1) return;
    TitleImageIdField.value = newIndex;

    currentTitleImage.classList.remove(titleImgClassName);
    currentTitleImage.closest(`.${imgContainerClassName}`).classList.remove(titleImgContainerClassName);

    currentTitleImage = clicked;

    currentTitleImage.classList.add(titleImgClassName);
    currentTitleImage.closest(`.${imgContainerClassName}`).classList.add(titleImgContainerClassName);
  } catch {
    return console.log(`error`); // TODO return a message
  }
}

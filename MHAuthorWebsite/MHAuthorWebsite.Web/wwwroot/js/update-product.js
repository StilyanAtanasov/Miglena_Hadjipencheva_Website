"use strict";

import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const quill = await initQuill(true, true);

  document.querySelector("#updateProductForm").addEventListener("submit", function (e) {
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

// --- Images ---
class Image {
  constructor(isTitle = false) {
    this.isTitle = isTitle;
  }
}

class ExistingImage extends Image {
  constructor(id, url, isTitle = false) {
    super(isTitle);
    this.id = id;
    this.url = url;
  }
}

class AddedImage extends Image {
  constructor(file, previewUrl, isTitle = false) {
    super(isTitle);
    this.file = file;
    this.previewUrl = previewUrl;
  }
}

const imageState = {
  existing: [], // - Server images
  added: [], // - File objects from input
  deletedIds: [], // - Track `id`s of removed images (existing only)
};

let currentTitleImage;
let currentTitleImageElement;

const imgContainerClassName = `image-container`;
const titleImgClassName = `title-img`;
const titleImgContainerClassName = `title-img-container`;

const imageInput = document.getElementById(`imageInput`);
const previewContainer = document.getElementById(`previewContainer`);
const imageErrorField = document.getElementById(`image-error`);
const maxImages = imageErrorField.dataset.maxImages;

document.addEventListener(`DOMContentLoaded`, async () => {
  const imageContainers = previewContainer.querySelectorAll(`.${imgContainerClassName}`);
  imageContainers.forEach(ic => {
    const imageElement = ic.querySelector(`img`);
    const image = new ExistingImage(imageElement.dataset.id, imageElement.src, imageElement.dataset.isTitle.toLowerCase() === `true`);
    imageState.existing.push(image);

    if (image.isTitle) {
      currentTitleImage = image;
      currentTitleImageElement = imageElement;
    }

    ic.querySelector(`.removeBtn`).addEventListener(`click`, () => deleteImage(image, ic));
    ic.querySelector(`.makeTitleBtn`).addEventListener(`click`, () => makeTitle(image, imageElement));
  });

  imageInput.addEventListener(`change`, async function () {
    const files = Array.from(this.files).filter(f => f.type.startsWith(`image/`));

    if (imageState.added.length + files.length + imageState.existing.length > maxImages) {
      imageErrorField.innerText = `Можете да качите максимум ${maxImages} снимки!`;
      this.value = ``; // Allow re-selecting same files
      updateFileInput();
      return;
    }

    const filePromises = files.map(async file => {
      const previewUrl = await readFileAsync(file);
      const image = new AddedImage(file, previewUrl);
      imageState.added.push(image);

      const imgWrapper = document.createElement(`div`);
      imgWrapper.classList.add(imgContainerClassName);

      const img = document.createElement(`img`);
      img.src = previewUrl;

      const removeBtn = document.createElement(`button`);
      removeBtn.type = `button`;
      removeBtn.classList = `removeBtn`;
      removeBtn.innerHTML = `<i class="fa-solid fa-file-slash"></i>`;
      removeBtn.addEventListener(`click`, () => deleteImage(image, imgWrapper));

      const makeTitleBtn = document.createElement(`button`);
      makeTitleBtn.type = `button`;
      makeTitleBtn.classList = `makeTitleBtn`;
      makeTitleBtn.innerHTML = `<i class="fa-solid fa-star-sharp"></i>`;
      makeTitleBtn.addEventListener(`click`, () => makeTitle(image, img));

      imgWrapper.appendChild(img);
      imgWrapper.appendChild(removeBtn);
      imgWrapper.appendChild(makeTitleBtn);
      previewContainer.appendChild(imgWrapper);

      if (imageState.added.length + imageState.existing.length === 1) makeTitle(image, img, true);
    });

    await Promise.all(filePromises);

    this.value = ``; // Allow re-selecting same files
    updateFileInput();
  });
});

function readFileAsync(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = e => resolve(e.target.result);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

function updateFileInput() {
  debugger;
  const dataTransfer = new DataTransfer();
  imageState.added.forEach(i => dataTransfer.items.add(i.file));
  imageInput.files = dataTransfer.files;
}

function deleteImage(image, imageContainerElement) {
  try {
    let index = imageState.added.indexOf(image);
    if (index !== -1) imageState.added.splice(index, 1);
    else {
      index = imageState.existing.indexOf(image);
      if (index !== -1) {
        const deleted = imageState.existing.splice(index, 1)[0];
        imageState.deletedIds.push(deleted.id);
      }
    }

    imageContainerElement.remove();

    const imagesLeft = imageState.added.length + imageState.existing.length;
    if (image.isTitle && imagesLeft > 0) {
      const newTitleImage = imageState.existing.length > 0 ? imageState.existing[0] : imageState.added[0];
      const newTitleImageElement = document.querySelector(`#previewContainer img[data-id="${newTitleImage.id}"]`);
      makeTitle(newTitleImage, newTitleImageElement, true);
    } else if (imagesLeft === 0) previewContainer.innerHTML = ``;
  } catch {
    return console.log("delete error"); // FIX
  }
}

function makeTitle(newImage, newImageElement, oldImageRemoved = false) {
  try {
    if (!newImage || !newImageElement || currentTitleImage === newImage) return;

    if (!oldImageRemoved) {
      currentTitleImage.isTitle = false;
      currentTitleImageElement.classList.remove(titleImgClassName);
      currentTitleImageElement.closest(`.${imgContainerClassName}`).classList.remove(titleImgContainerClassName);
    }

    currentTitleImage = newImage;
    currentTitleImageElement = newImageElement;

    currentTitleImage.isTitle = true;
    currentTitleImageElement.classList.add(titleImgClassName);
    currentTitleImageElement.closest(`.${imgContainerClassName}`).classList.add(titleImgContainerClassName);
  } catch {
    return console.log(`error`); // TODO return a message
  }
}

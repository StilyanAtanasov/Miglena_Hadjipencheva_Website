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

document.addEventListener(`DOMContentLoaded`, async () => {
  const imageContainers = document.querySelectorAll(`#previewContainer .image-container`);
  imageContainers.forEach(ic => {
    const imageElement = ic.querySelector(`img`);
    const image = new ExistingImage(imageElement.dataset.id, imageElement.src, imageElement.dataset.isTitle);
    imageState.existing.push(image);

    if (imageElement.dataset.isTitle) currentTitleImage = image;

    ic.querySelector(`.removeBtn`).addEventListener(`click`, () => deleteImage(image, ic));
  });
});

console.log(imageState);

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

    console.log(imageState);
  } catch {
    console.log("delete error"); // FIX
  }
}

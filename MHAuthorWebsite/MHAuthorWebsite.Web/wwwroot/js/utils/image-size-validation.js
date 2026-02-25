/**
 * image-size-validation.js
 * Mirrors ApplicationRules.Cloudinary.MaxImageSizeMb = 10.
 * Import this module in any page that has a file-upload input.
 */

"use strict";

export const MAX_IMAGE_SIZE_MB = 10;
export const MAX_IMAGE_SIZE_BYTES = MAX_IMAGE_SIZE_MB * 1024 * 1024;

/**
 * Returns true when *any* file in the array exceeds the Cloudinary limit.
 * @param {File[]} files
 */
export function hasOversizedFile(files) {
  return files.some(f => f.size > MAX_IMAGE_SIZE_BYTES);
}

/**
 * Returns the first oversized File, or undefined.
 * @param {File[]} files
 */
export function findOversizedFile(files) {
  return files.find(f => f.size > MAX_IMAGE_SIZE_BYTES);
}

/**
 * Convenience: shows the Bulgarian error message in an element and returns false.
 * @param {HTMLElement} errorEl  - The span / element that should show the error.
 * @param {string} [prefix=""]  - Optional prefix before the standard message.
 * @returns {false}
 */
export function showSizeError(errorEl, prefix = "") {
  errorEl.textContent = `${prefix}Всяко изображение трябва да е до ${MAX_IMAGE_SIZE_MB} MB.`;
  return false;
}

/**
 * Validates a FileList / File[] against the size limit.
 * Shows error in errorEl and returns false when any file is too large.
 * Returns true when all files are within the limit.
 * @param {File[]|FileList} files
 * @param {HTMLElement} errorEl
 */
export function validateImageSizes(files, errorEl) {
  const arr = Array.from(files);
  if (hasOversizedFile(arr)) {
    return showSizeError(errorEl);
  }
  return true;
}

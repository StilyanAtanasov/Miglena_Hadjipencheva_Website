"use strict";

const modal = document.getElementById(`modal`);
const body = modal.querySelector(`#modal-body`);

export const openModal = () => modal.classList.remove(`hidden`);
export const closeModal = () => modal.classList.add(`hidden`);
export const replaceBody = html => (body.innerHTML = html);

modal.querySelector(`.modal-overlay`).addEventListener(`click`, closeModal);
modal.querySelector(`button.modal-close`).addEventListener(`click`, closeModal);

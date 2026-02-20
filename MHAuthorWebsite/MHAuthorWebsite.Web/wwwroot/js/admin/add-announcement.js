"use strict";

import { initQuill } from "../editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const quill = await initQuill(true, true, true);

  const form = document.getElementById(`addAnnouncementForm`);
  const subjectInput = document.getElementById(`subjectInput`);
  const subjectError = document.getElementById(`subject-error`);
  const recipientGroupSelect = document.getElementById(`recipientGroupSelect`);
  const recipientGroupError = document.getElementById(`recipient-group-error`);
  const additionalRecipientsInput = document.getElementById(`additionalRecipientsInput`);
  const additionalRecipientsError = document.getElementById(`additional-recipients-error`);
  const descriptionInput = document.getElementById(`descriptionInput`);
  const messageHtmlInput = document.getElementById(`messageHtmlInput`);
  const descriptionError = document.getElementById(`description-input-error`);

  const subjectMinLength = parseInt(subjectInput.dataset.minLength || `0`);
  const subjectMaxLength = parseInt(subjectInput.dataset.maxLength || `0`);
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  subjectInput.addEventListener(`input`, validateSubject);
  recipientGroupSelect.addEventListener(`change`, validateRecipientGroup);
  additionalRecipientsInput.addEventListener(`input`, clearAdditionalRecipientsError);
  additionalRecipientsInput.addEventListener(`blur`, validateAdditionalRecipients);

  form.addEventListener(`submit`, function (e) {
    if (window.jQuery && !window.jQuery(form).valid()) {
      e.preventDefault();
      return;
    }

    const isSubjectValid = validateSubject();
    const isRecipientGroupValid = validateRecipientGroup();
    const areAdditionalRecipientsValid = validateAdditionalRecipients();

    const counterModule = quill.getModule(`counter`);
    if (counterModule && counterModule.hasError) {
      e.preventDefault();
      quill.container.scrollIntoView({ behavior: `smooth`, block: `center` });
      return;
    }

    const plainText = quill.getText().trim();
    if (plainText.length === 0) {
      e.preventDefault();
      descriptionError.textContent = `Съобщението е задължително.`;
      quill.container.scrollIntoView({ behavior: `smooth`, block: `center` });
      return;
    }

    descriptionError.textContent = ``;

    if (!isSubjectValid || !isRecipientGroupValid || !areAdditionalRecipientsValid) {
      e.preventDefault();
      return;
    }

    const delta = quill.getContents();
    descriptionInput.value = JSON.stringify(delta);
    const semanticHtml = typeof quill.getSemanticHTML === `function` ? quill.getSemanticHTML(0, quill.getLength()) : quill.root.innerHTML;
    if (messageHtmlInput) messageHtmlInput.value = semanticHtml;
  });

  function validateSubject() {
    const subject = subjectInput.value.trim();

    if (subject.length === 0) {
      subjectError.textContent = `Темата е задължителна.`;
      return false;
    }

    if (subjectMinLength > 0 && subject.length < subjectMinLength) {
      subjectError.textContent = `Темата трябва да е поне ${subjectMinLength} символа.`;
      return false;
    }

    if (subjectMaxLength > 0 && subject.length > subjectMaxLength) {
      subjectError.textContent = `Темата трябва да е до ${subjectMaxLength} символа.`;
      return false;
    }

    subjectError.textContent = ``;
    return true;
  }

  function validateRecipientGroup() {
    if (!recipientGroupSelect.value || parseInt(recipientGroupSelect.value) < 1) {
      recipientGroupError.textContent = `Изберете аудитория.`;
      return false;
    }

    recipientGroupError.textContent = ``;
    return true;
  }

  function clearAdditionalRecipientsError() {
    additionalRecipientsError.textContent = ``;
  }

  function validateAdditionalRecipients() {
    const rawValue = additionalRecipientsInput.value;
    const isAdditionalOnlyMode = parseInt(recipientGroupSelect.value) === 4;

    if (isAdditionalOnlyMode && (!rawValue || rawValue.trim() === ``)) {
      additionalRecipientsError.textContent = `При избор "Само допълнителни имейли" трябва да добавите поне един имейл адрес.`;
      return false;
    }

    if (!rawValue || rawValue.trim() === ``) {
      additionalRecipientsError.textContent = ``;
      return true;
    }

    const emails = rawValue
      .split(/[\s,;]+/)
      .map(email => email.trim())
      .filter(email => email !== ``);

    const invalidEmail = emails.find(email => !emailRegex.test(email));
    if (invalidEmail) {
      additionalRecipientsError.textContent = `Невалиден имейл адрес: ${invalidEmail}`;
      return false;
    }

    const uniqueEmails = [...new Set(emails)];
    additionalRecipientsInput.value = uniqueEmails.join(`; `);
    additionalRecipientsError.textContent = ``;
    return true;
  }
});

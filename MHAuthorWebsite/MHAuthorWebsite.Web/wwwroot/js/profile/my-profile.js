document.getElementById(`edit-email-btn`).addEventListener(`click`, function () {
  const emailField = document.getElementById(`email-field`);
  emailField.readOnly = !emailField.readOnly;
  if (!emailField.readOnly) emailField.focus();
});

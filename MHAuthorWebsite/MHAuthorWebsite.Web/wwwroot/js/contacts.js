"use strict";

import { pushNotification } from "./notification.js";
import { executeRecaptchaV3Async, getRecaptchaV2Response, renderRecaptchaV2Async, resetRecaptchaV2 } from "./recaptcha-v3.js";

const AUTOMATION_BLOCK_MESSAGE = "Вашата активност наподобява автоматизирано поведение. Моля, опитайте отново.";
const MANUAL_CAPTCHA_MESSAGE = "Моля, потвърдете ръчно, че не сте робот.";
const SUCCESS_MESSAGE = "Съобщението е изпратено успешно!";
const ERROR_MESSAGE = "Възникна грешка. Опитайте отново.";

const contactForm = document.getElementById("contact-form");
if (!contactForm) throw new Error("Contact form not found.");

const recaptchaV3TokenInput = document.getElementById("contact-recaptcha-v3-token");
const recaptchaV2TokenInput = document.getElementById("contact-recaptcha-v2-token");
const manualCaptchaBox = document.getElementById("contact-manual-recaptcha-box");
const manualCaptchaWidgetHost = document.getElementById("contact-recaptcha-v2-widget");

const recaptchaV3SiteKey = contactForm.dataset.recaptchaV3SiteKey || "";
const recaptchaV2SiteKey = contactForm.dataset.recaptchaV2SiteKey || "";

let requiresManualCaptcha = false;
let manualCaptchaWidgetId = null;

async function ensureManualCaptchaAsync() {
  requiresManualCaptcha = true;
  manualCaptchaBox?.classList.remove("hidden");

  if (manualCaptchaWidgetId !== null || !manualCaptchaWidgetHost) return true;
  manualCaptchaWidgetId = await renderRecaptchaV2Async(
    manualCaptchaWidgetHost,
    recaptchaV2SiteKey,
    token => {
      if (recaptchaV2TokenInput) recaptchaV2TokenInput.value = token || "";
    },
    recaptchaV3SiteKey,
  );

  return manualCaptchaWidgetId !== null;
}

async function submitContactFormAsync() {
  if (!$(contactForm).valid()) return;

  const recaptchaV3Token = await executeRecaptchaV3Async(recaptchaV3SiteKey, "contact_request");
  if (!recaptchaV3Token || !recaptchaV3TokenInput) {
    pushNotification(ERROR_MESSAGE, "error");
    return;
  }

  recaptchaV3TokenInput.value = recaptchaV3Token;

  if (requiresManualCaptcha) {
    const recaptchaV2Token = getRecaptchaV2Response(manualCaptchaWidgetId);
    if (!recaptchaV2Token || !recaptchaV2TokenInput) {
      pushNotification(MANUAL_CAPTCHA_MESSAGE, "warning");
      return;
    }

    recaptchaV2TokenInput.value = recaptchaV2Token;
  }

  const response = await fetch(contactForm.getAttribute("action"), {
    method: "POST",
    headers: {
      RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
    },
    body: new FormData(contactForm),
  });

  if (response.ok) {
    contactForm.reset();
    recaptchaV3TokenInput.value = "";
    recaptchaV2TokenInput.value = "";

    if (manualCaptchaWidgetId !== null) resetRecaptchaV2(manualCaptchaWidgetId);
    pushNotification(SUCCESS_MESSAGE, "success");
    return;
  }

  if (response.status === 428) {
    const payload = await response.json().catch(() => null);
    const didRender = await ensureManualCaptchaAsync();
    pushNotification(payload?.message || (didRender ? MANUAL_CAPTCHA_MESSAGE : AUTOMATION_BLOCK_MESSAGE), "warning");
    return;
  }

  const errorPayload = await response.json().catch(() => null);
  pushNotification(errorPayload?.message || ERROR_MESSAGE, "error");
}

contactForm.addEventListener("submit", async event => {
  event.preventDefault();
  await submitContactFormAsync();
});

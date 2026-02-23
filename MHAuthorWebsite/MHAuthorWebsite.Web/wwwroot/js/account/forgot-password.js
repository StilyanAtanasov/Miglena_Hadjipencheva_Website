"use strict";

import { executeRecaptchaV3Async, getRecaptchaV2Response, renderRecaptchaV2Async } from "../recaptcha-v3.js";

const form = document.getElementById("forgot-password-form");
if (!form) throw new Error("Forgot password form not found.");

const recaptchaV3TokenInput = document.getElementById("forgot-password-recaptcha-v3-token");
const recaptchaV2TokenInput = document.getElementById("forgot-password-recaptcha-v2-token");
const manualCaptchaBox = document.getElementById("forgot-password-manual-recaptcha-box");
const manualCaptchaWidgetHost = document.getElementById("forgot-password-recaptcha-v2-widget");

const recaptchaV3SiteKey = form.dataset.recaptchaV3SiteKey || "";
const recaptchaV2SiteKey = form.dataset.recaptchaV2SiteKey || "";
let requiresManualCaptcha = form.dataset.recaptchaManualRequired === "true";
let manualCaptchaWidgetId = null;
let isProgrammaticSubmit = false;

async function ensureManualCaptchaAsync() {
  requiresManualCaptcha = true;
  manualCaptchaBox?.classList.remove("hidden");

  if (manualCaptchaWidgetId !== null || !manualCaptchaWidgetHost) return;

  manualCaptchaWidgetId = await renderRecaptchaV2Async(
    manualCaptchaWidgetHost,
    recaptchaV2SiteKey,
    token => {
      if (recaptchaV2TokenInput) recaptchaV2TokenInput.value = token || "";
    },
    recaptchaV3SiteKey,
  );
}

document.addEventListener("DOMContentLoaded", async () => {
  if (requiresManualCaptcha) await ensureManualCaptchaAsync();
});

form.addEventListener("submit", async event => {
  if (isProgrammaticSubmit) {
    isProgrammaticSubmit = false;
    return;
  }

  event.preventDefault();

  const recaptchaV3Token = await executeRecaptchaV3Async(recaptchaV3SiteKey, "forgot_password");
  if (!recaptchaV3Token || !recaptchaV3TokenInput) return;

  recaptchaV3TokenInput.value = recaptchaV3Token;

  if (requiresManualCaptcha && recaptchaV2TokenInput) {
    const recaptchaV2Token = getRecaptchaV2Response(manualCaptchaWidgetId);
    if (!recaptchaV2Token) return;

    recaptchaV2TokenInput.value = recaptchaV2Token;
  }

  isProgrammaticSubmit = true;
  form.requestSubmit();
});

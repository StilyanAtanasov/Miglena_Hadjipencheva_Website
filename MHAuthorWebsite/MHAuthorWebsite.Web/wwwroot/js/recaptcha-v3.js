"use strict";

let recaptchaLoadPromise = null;
let loadedRenderMode = null;

function buildRecaptchaApiUrl(v3SiteKey) {
  const renderValue = v3SiteKey ? encodeURIComponent(v3SiteKey) : "explicit";
  return {
    url: `https://www.google.com/recaptcha/api.js?render=${renderValue}`,
    mode: renderValue,
  };
}

function loadRecaptchaApiAsync(v3SiteKey = "") {
  if (typeof window.grecaptcha !== "undefined") return Promise.resolve(window.grecaptcha);

  const { url, mode } = buildRecaptchaApiUrl(v3SiteKey);
  if (recaptchaLoadPromise && loadedRenderMode === mode) return recaptchaLoadPromise;
  if (recaptchaLoadPromise && loadedRenderMode !== mode) return recaptchaLoadPromise;

  loadedRenderMode = mode;

  recaptchaLoadPromise = new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = url;
    script.async = true;
    script.defer = true;
    script.onload = () => resolve(window.grecaptcha);
    script.onerror = () => reject(new Error("Failed to load reCAPTCHA API"));
    document.head.appendChild(script);
  });

  return recaptchaLoadPromise;
}

export async function executeRecaptchaV3Async(siteKey, action) {
  if (!siteKey) return null;

  try {
    const grecaptcha = await loadRecaptchaApiAsync(siteKey);
    return await new Promise(resolve => {
      grecaptcha.ready(async () => {
        try {
          const token = await grecaptcha.execute(siteKey, { action });
          resolve(token || null);
        } catch {
          resolve(null);
        }
      });
    });
  } catch {
    return null;
  }
}

export async function renderRecaptchaV2Async(container, siteKey, onSuccess, v3BootstrapSiteKey = "") {
  if (!container || !siteKey) return null;

  try {
    const grecaptcha = await loadRecaptchaApiAsync(v3BootstrapSiteKey);
    return grecaptcha.render(container, {
      sitekey: siteKey,
      callback: onSuccess,
    });
  } catch {
    return null;
  }
}

export function getRecaptchaV2Response(widgetId) {
  if (typeof window.grecaptcha === "undefined" || widgetId === null || widgetId === undefined) return "";
  return window.grecaptcha.getResponse(widgetId);
}

export function resetRecaptchaV2(widgetId) {
  if (typeof window.grecaptcha === "undefined" || widgetId === null || widgetId === undefined) return;
  window.grecaptcha.reset(widgetId);
}

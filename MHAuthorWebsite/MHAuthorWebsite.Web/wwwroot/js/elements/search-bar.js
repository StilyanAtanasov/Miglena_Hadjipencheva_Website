import { pushNotification } from "../notification.js";
import { executeRecaptchaV3Async, getRecaptchaV2Response, renderRecaptchaV2Async, resetRecaptchaV2 } from "../recaptcha-v3.js";
import { injectLoader } from "./loader.js";

const AUTOMATION_BLOCK_MESSAGE = "Вашата активност наподобява автоматизирано поведение. Моля, опитайте отново.";
const MANUAL_CAPTCHA_MESSAGE = "Моля, потвърдете ръчно, че не сте робот.";

export class SearchBarHandler {
  constructor(config) {
    this.input = document.querySelector(config.inputSelector);
    this.target = document.querySelector(config.targetSelector);
    this.form = document.querySelector(config.formSelector);
    this.url = config.url;
    this.param = config.param || "search";
    this.debounceTimeoutMilliseconds = config.debounceTimeoutMilliseconds || 300;
    this.enableLoader = config.enableLoader !== undefined ? config.enableLoader : true;
    this.resetParams = config.resetParams || [];
    this.submitBtnLoaderConfig = config.submitBtnLoaderConfig || {
      size: "small",
      theme: "light",
      screenColor: "var(--color-secondary)",
    };
    this.targetLoaderConfig = config.targetLoaderConfig || { size: "large" };

    this.debounceTimer = null;
    this.abortController = null;
    this.submitBtn = this.form?.querySelector(".search-btn");

    this.recaptchaV3Enabled = this.form?.dataset.recaptchaV3Enabled === "true";
    this.recaptchaV3SiteKey = this.form?.dataset.recaptchaV3SiteKey || "";
    this.recaptchaV3Action = this.form?.dataset.recaptchaV3Action || "search_query";
    this.recaptchaV2SiteKey = this.form?.dataset.recaptchaV2SiteKey || "";

    this.manualCaptchaContainer = this.form?.parentElement?.querySelector(".search-manual-captcha-box");
    this.manualCaptchaWidgetHost = this.form?.parentElement?.querySelector(".search-recaptcha-v2-widget");
    this.recaptchaV2TokenInput = this.form?.querySelector(".recaptcha-v2-token");
    this.requiresManualCaptcha = false;
    this.manualCaptchaWidgetId = null;

    if (this.input && this.target && this.form) this.init();
    else throw new Error("Search bar not properly configured!");
  }

  init() {
    this.input.addEventListener("input", event => {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => this.performSearch(event.target.value), this.debounceTimeoutMilliseconds);
    });

    this.form.addEventListener("submit", event => {
      event.preventDefault();
      clearTimeout(this.debounceTimer);
      this.performSearch(this.input.value);
    });
  }

  async ensureManualCaptchaAsync() {
    this.requiresManualCaptcha = true;
    this.manualCaptchaContainer?.classList.remove("hidden");

    if (this.manualCaptchaWidgetId !== null || !this.manualCaptchaWidgetHost) return true;
    this.manualCaptchaWidgetId = await renderRecaptchaV2Async(
      this.manualCaptchaWidgetHost,
      this.recaptchaV2SiteKey,
      token => {
        if (this.recaptchaV2TokenInput) this.recaptchaV2TokenInput.value = token || "";
      },
      this.recaptchaV3SiteKey,
    );

    return this.manualCaptchaWidgetId !== null;
  }

  async getRecaptchaHeadersAsync(query) {
    const headers = {};

    if (this.recaptchaV3Enabled && query.trim() !== "") {
      const recaptchaV3Token = await executeRecaptchaV3Async(this.recaptchaV3SiteKey, this.recaptchaV3Action);
      if (!recaptchaV3Token) {
        pushNotification(AUTOMATION_BLOCK_MESSAGE, "error");
        return null;
      }

      headers["X-Recaptcha-Token"] = recaptchaV3Token;
    }

    if (this.requiresManualCaptcha) {
      const recaptchaV2Token = getRecaptchaV2Response(this.manualCaptchaWidgetId);
      if (!recaptchaV2Token) {
        pushNotification(MANUAL_CAPTCHA_MESSAGE, "warning");
        return null;
      }

      headers["X-Recaptcha-V2-Token"] = recaptchaV2Token;
    }

    return headers;
  }

  async handleManualCaptchaRequiredAsync(payload) {
    const didRender = await this.ensureManualCaptchaAsync();
    pushNotification(payload?.message || (didRender ? MANUAL_CAPTCHA_MESSAGE : AUTOMATION_BLOCK_MESSAGE), "warning");
  }

  resetManualCaptcha() {
    resetRecaptchaV2(this.manualCaptchaWidgetId);
    if (this.recaptchaV2TokenInput) this.recaptchaV2TokenInput.value = "";
  }

  async performSearch(query) {
    if (this.abortController) this.abortController.abort();
    this.abortController = new AbortController();

    let localBtnLoader = null;
    let localTargetLoader = null;

    if (this.submitBtn) {
      this.submitBtn.disabled = true;
      localBtnLoader = injectLoader(this.submitBtn, this.submitBtnLoaderConfig);
    }

    if (this.enableLoader) localTargetLoader = injectLoader(this.target, this.targetLoaderConfig);

    try {
      const currentUrlParams = new URLSearchParams(window.location.search);

      const recaptchaHeaders = await this.getRecaptchaHeadersAsync(query);
      if (recaptchaHeaders === null) return;

      query.trim() === "" ? currentUrlParams.delete(this.param) : currentUrlParams.set(this.param, query);
      this.resetParams.forEach(param => currentUrlParams.delete(param));

      const queryString = currentUrlParams.toString();
      const requestUrl = queryString ? `${this.url}?${queryString}` : this.url;

      const headers = { "X-Requested-With": "XMLHttpRequest", ...recaptchaHeaders };

      const response = await fetch(requestUrl, {
        headers,
        signal: this.abortController.signal,
      });

      if (response.status === 428) {
        const payload = await response.json().catch(() => null);
        await this.handleManualCaptchaRequiredAsync(payload);
        return;
      }

      if (!response.ok) {
        const payload = await response.json().catch(() => null);
        pushNotification(payload?.message || "Възникна грешка при търсенето!", "error");
        return;
      }

      const html = await response.text();
      this.target.innerHTML = html;
      if (this.requiresManualCaptcha) this.resetManualCaptcha();
      window.history.pushState(null, "", requestUrl);
    } catch (error) {
      if (error.name !== "AbortError") {
        console.error("Search failed:", error);
        pushNotification("Грешка при търсенето!", "error");
      }
    } finally {
      if (localBtnLoader) localBtnLoader.close();
      if (localTargetLoader) localTargetLoader.close();
      if (this.submitBtn) this.submitBtn.disabled = false;
    }
  }
}

import { pushNotification } from "../notification.js";

const MANUAL_CAPTCHA_MESSAGE = "Моля, потвърдете ръчно, че не сте робот.";
const AUTOMATION_BLOCK_MESSAGE = "Вашата активност наподобява автоматизирано поведение. Моля, опитайте отново.";

export class PaginationHandler {
  constructor(config) {
    this.container = document.querySelector(config.containerSelector);
    this.url = config.url;
    this.recaptchaHandler = config.recaptchaHandler;
    this.abortController = null;

    if (this.container) this.init();
  }

  init() {
    this.container.addEventListener(`click`, e => {
      const link = e.target.closest(`.pagination-link`);

      if (link) {
        e.preventDefault();
        const urlObj = new URL(link.href, window.location.origin);
        this.changePage(urlObj.searchParams);
      }
    });
  }

  async changePage(newParams) {
    if (this.abortController) this.abortController.abort();
    this.abortController = new AbortController();

    try {
      const currentUrlParams = new URLSearchParams(window.location.search);
      newParams.forEach((value, key) => {
        currentUrlParams.set(key, value);
      });

      const recaptchaHeaders = this.recaptchaHandler ? await this.recaptchaHandler.getRecaptchaHeadersAsync(currentUrlParams.get("search") || "") : {};
      if (recaptchaHeaders === null) return;

      const response = await fetch(`${this.url}?${currentUrlParams.toString()}`, {
        headers: { "X-Requested-With": "XMLHttpRequest", ...recaptchaHeaders },
        signal: this.abortController.signal,
      });

      if (response.status === 428) {
        const payload = await response.json().catch(() => null);
        if (this.recaptchaHandler) await this.recaptchaHandler.handleManualCaptchaRequiredAsync(payload);
        else pushNotification(payload?.message || MANUAL_CAPTCHA_MESSAGE, "warning");
        return;
      }

      if (!response.ok) {
        pushNotification(AUTOMATION_BLOCK_MESSAGE, "error");
        return;
      }

      const html = await response.text();

      this.container.innerHTML = html;
      if (this.recaptchaHandler?.requiresManualCaptcha) this.recaptchaHandler.resetManualCaptcha();

      window.history.pushState(null, ``, `?${currentUrlParams.toString()}`);
    } catch (error) {
      if (error.name !== `AbortError`) console.error(error);
    }
  }
}

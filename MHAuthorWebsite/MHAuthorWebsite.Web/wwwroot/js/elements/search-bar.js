import { pushNotification } from "../notification.js";
import { injectLoader } from "./loader.js";

export class SearchBarHandler {
  constructor(config) {
    this.input = document.querySelector(config.inputSelector);
    this.target = document.querySelector(config.targetSelector);
    this.form = document.querySelector(config.formSelector);
    this.url = config.url;
    this.param = config.param || `search`;
    this.debounceTimeoutMilliseconds = config.debounceTimeoutMilliseconds || 300;
    this.debounceTimer = null;
    this.abortController = null;
    this.submitBtn = this.form?.querySelector(`.search-btn`);
    this.loader = null;
    this.enableLoader = config.enableLoader || true;
    this.resetParams = config.resetParams || [];
    this.submitBtnLoaderConfig = config.submitBtnLoaderConfig || {
      size: `small`,
      theme: `light`,
      screenColor: `var(--color-secondary)`,
    };
    this.targetLoaderConfig = config.targetLoaderConfig || {
      size: `large`,
    };

    console.log(this.targetLoaderConfig);

    if (this.input && this.target && this.form) this.init();
    else {
      pushNotification(`Полето за търсене е неактивно!`, `warning`);
      console.error(`Search bar not properly configured!`);
      throw new Error(`Search bar not properly configured!`);
    }
  }

  init() {
    this.input.addEventListener(`input`, e => {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => this.performSearch(e.target.value), this.debounceTimeoutMilliseconds);
    });

    if (this.form) {
      this.form.addEventListener(`submit`, e => {
        e.preventDefault();
        clearTimeout(this.debounceTimer);
        this.performSearch(this.input.value);
      });
    }
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

    if (this.enableLoader) {
      localTargetLoader = injectLoader(this.target, this.targetLoaderConfig);
    }

    try {
      const currentUrlParams = new URLSearchParams(window.location.search);

      query.trim() === "" ? currentUrlParams.delete(this.param) : currentUrlParams.set(this.param, query);

      this.resetParams.forEach(p => currentUrlParams.delete(p));

      const queryString = currentUrlParams.toString();
      const requestUrl = queryString ? `${this.url}?${queryString}` : this.url;

      const response = await fetch(requestUrl, {
        headers: { "X-Requested-With": "XMLHttpRequest" },
        signal: this.abortController.signal,
      });

      const html = await response.text();
      this.target.innerHTML = html;
      window.history.pushState(null, ``, requestUrl);
    } catch (error) {
      if (error.name !== `AbortError`) {
        console.error(`Search failed:`, error);
        pushNotification(`Грешка при търсенето!`, `error`);
      }
    } finally {
      if (localBtnLoader) localBtnLoader.close();
      if (localTargetLoader) localTargetLoader.close();
      if (this.submitBtn) this.submitBtn.disabled = false;
    }
  }
}

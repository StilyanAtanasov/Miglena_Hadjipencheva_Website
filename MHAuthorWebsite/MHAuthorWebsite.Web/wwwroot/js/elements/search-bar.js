import { pushNotification } from "../notification.js";

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
    if (this.abortController) {
      this.abortController.abort();
    }
    this.abortController = new AbortController();

    if (this.submitBtn) {
      this.submitBtn.disabled = true;
      this.submitBtn.innerHTML = ``;
      this.submitBtn.classList.add(`loading`);
    }

    try {
      const currentUrlParams = new URLSearchParams(window.location.search);

      query.trim() === "" ? currentUrlParams.delete(this.param) : currentUrlParams.set(this.param, query);

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
      if (error.name === `AbortError`) {
        console.log(`Fetch aborted: newer search started.`);
        pushNotification(`Грешка при повторно търсене!`, `error`);
      } else {
        console.error(`Search failed:`, error);
        pushNotification(`Грешка при търсенето!`, `error`);
      }
    } finally {
      if (this.submitBtn) {
        this.submitBtn.disabled = false;
        this.submitBtn.innerHTML = `<i class="fa-solid fa-magnifying-glass"></i> Търси`;
        this.submitBtn.classList.remove(`loading`);
      }
    }
  }
}

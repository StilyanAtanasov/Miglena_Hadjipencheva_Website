export class PaginationHandler {
  constructor(config) {
    this.container = document.querySelector(config.containerSelector);
    this.url = config.url;
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

      const response = await fetch(`${this.url}?${currentUrlParams.toString()}`, {
        headers: { "X-Requested-With": "XMLHttpRequest" },
        signal: this.abortController.signal,
      });

      const html = await response.text();

      this.container.innerHTML = html;

      window.history.pushState(null, ``, `?${currentUrlParams.toString()}`);
    } catch (error) {
      if (error.name !== `AbortError`) console.error(error);
    }
  }
}

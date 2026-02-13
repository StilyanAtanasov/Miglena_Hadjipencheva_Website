"use strict";

import { SearchBarHandler } from "../elements/search-bar.js";
import { PaginationHandler } from "../elements/pagination-handler.js";
import { formatLocalDates } from "../time-zone-manager.js";

document.addEventListener(`DOMContentLoaded`, function () {
  const searchForm = document.getElementById(`announcements-search-form`);
  const searchInput = document.getElementById(`announcements-search-input`);

  if (searchForm && searchInput) {
    new SearchBarHandler({
      inputSelector: `#announcements-search-input`,
      formSelector: `#announcements-search-form`,
      targetSelector: `#announcements-content`,
      url: `/Admin/AdminAnnouncements/AnnouncementsBoard`,
      param: `search`,
      debounceTimeoutMilliseconds: 500,
      resetParams: [`page`],
    });
  }

  const paginationContainer = document.querySelector(`.pagination-box`);
  if (paginationContainer) {
    new PaginationHandler({
      containerSelector: `#announcements-content`,
      url: `/Admin/AdminAnnouncements/AnnouncementsBoard`,
    });
  }

  const contentContainer = document.getElementById(`announcements-content`);
  if (contentContainer) {
    const observer = new MutationObserver(() => formatLocalDates());
    observer.observe(contentContainer, { childList: true, subtree: true });
  }
});

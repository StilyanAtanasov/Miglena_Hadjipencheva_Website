import { SearchBarHandler } from "./elements/search-bar.js";
import { PaginationHandler } from "./elements/pagination-handler.js";

document.addEventListener("DOMContentLoaded", function () {
  // Initialize search bar
  const searchForm = document.getElementById("work-search-form");
  const searchInput = document.getElementById("work-search-input");

  if (searchForm && searchInput) {
    new SearchBarHandler({
      inputSelector: "#work-search-input",
      formSelector: "#work-search-form",
      targetSelector: "#works-grid",
      url: "/Work",
      param: "search",
      debounceTimeoutMilliseconds: 500,
      resetParams: [`page`],
    });
  }

  // Initialize pagination
  const paginationContainer = document.querySelector(".pagination-box");
  if (paginationContainer) {
    new PaginationHandler({
      containerSelector: "#works-container",
      url: "/Work",
    });
  }
});

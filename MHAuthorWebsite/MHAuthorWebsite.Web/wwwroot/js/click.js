"use strict";

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".clickable").forEach(function (div) {
    div.addEventListener("click", function (e) {
      if (e.target.closest("a, button, input")) return;
      window.location = div.getAttribute("data-details-url");
    });
  });
});

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".product").forEach(function (div) {
    div.addEventListener("click", function (e) {
      // Prevent navigation if a button, link, or form element was clicked
      if (e.target.closest("a, button, form")) return;
      window.location = div.getAttribute("data-details-url");
    });
  });
});

document.addEventListener("DOMContentLoaded", function () {
  const toggleButtons = document.querySelectorAll(".password-toggle-btn");

  toggleButtons.forEach(button => {
    button.addEventListener("click", function () {
      const wrapper = button.closest(".password-input-wrapper");
      if (!wrapper) return;

      const input = wrapper.querySelector("input");
      if (!input) return;

      const icon = button.querySelector("i");
      
      if (input.type === "password") {
        input.type = "text";
        if (icon) {
          icon.classList.remove("fa-eye");
          icon.classList.add("fa-eye-slash");
        }
      } else {
        input.type = "password";
        if (icon) {
          icon.classList.remove("fa-eye-slash");
          icon.classList.add("fa-eye");
        }
      }
    });
  });
});

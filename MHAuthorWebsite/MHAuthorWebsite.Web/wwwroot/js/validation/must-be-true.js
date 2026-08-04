"use strict";

if (window.jQuery?.validator?.unobtrusive) {
  $.validator.addMethod(
    "mustbetrue",
    function (_value, element) {
      return element.checked === true;
    },
    "",
  );

  $.validator.unobtrusive.adapters.addBool("mustbetrue");
}

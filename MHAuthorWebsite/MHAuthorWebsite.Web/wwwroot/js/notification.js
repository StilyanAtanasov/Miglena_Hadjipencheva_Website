"use strict";

import Swal from "https://cdn.jsdelivr.net/npm/sweetalert2@11.22.3/dist/sweetalert2.esm.js";

const defaultNotificationDuration = 5000;

export function pushNotification(message, icon = "success", duration = defaultNotificationDuration) {
  Swal.fire({
    toast: true,
    position: "top-end",
    icon: icon,
    title: message,
    showConfirmButton: false,
    timer: duration,
    timerProgressBar: true,
    showCloseButton: true,
    didOpen: toast => {
      toast.addEventListener("mouseenter", Swal.stopTimer);
      toast.addEventListener("mouseleave", Swal.resumeTimer);
    },
  });
}

export async function showPopupAsync(args) {
  const {
    icon = "success",
    title = "",
    text = "",
    duration,
    showCancelButton = false,
    showCloseButton = false,
    confirmButtonColor,
    cancelButtonColor,
    confirmButtonText = `Потвърди`,
    cancelButtonText = `Отказ`,
    allowOutsideClick = false,
    onConfirm,
    onConfirmArgs = null,
    onCancel,
    onCancelArgs = null,
    onClose,
    onCloseArgs = null,
    onBackdrop,
    onBackdropArgs = null,
  } = args;

  const result = await Swal.fire({
    icon,
    title,
    text,
    showConfirmButton: !duration, // if duration is set, auto-close
    timer: duration,
    showCloseButton,
    showCancelButton,
    confirmButtonColor,
    cancelButtonColor,
    confirmButtonText,
    cancelButtonText,
    allowOutsideClick,
  });

  if (result.isConfirmed) {
    onConfirm?.(...onConfirmArgs);
  } else if (result.isDismissed) {
    switch (result.dismiss) {
      case Swal.DismissReason.cancel:
        onCancel?.(...onCancelArgs);
        break;
      case Swal.DismissReason.close:
        onClose?.(...onCloseArgs);
        break;
      case Swal.DismissReason.backdrop:
        onBackdrop?.(...onBackdropArgs);
        break;
    }
  }
}

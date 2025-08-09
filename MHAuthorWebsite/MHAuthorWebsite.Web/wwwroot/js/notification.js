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
    didOpen: toast => {
      toast.addEventListener("mouseenter", Swal.stopTimer);
      toast.addEventListener("mouseleave", Swal.resumeTimer);
    },
  });
}

import { initQuill } from "./editor.js";

document.addEventListener("DOMContentLoaded", async function () {
  // Initialize Quill in read-only mode
  await initQuill(false, false);
});

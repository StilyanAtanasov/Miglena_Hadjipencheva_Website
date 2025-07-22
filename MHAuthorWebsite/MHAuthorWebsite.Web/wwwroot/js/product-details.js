import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  await initQuill(false, false);
});

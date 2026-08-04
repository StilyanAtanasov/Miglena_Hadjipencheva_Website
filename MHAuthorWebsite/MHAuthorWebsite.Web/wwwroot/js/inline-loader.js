/**
 * @param {HTMLElement} parent - The container.
 * @param {boolean} show - Toggle visibility.
 * @param {string} size - Optional CSS size (e.g., '2rem', '20px', '100%').
 */
export function toggleLoader(parent, show, size = null) {
  let loaderWrapper = parent.querySelector(`.inline-loader-wrapper`);

  if (show) {
    if (!loaderWrapper) {
      loaderWrapper = document.createElement(`div`);
      loaderWrapper.className = `inline-loader-wrapper`;

      // Apply the size variable if provided
      const style = size ? `style="--loader-size: ${size};"` : ``;

      loaderWrapper.innerHTML = `
                <div class="loader-inner" ${style}>
                    <div class="spinner"></div>
                </div>`;
      parent.appendChild(loaderWrapper);
    }
  } else if (loaderWrapper) loaderWrapper.remove();
}

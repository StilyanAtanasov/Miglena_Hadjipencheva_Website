export const injectLoader = (container, config = {}) => {
  const { size = `medium`, customWidth = ``, customHeight = ``, theme = `dark`, screenColor = null } = config;

  const screen = document.createElement(`div`);
  screen.className = `loading-screen show`;
  if (screenColor) screen.style.backgroundColor = screenColor;

  const loader = document.createElement(`div`);
  loader.className = `loader ${size} ${theme}`;

  if (customWidth) loader.style.width = customWidth;
  if (customHeight) loader.style.height = customHeight;

  screen.appendChild(loader);

  if (window.getComputedStyle(container).position === `static`) container.style.position = `relative`;

  container.appendChild(screen);

  return {
    close: () => {
      screen.classList.replace(`show`, `hide`);
      setTimeout(() => screen.remove(), 200);
    },
  };
};

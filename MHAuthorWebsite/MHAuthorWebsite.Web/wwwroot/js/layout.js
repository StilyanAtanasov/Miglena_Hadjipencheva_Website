"use strict";

// --- Header ---
const header = document.querySelector(`header`);
const bars = document.getElementById(`bars`);
const navLists = document.querySelectorAll(`.header-nav`);
const navListsBox = document.getElementById(`navLists`);

let isNavOpened = false;

bars.addEventListener(`click`, () => toggleNavLists(navLists));

function toggleNavLists(navLists) {
  isNavOpened = !isNavOpened;
  navListsBox.classList.toggle(`show`);
  navListsBox.classList.toggle(`hide`);
  bars.classList.toggle(`x-mark`);
  header.classList.toggle(`mobile-nav`);
  header.classList.toggle(`mobile-nav-closed`);

  if (isNavOpened) {
    navListsBox.innerHTML = ``;
    navLists.forEach(l => {
      navListsBox.appendChild(l.cloneNode(true));
    });
  } else {
    setTimeout(() => (navListsBox.innerHTML = ``), 500);
  }
}

// --- Footer ---
const items = document.querySelectorAll(`.social-media li`);

items.forEach((item, i) => {
  item.addEventListener(`mouseenter`, () => {
    items.forEach((other, j) => {
      if (j < i) other.classList.add(`left`);
      else if (j > i) other.classList.add(`right`);
    });
  });

  item.addEventListener(`mouseleave`, () => {
    items.forEach(other => other.classList.remove(`left`, `right`));
  });
});

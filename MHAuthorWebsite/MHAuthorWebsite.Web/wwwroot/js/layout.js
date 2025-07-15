const items = document.querySelectorAll(".social-media li");

items.forEach((item, i) => {
  item.addEventListener("mouseenter", () => {
    items.forEach((other, j) => {
      if (j < i) other.classList.add("left");
      else if (j > i) other.classList.add("right");
    });
  });

  item.addEventListener("mouseleave", () => {
    items.forEach((other) => other.classList.remove("left", "right"));
  });
});

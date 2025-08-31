function triggerAnimation(entries, observer) {
  entries.forEach(e => {
    if (e.isIntersecting) {
      const target = e.target;

      target.classList.add(`animate`);
      observer.unobserve(target);
    }
  });
}

const observer = new IntersectionObserver(triggerAnimation, {
  threshold: 0.5,
});

document.querySelectorAll(`.animate-on-scroll`).forEach(s => observer.observe(s));

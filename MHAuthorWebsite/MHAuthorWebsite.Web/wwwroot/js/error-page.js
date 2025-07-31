const animatedEl = document.getElementById(`error-title-container`);

function loopAnimation() {
  animatedEl.classList.add(`spin`);
  setTimeout(() => {
    animatedEl.classList.remove(`spin`);
    setTimeout(loopAnimation, 5000); // pause before next spin
  }, 400); // duration of spin
}

loopAnimation();

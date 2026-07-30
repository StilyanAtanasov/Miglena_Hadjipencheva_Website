"use strict";

// TODO use my own quotes
const footerQuotes = [
  "Всяка трудност е стъпало към по-силна версия на теб.",
  "Когато сърцето е спокойно, пътят се подрежда сам.",
  "Най-красивите неща узряват бавно.",
  "Това, което днес е усилие, утре е увереност.",
  "Истинската промяна започва с една малка смела крачка.",
  "Тишината често казва повече от най-силните думи.",
  "Пази добротата си. Тя е сила, не слабост.",
];

const footerQuoteTextId = `footer-quote-text`;
const footerQuoteRegenerateBtnId = `footer-quote-regenerate-btn`;

let lastFooterQuoteIndex = -1;

function pickRandomQuoteIndex() {
  if (footerQuotes.length === 1) return 0;

  let randomIndex;
  do {
    randomIndex = Math.floor(Math.random() * footerQuotes.length);
  } while (randomIndex === lastFooterQuoteIndex);

  return randomIndex;
}

function renderRandomFooterQuote() {
  const quoteTextElement = document.getElementById(footerQuoteTextId);
  if (!quoteTextElement || footerQuotes.length === 0) return;

  const quoteIndex = pickRandomQuoteIndex();
  lastFooterQuoteIndex = quoteIndex;
  quoteTextElement.textContent = `„${footerQuotes[quoteIndex]}“`;
}

function setupFooterQuoteGenerator() {
  const quoteTextElement = document.getElementById(footerQuoteTextId);
  const regenerateButton = document.getElementById(footerQuoteRegenerateBtnId);
  if (!quoteTextElement || !regenerateButton) return;

  renderRandomFooterQuote();
  regenerateButton.addEventListener(`click`, renderRandomFooterQuote);
}

if (document.readyState === `loading`) {
  document.addEventListener(`DOMContentLoaded`, setupFooterQuoteGenerator);
} else {
  setupFooterQuoteGenerator();
}

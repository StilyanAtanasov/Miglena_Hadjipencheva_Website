"use strict";

export function formatLocalDates() {
  const dateElements = document.querySelectorAll(`time[to-local]`);

  dateElements.forEach(el => {
    const utcString = el.getAttribute(`datetime`);
    if (!utcString) return;

    const date = new Date(utcString);
    if (isNaN(date.getTime())) return;

    const formatOptions = {
      day: `2-digit`,
      month: el.hasAttribute(`month-long`) ? `long` : `2-digit`,
      year: `numeric`,
    };

    if (!el.hasAttribute(`date-only`)) {
      formatOptions.hour = `2-digit`;
      formatOptions.hour12 = false;
      formatOptions.minute = `2-digit`;
      if (!el.hasAttribute(`no-seconds`)) formatOptions.second = `2-digit`;
    }

    const formattedDate = new Intl.DateTimeFormat(navigator.language, formatOptions).format(date);

    if (el.textContent.trim() !== formattedDate) el.textContent = formattedDate;
  });
}

document.addEventListener(`DOMContentLoaded`, formatLocalDates);

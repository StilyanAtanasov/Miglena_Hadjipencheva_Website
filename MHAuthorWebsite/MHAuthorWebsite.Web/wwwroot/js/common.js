export function parseBgNumber(value) {
  if (typeof value !== "string") return NaN;
  return Number(value.replace(",", "."));
}

export function formatBgNumber(value, decimals = 2) {
  return new Intl.NumberFormat(`bg-BG`, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value);
}

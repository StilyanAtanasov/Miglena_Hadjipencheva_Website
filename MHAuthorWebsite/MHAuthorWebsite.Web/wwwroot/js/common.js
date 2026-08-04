export function parseBgNumber(value) {
  if (typeof value === "number") return value;
  if (typeof value !== "string") return NaN;

  const normalizedValue = value.trim().replace(/\s/g, "").replace(",", ".");
  return Number(normalizedValue);
}

export function formatBgNumber(value, decimals = 2) {
  const numericValue = parseBgNumber(value);
  if (!Number.isFinite(numericValue)) return "";

  const parts = new Intl.NumberFormat(`bg-BG`, {
    useGrouping: true,
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).formatToParts(numericValue);

  return parts.map(part => (part.type === `decimal` ? `,` : part.type === `group` ? ` ` : part.value)).join("");
}

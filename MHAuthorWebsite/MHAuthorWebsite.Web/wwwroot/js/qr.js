window.addEventListener("load", () => {
  const fontSize = +getComputedStyle(document.documentElement).fontSize.replace(`px`, ``);
  const length = fontSize * 25;

  new QRCode(document.getElementById("qrCode"), {
    text: document.getElementById("qrCodeData").getAttribute("data-url"),
    width: length,
    height: length,
  });
});

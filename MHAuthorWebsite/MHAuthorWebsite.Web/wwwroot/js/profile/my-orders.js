import { toggleLoader } from "../inline-loader.js";

const ordersContainer = document.querySelector(`.content-body`);
let currentPage = 1;
let isFetching = false;
let hasMoreData = true;

toggleLoader(ordersContainer, true);

// --- Create a "Sentinel" element at the bottom of the list
const sentinel = document.createElement(`div`);
sentinel.id = `infinite-scroll-sentinel`;
sentinel.style.height = `10px`;
ordersContainer.appendChild(sentinel);

const observerOptions = {
  root: null, // - Use the viewport
  rootMargin: `200px`, // - Trigger 200px before the user reaches the end
  threshold: 0.1,
};

const observer = new IntersectionObserver(async entries => {
  const entry = entries[0];

  if (entry.isIntersecting && !isFetching && hasMoreData) await loadNextPage();
}, observerOptions);

observer.observe(sentinel);

async function loadNextPage() {
  isFetching = true;

  // Show loader inside the container, but before the sentinel
  toggleLoader(ordersContainer, true);

  try {
    const response = await fetch(`/Order/MyOrders?page=${++currentPage}`, {
      method: `GET`,
      headers: { "X-Requested-With": `XMLHttpRequest` },
    });

    if (response.ok) {
      const html = await response.text();

      !html || html.trim().length === 0 ? stopObserving() : sentinel.insertAdjacentHTML(`beforebegin`, html);
    } else stopObserving();
  } catch (error) {
    console.error(`Error loading orders:`, error);
    stopObserving();
  } finally {
    isFetching = false;
    toggleLoader(ordersContainer, false);
  }
}

function stopObserving() {
  hasMoreData = false;
  observer.unobserve(sentinel);
  sentinel.remove();
}

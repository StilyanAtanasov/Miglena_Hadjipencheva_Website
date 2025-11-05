# Changelog

## - v1.0.0 - 2025-10-12

### General

- First stable release of the MHAuthorWebsite: A modern, secure, and scalable ASP.NET Core web application dedicated to the Bulgarian author Miglena Hadjipencheva.

- Public platform for readers to explore and order her literary works.

- Admin interface for managing publications, content, and related digital assets efficiently.

### NOTE:

- The application might still have some minor bugs or by far unimplemented functionalities that will be covered in the uncoming application updates and patches.

## Patch v1.0.1 — 2025-11-05

### 🛠 Improvements

- Improved **Admin Order Details** page UX.
- Significant enhancements to **Product Comments**:
  - Added comment image previews.
  - Refined overall comment and reply visual design.
  - Implemented pagination for comments and replies.
  - Added filtering of comments by rating.
  - Improved average rating star visualization.
  - Added detailed comment info modal.
  - Enabled editing and deleting of comments by users.
  - Introduced anti-spam rate limits and security restrictions.

### ✅ Fixes

- Users can no longer access the order page when the cart is empty.
- Fixed input label overflow in the **Econt** shipping form on mobile layouts.
- Resolved issue where OAuth providers' profile data was not stored correctly.
- Prices are now consistently displayed in **BGN and EUR**.
- Fixed non-working **Delete Personal Data** action.

### ✨ New Features

- Added dedicated **Order Success** confirmation page, available only immediately after an order.
- Introduced environment-specific popup warning in **Staging**.
- Added unified **app icon / favicon / logo** definitions.

# README screenshots

Captured on September 10, 2026 from the running Next.js production build in headless Microsoft Edge. These are web application captures; the phone-width images do not show the Android app.

| File | Viewport | State |
| --- | --- | --- |
| `web-login-desktop.png` | 1440 × 960 | Empty login form |
| `web-login-mobile.png` | 390 × 844 | Empty login form |
| `web-login-validation.png` | 390 × 844 | Required-field errors after submitting the empty form |

To refresh them, run `npm run build` and `npm run start` from `web/`, open `/login` in Edge, and set the viewport dimensions above with DevTools device emulation. Capture the full page as PNG; the validation page extends beyond its viewport. Use an empty form so the images contain no personal credentials. The API is not needed for these three states.

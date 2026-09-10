# Account Architecture Decisions

## Controller-based API

Use controllers instead of Minimal APIs for account endpoints. HomeHarmony is expected to grow across multiple business areas, and controllers provide a familiar, consistent structure with built-in request validation.

## Login request validation

Login uses email, not username. Email and password are required, and email must have a valid email format.

Login does not enforce password length or password-creation policy. Those rules belong to registration and will later be handled by ASP.NET Core Identity.

Both clients validate obvious form errors for usability. API validation remains the trust boundary because callers can bypass client validation.

## Future authentication

ASP.NET Core Identity has been chosen for real authentication. The intended direction is cookie authentication for Web and Identity's bearer/access token response for Android. Identity will own the actual authentication result.

These are future decisions, not implemented behavior. Identity integration, database/user persistence, cookie sessions, token storage and refresh, registration, password recovery, household authorization, and protected routes/endpoints are outside this mock login slice. Create user, forgot password, and policies actions remain placeholders.

## Client feedback and tests

Both clients show field validation beside the relevant input and invalid credentials at form level. Network, server, and unexpected failures use transient feedback (a Compose Snackbar on Android and a Toast on Web). User-facing messages do not expose technical exception details.

Keep tests focused on login validation, success/failure handling, and the empty success response contract. Use API tests, Android JVM unit tests, and lightweight Web unit tests; a larger test architecture or browser end-to-end suite is not needed for this feature.

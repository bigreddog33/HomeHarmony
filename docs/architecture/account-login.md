# Account Architecture Decisions

## Controller-based API

Use controllers instead of Minimal APIs for account endpoints. HomeHarmony is expected to grow across multiple business areas, and controllers provide a familiar, consistent structure with built-in request validation.

## Login request validation

Login does not enforce password length or password-creation policy. Those rules belong to registration and will later be handled by ASP.NET Core Identity.

## Login response deferred

`LoginResponse` is intentionally empty until cookie-based or token-based authentication is chosen. The current mock validates credentials but does not persist the login.

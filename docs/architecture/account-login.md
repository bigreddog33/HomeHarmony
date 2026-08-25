# Account Architecture Decisions

## Controller-based API

Use controllers instead of Minimal APIs for account endpoints. HomeHarmony is expected to grow across multiple business areas, and controllers provide a familiar, consistent structure with built-in request validation.

## Feature-based organization

Keep controllers, contracts, and services together under `Features/Account`. This makes account functionality easier to find and allows other features to follow the same structure.

## Mock authentication behind an interface

The controller depends on `IAccountService`, with `MockAccountService` used temporarily. A future ASP.NET Core Identity implementation can replace the mock through dependency injection without changing the controller.

## Generic login failure

Invalid credentials return only `401 Unauthorized`. The response does not reveal whether the email or password was incorrect.

## Login request validation

Email and password are required, and the email must be well formed. Empty or malformed requests are rejected automatically by `[ApiController]` with `400 Bad Request`.

Login does not enforce password length or password-creation policy. Those rules belong to registration and will later be handled by ASP.NET Core Identity.

## Login response deferred

`LoginResponse` is intentionally empty until cookie-based or token-based authentication is chosen. The current mock validates credentials but does not persist the login.

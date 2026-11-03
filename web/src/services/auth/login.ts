import type { LoginRequest } from "@/contracts/auth/LoginRequest";
import { ApiError, postJson } from "@/services/apiClient";

export type LoginFailure = "invalid-credentials" | "invalid-request" | "requires-two-factor";

export class LoginApiError extends Error {
  constructor(public readonly failure: LoginFailure) {
    super(failure);
    this.name = "LoginApiError";
  }
}

export async function login(credentials: LoginRequest): Promise<void> {
  try {
    const body = await postJson(
      "/api/account/login", {
        email: credentials.email,
        password: credentials.password,
        mode: "SessionCookie"
      },
      true
    );

    if (
      typeof body !== "object" ||
      body === null ||
      !("status" in body) ||
      body.status !== "Succeeded"
    ) {
      throw new Error("Unexpected login response.");
    }

  } catch (error) {
    if (!(error instanceof ApiError)) {
      throw error;
    }

    const body = error.body;
    const status = typeof body === "object" && body !== null && "status" in body ? body.status : undefined;

    if (error.status === 401) {
      if (status === "RequiresTwoFactor") {
        throw new LoginApiError("requires-two-factor");
      }

      throw new LoginApiError("invalid-credentials");
    }

    if (error.status === 400) {
      throw new LoginApiError("invalid-request");
    }

    throw error;
  }
}

import type { LoginRequest } from "@/contracts/auth/LoginRequest";
import { ApiError, postJson } from "@/services/apiClient";

export type LoginFailure = "invalid-credentials" | "invalid-request";

export class LoginApiError extends Error {
  constructor(public readonly failure: LoginFailure) {
    super(failure);
    this.name = "LoginApiError";
  }
}

export async function login(credentials: LoginRequest): Promise<void> {
  try {
    await postJson("/api/account/login", credentials);
  } catch (error) {
    if (!(error instanceof ApiError)) {
      throw error;
    }

    if (error.status === 401) {
      throw new LoginApiError("invalid-credentials");
    }

    if (error.status === 400) {
      throw new LoginApiError("invalid-request");
    }

    throw error;
  }
}

import type { LoginRequest } from "@/contracts/auth/LoginRequest";

export type LoginFailure =
  | "invalid-credentials"
  | "invalid-request"
  | "timeout"
  | "network"
  | "server";

export class LoginApiError extends Error {
  constructor(public readonly failure: LoginFailure) {
    super(failure);
    this.name = "LoginApiError";
  }
}

const LOGIN_TIMEOUT_MS = 15_000;

export async function login(credentials: LoginRequest): Promise<void> {
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

  if (!apiBaseUrl) {
    throw new LoginApiError("server");
  }

  const abortController = new AbortController();
  const timeoutId = setTimeout(
    () => abortController.abort(),
    LOGIN_TIMEOUT_MS,
  );

  try {
    const response = await fetch(
      `${apiBaseUrl.replace(/\/$/, "")}/api/account/login`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(credentials),
        signal: abortController.signal,
      },
    );

    if (response.status === 401) {
      throw new LoginApiError("invalid-credentials");
    }

    if (response.status === 400) {
      throw new LoginApiError("invalid-request");
    }

    if (!response.ok) {
      throw new LoginApiError("server");
    }
  } catch (error) {
    if (error instanceof LoginApiError) {
      throw error;
    }

    if (error instanceof DOMException && error.name === "AbortError") {
      throw new LoginApiError("timeout");
    }

    throw new LoginApiError("network");
  } finally {
    clearTimeout(timeoutId);
  }
}

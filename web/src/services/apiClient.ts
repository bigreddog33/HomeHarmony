export type ApiFailure = "http" | "configuration" | "timeout" | "network";

export class ApiError extends Error {
  constructor(
    public readonly failure: ApiFailure,
    public readonly status?: number,
    public readonly body?: unknown,
  ) {
    super(failure);
    this.name = "ApiError";
  }
}

const REQUEST_TIMEOUT_MS = 15_000;

export async function postJson(path: string, body: unknown, readResponseBody = false,): Promise<unknown> {
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

  if (!apiBaseUrl) {
    throw new ApiError("configuration");
  }

  const serializedBody = JSON.stringify(body);
  const abortController = new AbortController();
  const timeoutId = setTimeout(
    () => abortController.abort(),
    REQUEST_TIMEOUT_MS,
  );

  try {
    const response = await fetch(
      `${apiBaseUrl.replace(/\/$/, "")}/${path.replace(/^\//, "")}`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: serializedBody,
        signal: abortController.signal,
      },
    );

    if (!response.ok) {
      const body: unknown = await response.json().catch(() => undefined);
      throw new ApiError("http", response.status, body);
    }

    if (readResponseBody) {
      return await response.json();
    }
  }
  catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }

    if (error instanceof DOMException && error.name === "AbortError") {
      throw new ApiError("timeout");
    }

    throw new ApiError("network");
  }
  finally {
    clearTimeout(timeoutId);
  }
}

import { afterEach, beforeEach, vi } from "vitest";

export function stubApi() {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    vi.useFakeTimers();
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5000/");
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllEnvs();
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
    fetchMock.mockReset();
  });

  return fetchMock;
}

// Behave like a server that never replies, but still honor fetch cancellation.
export const pendingUntilAborted: typeof fetch = (_url, options) =>
  new Promise((_resolve, reject) => {
    options?.signal?.addEventListener("abort", () => {
      reject(new DOMException("The request was aborted", "AbortError"));
    }, { once: true });
  });

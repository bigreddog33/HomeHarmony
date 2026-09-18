import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { login, LoginApiError } from "@/services/auth/login";

const credentials = { email: "name@example.com", password: " password " };
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

describe("login API", () => {
  it("succeeds on an empty 200 response without parsing a response body", async () => {
    const response = new Response(null, { status: 200 });
    const parseJson = vi.spyOn(response, "json");
    fetchMock.mockResolvedValue(response);

    await expect(login(credentials)).resolves.toBeUndefined();

    expect(parseJson).not.toHaveBeenCalled();
    expect(fetchMock).toHaveBeenCalledExactlyOnceWith(
      "http://localhost:5000/api/account/login",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(credentials),
        signal: expect.any(AbortSignal),
      },
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it.each([
    [401, "invalid-credentials"],
    [400, "invalid-request"],
    [403, "server"],
    [500, "server"],
    [503, "server"],
  ] as const)("classifies HTTP %i as %s", async (status, failure) => {
    fetchMock.mockResolvedValue(new Response("technical details", { status }));

    await expect(login(credentials)).rejects.toEqual(new LoginApiError(failure));
    expect(vi.getTimerCount()).toBe(0);
  });

  it("classifies a failed connection without exposing its technical message", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch internal-host"));

    await expect(login(credentials)).rejects.toEqual(new LoginApiError("network"));
    expect(vi.getTimerCount()).toBe(0);
  });

  it("aborts a request that takes too long and reports a timeout", async () => {
    fetchMock.mockImplementation(
      (_url, options) =>
        new Promise((_resolve, reject) => {
          options?.signal?.addEventListener("abort", () => {
            reject(new DOMException("The request was aborted", "AbortError"));
          });
        }),
    );

    const request = login(credentials);
    const result = expect(request).rejects.toEqual(new LoginApiError("timeout"));

    await vi.advanceTimersByTimeAsync(15_000);

    await result;
    expect(fetchMock.mock.calls[0][1]?.signal?.aborted).toBe(true);
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports missing API configuration without sending a request", async () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "");

    await expect(login(credentials)).rejects.toEqual(new LoginApiError("server"));
    expect(fetchMock).not.toHaveBeenCalled();
    expect(vi.getTimerCount()).toBe(0);
  });
});

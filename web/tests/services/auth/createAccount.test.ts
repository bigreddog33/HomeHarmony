import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/services/apiClient";
import {
  createAccount,
  CreateAccountApiError,
} from "@/services/auth/createAccount";

const request = { email: "name@example.com", password: " Password1! " };
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
  fetchMock.mockReset();
});

describe("create account API", () => {
  it("posts to the creation endpoint and accepts an empty success response", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 200 }));

    await expect(createAccount(request)).resolves.toBeUndefined();

    expect(fetchMock).toHaveBeenCalledExactlyOnceWith(
      "http://localhost:5000/api/account/createuser",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
        signal: expect.any(AbortSignal),
      },
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it.each([
    [401, "creation-failed"],
    [400, "invalid-request"],
  ] as const)("classifies HTTP %i as %s", async (status, failure) => {
    fetchMock.mockResolvedValue(new Response(null, { status }));

    await expect(createAccount(request)).rejects.toEqual(
      new CreateAccountApiError(failure),
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it("preserves an HTTP server failure as a shared error", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 500 }));

    const result = createAccount(request);

    await expect(result).rejects.toBeInstanceOf(ApiError);
    await expect(result).rejects.toMatchObject({ failure: "http", status: 500 });
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports connection failures as shared API errors", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch"));

    await expect(createAccount(request)).rejects.toEqual(
      new ApiError("network"),
    );
    expect(vi.getTimerCount()).toBe(0);
  });
});

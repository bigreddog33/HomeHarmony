import { describe, expect, it, vi } from "vitest";
import { ApiError } from "@/services/apiClient";
import { login, LoginApiError } from "@/services/auth/login";
import { loginValues } from "../../fixtures/auth";
import { pendingUntilAborted, stubApi } from "../../support/apiStub";

const credentials = loginValues({ password: " password " });
const fetchMock = stubApi();

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
  ] as const)("classifies HTTP %i as %s", async (status, failure) => {
    fetchMock.mockResolvedValue(new Response("technical details", { status }));

    await expect(login(credentials)).rejects.toEqual(new LoginApiError(failure));
    expect(vi.getTimerCount()).toBe(0);
  });

  it.each([403, 500, 503])("preserves HTTP %i as a shared error", async (status) => {
    fetchMock.mockResolvedValue(new Response("technical details", { status }));

    const result = login(credentials);

    await expect(result).rejects.toBeInstanceOf(ApiError);
    await expect(result).rejects.toMatchObject({ failure: "http", status });
    expect(vi.getTimerCount()).toBe(0);
  });

  it("classifies a failed connection without exposing its technical message", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch internal-host"));

    await expect(login(credentials)).rejects.toEqual(new ApiError("network"));
    expect(vi.getTimerCount()).toBe(0);
  });

  it("aborts a request that takes too long and reports a timeout", async () => {
    fetchMock.mockImplementation(pendingUntilAborted);

    const request = login(credentials);
    const result = expect(request).rejects.toEqual(new ApiError("timeout"));

    await vi.advanceTimersByTimeAsync(15_000);

    await result;
    expect(fetchMock.mock.calls[0][1]?.signal?.aborted).toBe(true);
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports missing API configuration without sending a request", async () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "");

    await expect(login(credentials)).rejects.toEqual(new ApiError("configuration"));
    expect(fetchMock).not.toHaveBeenCalled();
    expect(vi.getTimerCount()).toBe(0);
  });
});

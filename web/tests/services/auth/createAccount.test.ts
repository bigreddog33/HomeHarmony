import { describe, expect, it, vi } from "vitest";
import { ApiError } from "@/services/apiClient";
import {
  createAccount,
  CreateAccountApiError,
} from "@/services/auth/createAccount";
import { loginValues } from "../../fixtures/auth";
import { pendingUntilAborted, stubApi } from "../../support/apiStub";

const request = loginValues({ password: " Password1! " });
const fetchMock = stubApi();

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

  it.each([500, 503])("preserves HTTP %i as a shared error", async (status) => {
    fetchMock.mockResolvedValue(new Response(null, { status }));

    const result = createAccount(request);

    await expect(result).rejects.toBeInstanceOf(ApiError);
    await expect(result).rejects.toMatchObject({ failure: "http", status });
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports connection failures as shared API errors", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch"));

    await expect(createAccount(request)).rejects.toEqual(
      new ApiError("network"),
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it("aborts a request that takes too long and reports a timeout", async () => {
    fetchMock.mockImplementation(pendingUntilAborted);
    const result = expect(createAccount(request)).rejects.toEqual(new ApiError("timeout"));

    await vi.advanceTimersByTimeAsync(15_000);

    await result;
    expect(fetchMock.mock.calls[0][1]?.signal?.aborted).toBe(true);
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports missing API configuration without sending a request", async () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "");

    await expect(createAccount(request)).rejects.toEqual(new ApiError("configuration"));
    expect(fetchMock).not.toHaveBeenCalled();
    expect(vi.getTimerCount()).toBe(0);
  });
});

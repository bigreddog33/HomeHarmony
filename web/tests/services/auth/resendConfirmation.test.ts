import { describe, expect, it, vi } from "vitest";
import { ApiError } from "@/services/apiClient";
import { resendConfirmation } from "@/services/auth/resendConfirmation";
import { pendingUntilAborted, stubApi } from "../../support/apiStub";

const fetchMock = stubApi();
const email = "user@example.com";

describe("resend confirmation API", () => {
  it("posts only the email and accepts an empty 200 response", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 200 }));

    await expect(resendConfirmation(email)).resolves.toBeUndefined();

    expect(fetchMock).toHaveBeenCalledExactlyOnceWith(
      "http://localhost:5000/api/account/resendconfirmation",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email }),
        signal: expect.any(AbortSignal),
      },
    );
    expect(vi.getTimerCount()).toBe(0);
  });

  it.each([400, 404, 500])("preserves HTTP %i", async (status) => {
    fetchMock.mockResolvedValue(new Response(null, { status }));
    await expect(resendConfirmation(email)).rejects.toMatchObject({ failure: "http", status });
    expect(vi.getTimerCount()).toBe(0);
  });

  it("reports a network failure", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch"));
    await expect(resendConfirmation(email)).rejects.toEqual(new ApiError("network"));
    expect(vi.getTimerCount()).toBe(0);
  });

  it("aborts a slow request after 15 seconds", async () => {
    fetchMock.mockImplementation(pendingUntilAborted);
    const result = expect(resendConfirmation(email)).rejects.toEqual(new ApiError("timeout"));
    await vi.advanceTimersByTimeAsync(15_000);
    await result;
    expect(fetchMock.mock.calls[0][1]?.signal?.aborted).toBe(true);
    expect(vi.getTimerCount()).toBe(0);
  });

  it("does not send a request without API configuration", async () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "");
    await expect(resendConfirmation(email)).rejects.toEqual(new ApiError("configuration"));
    expect(fetchMock).not.toHaveBeenCalled();
  });
});

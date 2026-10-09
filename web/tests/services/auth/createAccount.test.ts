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
  it.each(["Created", "CreatedConfirmationFailed"])("posts to the creation endpoint and returns %s", async (status) => {
    fetchMock.mockResolvedValue(Response.json({ status }, { status: 201 }));

    await expect(createAccount(request)).resolves.toEqual({ status });

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

  it.each([true, false])("preserves emailConfirmed=%s on a duplicate response", async (emailConfirmed) => {
    fetchMock.mockResolvedValue(Response.json({ status: "AlreadyExists", emailConfirmed }, { status: 409 }));

    await expect(createAccount(request)).rejects.toMatchObject({
      failure: "already-exists", emailConfirmed,
    });
  });

  it.each([null, {}, { emailConfirmed: null }, { emailConfirmed: "false" }, { emailConfirmed: 0 }])(
    "treats an unusable confirmation flag as unknown: %j", async (body) => {
      fetchMock.mockResolvedValue(Response.json(body, { status: 409 }));

      await expect(createAccount(request)).rejects.toMatchObject({
        failure: "already-exists", emailConfirmed: undefined,
      });
    },
  );

  it.each([null, "not JSON"])("handles a duplicate with an empty or non-JSON body: %j", async (body) => {
    fetchMock.mockResolvedValue(new Response(body, { status: 409 }));

    await expect(createAccount(request)).rejects.toMatchObject({
      failure: "already-exists", emailConfirmed: undefined,
    });
  });

  it.each([null, {}, "Created", { status: "Unknown" }, { status: "AlreadyExists" }, { status: 0 }])(
    "rejects an invalid success response: %j", async (body) => {
      fetchMock.mockResolvedValue(Response.json(body, { status: 201 }));

      await expect(createAccount(request)).rejects.toThrow("Unexpected account creation response.");
      expect(vi.getTimerCount()).toBe(0);
    },
  );

  it.each([null, "{"])("rejects an empty or malformed success body: %j", async (body) => {
    fetchMock.mockResolvedValue(new Response(body, { status: 201 }));

    await expect(createAccount(request)).rejects.toBeInstanceOf(Error);
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

import { afterEach, expect, it, vi } from "vitest";
import { ApiError, postJson } from "@/services/apiClient";

afterEach(() => {
  vi.unstubAllEnvs();
  vi.unstubAllGlobals();
});

it.each([400, 401, 409, 503])(
  "preserves HTTP %i for the endpoint to interpret",
  async (status) => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5000");
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(new Response(null, { status })),
    );

    const result = postJson("/api/example", {});

    await expect(result).rejects.toBeInstanceOf(ApiError);
    await expect(result).rejects.toMatchObject({ failure: "http", status });
  },
);

it("preserves a JSON error body for the account service", async () => {
  const body = { status: "AlreadyExists", emailConfirmed: false };
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5000");
  vi.stubGlobal("fetch", vi.fn().mockResolvedValue(Response.json(body, { status: 409 })));

  await expect(postJson("/api/account/createuser", {})).rejects.toMatchObject({
    failure: "http", status: 409, body,
  });
});

it("preserves the HTTP error when its body is not JSON", async () => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5000");
  vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("Server unavailable", { status: 503 })));

  await expect(postJson("/api/account/createuser", {})).rejects.toMatchObject({
    failure: "http", status: 503, body: undefined,
  });
});

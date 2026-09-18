import { afterEach, describe, expect, it, vi } from "vitest";
import * as apiClient from "@/services/apiClient";
import { login } from "@/services/auth/login";
import { createAccount } from "@/services/auth/createAccount";
import { loginValues } from "../../fixtures/auth";

afterEach(() => {
  vi.restoreAllMocks();
});

describe.each([
  ["login", login],
  ["createAccount", createAccount],
] as const)("%s error propagation", (_name, submit) => {
  it.each([
    new apiClient.ApiError("timeout"),
    new apiClient.ApiError("network"),
    new apiClient.ApiError("configuration"),
    new apiClient.ApiError("http", 503),
    new Error("Unexpected failure"),
  ])("passes through the original $message error", async (error) => {
    vi.spyOn(apiClient, "postJson").mockRejectedValue(error);

    await expect(
      submit(loginValues()),
    ).rejects.toBe(error);
  });
});

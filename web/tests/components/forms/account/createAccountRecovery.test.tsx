// @vitest-environment jsdom
import "../../../support/dom";
import { navigation } from "../../../support/navigationStub";
import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import CreateAccountForm from "@/components/forms/account/CreateAccountForm";
import { createAccount, CreateAccountApiError } from "@/services/auth/createAccount";
import { resendConfirmation } from "@/services/auth/resendConfirmation";
import { accountFields, accountValues } from "../../../fixtures/auth";
import { resendErrors } from "../../../fixtures/apiErrors";
import { fillForm, replaceField } from "../../../support/form";
import { deferred } from "../../../support/deferred";

vi.mock("@/services/auth/createAccount", async (importOriginal) => ({
  ...await importOriginal<typeof import("@/services/auth/createAccount")>(),
  createAccount: vi.fn(),
}));
vi.mock("@/services/auth/resendConfirmation", () => ({ resendConfirmation: vi.fn() }));

const submit = vi.mocked(createAccount);
const resend = vi.mocked(resendConfirmation);
const storageKey = "accountConfirmation";

beforeEach(() => {
  sessionStorage.clear();
  submit.mockReset();
  submit.mockResolvedValue({ status: "Created" });
  resend.mockReset();
  resend.mockResolvedValue(undefined);
});

afterEach(() => {
  vi.restoreAllMocks();
  sessionStorage.clear();
});

async function fillAndSubmit() {
  const user = userEvent.setup();
  render(<CreateAccountForm />);
  await fillForm(user, accountFields, accountValues({
    email: " name@example.com ", confirmEmail: " name@example.com ",
  }));
  await user.click(screen.getByRole("button", { name: "Create account" }));
  return user;
}

describe("registration page handoff", () => {
  it.each(["Created", "CreatedConfirmationFailed"] as const)("stores only email and %s before navigating, replacing old context", async (status) => {
    sessionStorage.setItem(storageKey, JSON.stringify({ email: "old@example.com", status: "Created" }));
    submit.mockResolvedValue({ status });
    navigation.replace.mockImplementation(() => {
      expect(JSON.parse(sessionStorage.getItem(storageKey)!)).toEqual({ email: "name@example.com", status });
    });

    await fillAndSubmit();

    expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount");
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(resend).not.toHaveBeenCalled();
  });

  it.each(["removeItem", "setItem"] as const)("still navigates after creation when storage.%s throws", async (method) => {
    vi.spyOn(Storage.prototype, method).mockImplementation(() => {
      throw new DOMException("Storage unavailable", "SecurityError");
    });

    await fillAndSubmit();

    expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount");
    expect(submit).toHaveBeenCalledTimes(1);
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("does not navigate or store a context when the API response is unusable", async () => {
    submit.mockRejectedValue(new Error("Unexpected account creation response."));
    await fillAndSubmit();

    expect(screen.getByRole("alert")).toHaveTextContent("An unexpected error occurred. Please try again.");
    expect(navigation.replace).not.toHaveBeenCalled();
    expect(sessionStorage.getItem(storageKey)).toBeNull();
    expect(screen.getByRole("button", { name: "Create account" })).toBeEnabled();
  });
});

describe("existing-account recovery", () => {
  it.each([true, false, undefined])("selects the recovery action for emailConfirmed=%s", async (emailConfirmed) => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", emailConfirmed));
    const user = await fillAndSubmit();

    expect(screen.getByRole("alert")).toHaveTextContent("There is already an account existing with this email");
    const resendButton = screen.queryByRole("button", { name: "Resend confirmation link" });
    const resetButton = screen.queryByRole("button", { name: "Reset password" });
    if (emailConfirmed === false) {
      expect(resendButton).toBeEnabled();
      expect(resetButton).not.toBeInTheDocument();
    } else if (emailConfirmed === true) {
      expect(resendButton).not.toBeInTheDocument();
      expect(resetButton).toBeEnabled();
      await user.click(resetButton!);
    } else {
      expect(resendButton).not.toBeInTheDocument();
      expect(resetButton).not.toBeInTheDocument();
    }
    expect(submit).toHaveBeenCalledTimes(1);
    expect(resend).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();
  });

  it("resends for the trimmed email without submitting registration and blocks edits while pending", async () => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", false));
    const pending = deferred<void>();
    resend.mockReturnValueOnce(pending.promise);
    const user = await fillAndSubmit();

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    const pendingButton = screen.getByRole("button", { name: "Requesting confirmation..." });
    expect(pendingButton).toBeDisabled();
    expect(screen.getByRole("button", { name: "Create account" })).toBeDisabled();
    for (const { label } of accountFields) {
      expect(screen.getByLabelText(label, { exact: true })).toBeDisabled();
    }
    await user.click(pendingButton);
    await user.click(screen.getByRole("button", { name: "Create account" }));
    expect(resend).toHaveBeenCalledExactlyOnceWith("name@example.com");
    expect(submit).toHaveBeenCalledTimes(1);
    expect(navigation.replace).not.toHaveBeenCalled();

    await act(async () => pending.resolve());

    expect(screen.getByRole("status")).toHaveTextContent("Your confirmation request was processed");
    expect(screen.getByRole("button", { name: "Resend confirmation link" })).toBeEnabled();
    for (const { label } of accountFields) {
      expect(screen.getByLabelText(label, { exact: true })).toBeEnabled();
    }
    expect(navigation.replace).not.toHaveBeenCalled();
  });

  it.each(resendErrors)("shows $name and allows a successful retry without navigating", async ({ error, message }) => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", false));
    resend.mockRejectedValueOnce(error);
    const user = await fillAndSubmit();

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    // The form also has an existing-account alert; inspect the resend feedback by text.
    expect(await screen.findByText(message)).toBeVisible();
    expect(screen.getByRole("button", { name: "Resend confirmation link" })).toBeEnabled();
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));

    expect(screen.queryByText(message)).not.toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveTextContent("Your confirmation request was processed");
    expect(resend).toHaveBeenCalledTimes(2);
    expect(submit).toHaveBeenCalledTimes(1);
    expect(navigation.replace).not.toHaveBeenCalled();
  });

  it.each(accountFields)("clears the old recovery action and success feedback when $field changes", async ({ label }) => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", false));
    const user = await fillAndSubmit();
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    expect(screen.getByRole("status")).toBeVisible();

    await user.type(screen.getByLabelText(label, { exact: true }), "x");

    expect(screen.queryByRole("status")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(screen.queryByText("There is already an account existing with this email")).not.toBeInTheDocument();
  });

  it("uses the edited email for a later recovery request", async () => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", false));
    const user = await fillAndSubmit();
    await replaceField(user, "Email", "new@example.com");
    await replaceField(user, "Confirm email", "new@example.com");
    await user.click(screen.getByRole("button", { name: "Create account" }));
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));

    expect(resend).toHaveBeenCalledExactlyOnceWith("new@example.com");
  });

  it("clears recovery feedback when registration is submitted again", async () => {
    submit.mockRejectedValueOnce(new CreateAccountApiError("already-exists", false));
    const pending = deferred<{ status: "Created" }>();
    submit.mockReturnValueOnce(pending.promise);
    const user = await fillAndSubmit();
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));

    await user.click(screen.getByRole("button", { name: "Create account" }));
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    await act(async () => pending.resolve({ status: "Created" }));
    await waitFor(() => expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount"));
  });

  it("clears previous resend success before another request and its failure", async () => {
    submit.mockRejectedValue(new CreateAccountApiError("already-exists", false));
    const retry = deferred<void>();
    resend.mockResolvedValueOnce(undefined).mockReturnValueOnce(retry.promise);
    const user = await fillAndSubmit();
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    expect(screen.getByRole("status")).toBeVisible();
    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
    await act(async () => retry.reject(resendErrors[0].error));
    expect(screen.getByText(resendErrors[0].message)).toBeVisible();
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
  });
});

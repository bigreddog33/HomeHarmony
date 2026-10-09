// @vitest-environment jsdom
import "../support/dom";
import { act, render, screen } from "@testing-library/react";
import { renderToString } from "react-dom/server";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import SuccessCreateAccountPage from "@/app/(account)/successCreateAccount/page";
import { resendConfirmation } from "@/services/auth/resendConfirmation";
import { resendErrors } from "../fixtures/apiErrors";
import { deferred } from "../support/deferred";

vi.mock("@/services/auth/resendConfirmation", () => ({ resendConfirmation: vi.fn() }));

const resend = vi.mocked(resendConfirmation);
const storageKey = "accountConfirmation";
const email = "user@example.com";
const failedContext = { email, status: "CreatedConfirmationFailed" };
const instructions = /Follow the confirmation link in your email/;
const failureMessage = /Your account was created, but we couldn’t send/;

beforeEach(() => {
  sessionStorage.clear();
  resend.mockReset();
  resend.mockResolvedValue(undefined);
});

afterEach(() => {
  vi.restoreAllMocks();
  sessionStorage.clear();
});

describe("account confirmation page", () => {
  it("renders loading instructions before reading browser storage", () => {
    sessionStorage.setItem(storageKey, JSON.stringify(failedContext));
    const html = renderToString(<SuccessCreateAccountPage />);
    expect(html).toContain("Loading your account details...");
    expect(html).not.toContain("Account created");
    expect(html).not.toContain("Follow the confirmation link");
    expect(html).not.toContain("Resend confirmation link");
    expect(resend).not.toHaveBeenCalled();
  });

  it.each(["Created", "CreatedConfirmationFailed"])("restores %s after a page reload", (status) => {
    sessionStorage.setItem(storageKey, JSON.stringify({ email, status }));
    const firstVisit = render(<SuccessCreateAccountPage />);
    firstVisit.unmount();
    render(<SuccessCreateAccountPage />);

    expect(screen.getByText("Account created")).toBeVisible();
    expect(screen.queryByText(/Loading your account details/)).not.toBeInTheDocument();
    if (status === "Created") {
      expect(screen.getByText(instructions)).toBeVisible();
      expect(screen.queryByText(failureMessage)).not.toBeInTheDocument();
      expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    } else {
      expect(screen.getByText(failureMessage)).toBeVisible();
      expect(screen.getByRole("button", { name: "Resend confirmation link" })).toBeEnabled();
      expect(screen.queryByText(instructions)).not.toBeInTheDocument();
    }
    expect(resend).not.toHaveBeenCalled();
  });

  it.each([
    null, "{", "null", "{}",
    JSON.stringify({ email: "", status: "CreatedConfirmationFailed" }),
    JSON.stringify({ email: "   ", status: "Created" }),
    JSON.stringify({ email: 123, status: "CreatedConfirmationFailed" }),
    JSON.stringify({ status: "CreatedConfirmationFailed" }),
    JSON.stringify({ email }),
    JSON.stringify({ email, status: "Unknown" }),
  ])("uses general instructions for missing or invalid stored context: %j", (saved) => {
    if (saved !== null) sessionStorage.setItem(storageKey, saved);
    render(<SuccessCreateAccountPage />);

    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText("Account created")).not.toBeInTheDocument();
    expect(screen.queryByText(/Loading your account details/)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(resend).not.toHaveBeenCalled();
  });

  it("uses general instructions when reading storage throws", () => {
    vi.spyOn(Storage.prototype, "getItem").mockImplementation(() => {
      throw new DOMException("Storage blocked", "SecurityError");
    });
    render(<SuccessCreateAccountPage />);

    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText("Account created")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(resend).not.toHaveBeenCalled();
  });

  it("prevents duplicate resends, replaces failure instructions on success, and remembers success on reload", async () => {
    const user = userEvent.setup();
    const pending = deferred<void>();
    resend.mockReturnValueOnce(pending.promise);
    sessionStorage.setItem(storageKey, JSON.stringify(failedContext));
    const page = render(<SuccessCreateAccountPage />);

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    const button = screen.getByRole("button", { name: "Requesting confirmation..." });
    expect(button).toBeDisabled();
    await user.click(button);
    expect(resend).toHaveBeenCalledExactlyOnceWith(email);
    expect(screen.getByText(failureMessage)).toBeVisible();

    await act(async () => pending.resolve());

    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText(failureMessage)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /confirmation/i })).not.toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveTextContent("Your confirmation request was processed.");
    expect(JSON.parse(sessionStorage.getItem(storageKey)!)).toEqual({ email, status: "Created" });
    page.unmount();
    render(<SuccessCreateAccountPage />);
    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText(failureMessage)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(resend).toHaveBeenCalledTimes(1);
  });

  it.each(resendErrors)("shows $name, clears the error on retry, and switches to inbox instructions on success", async ({ error, message }) => {
    const user = userEvent.setup();
    const retry = deferred<void>();
    resend.mockRejectedValueOnce(error).mockReturnValueOnce(retry.promise);
    sessionStorage.setItem(storageKey, JSON.stringify(failedContext));
    render(<SuccessCreateAccountPage />);

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(message);
    expect(screen.getByText(failureMessage)).toBeVisible();
    expect(JSON.parse(sessionStorage.getItem(storageKey)!)).toEqual(failedContext);
    expect(screen.getByRole("button", { name: "Resend confirmation link" })).toBeEnabled();

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Requesting confirmation..." })).toBeDisabled();
    await act(async () => retry.resolve());
    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText(failureMessage)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(resend).toHaveBeenCalledTimes(2);
  });

  it("still switches to inbox instructions when saving resend success fails", async () => {
    const user = userEvent.setup();
    sessionStorage.setItem(storageKey, JSON.stringify(failedContext));
    render(<SuccessCreateAccountPage />);
    vi.spyOn(Storage.prototype, "setItem").mockImplementation(() => {
      throw new DOMException("Storage full", "QuotaExceededError");
    });

    await user.click(screen.getByRole("button", { name: "Resend confirmation link" }));

    expect(screen.getByText(instructions)).toBeVisible();
    expect(screen.queryByText(failureMessage)).not.toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Resend confirmation link" })).not.toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveTextContent("Your confirmation request was processed.");
  });
});

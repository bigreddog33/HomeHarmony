// @vitest-environment jsdom
import "../../../support/dom";
import { navigation } from "../../../support/navigationStub";
import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ComponentType } from "react";
import { beforeEach, describe, expect, it, vi, type MockedFunction } from "vitest";
import LoginForm from "@/components/forms/account/LoginForm";
import CreateAccountForm from "@/components/forms/account/CreateAccountForm";
import { login, LoginApiError } from "@/services/auth/login";
import { createAccount, CreateAccountApiError } from "@/services/auth/createAccount";
import { accountFields, accountValues, loginFields, loginValues } from "../../../fixtures/auth";
import { sharedApiErrors } from "../../../fixtures/apiErrors";
import { fillForm, replaceField } from "../../../support/form";

// Keep validation and rendering real; control only the external service and router.
vi.mock("@/services/auth/login", async (importOriginal) => ({
  ...await importOriginal<typeof import("@/services/auth/login")>(),
  login: vi.fn(),
}));
vi.mock("@/services/auth/createAccount", async (importOriginal) => ({
  ...await importOriginal<typeof import("@/services/auth/createAccount")>(),
  createAccount: vi.fn(),
}));

type FormCase = {
  name: string;
  Component: ComponentType;
  submit: MockedFunction<typeof login>;
  fields: readonly { field: string; label: string; required: string }[];
  values: Record<string, string>;
  button: string;
  pending: string;
  navigate: typeof navigation.push;
  destination: string;
  accountErrors: { error: LoginApiError | CreateAccountApiError; message: string }[];
};

const forms: FormCase[] = [
  {
    name: "login", Component: LoginForm, submit: vi.mocked(login),
    fields: loginFields, values: loginValues(), button: "Login",
    pending: "Logging in...", navigate: navigation.push, destination: "/home",
    accountErrors: [
      { error: new LoginApiError("invalid-credentials"), message: "The email or password is incorrect." },
      { error: new LoginApiError("invalid-request"), message: "Check your email and password, then try again." },
    ],
  },
  {
    name: "create account", Component: CreateAccountForm, submit: vi.mocked(createAccount),
    fields: accountFields, values: accountValues(), button: "Create account",
    pending: "Creating account...", navigate: navigation.replace, destination: "/successCreateAccount",
    accountErrors: [
      { error: new CreateAccountApiError("creation-failed"), message: "We couldn't create your account. Please try again." },
      { error: new CreateAccountApiError("invalid-request"), message: "Check your email and password, then try again." },
    ],
  },
];

describe.each(forms)("$name form", ({ Component, submit, fields, values, button, pending, navigate, destination, accountErrors }) => {
  beforeEach(() => {
    submit.mockReset();
    submit.mockResolvedValue(undefined);
  });

  it.each(fields)("shows an accessible error for missing $field and prevents submission", async ({ field, label, required }) => {
    const user = userEvent.setup();
    render(<Component />);
    await fillForm(user, fields, { ...values, [field]: "" });

    await user.click(screen.getByRole("button", { name: button }));

    const input = screen.getByLabelText(label, { exact: true });
    expect(input).toHaveFocus();
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input).toHaveAccessibleDescription(new RegExp(required.replaceAll(".", "\\.")));
    expect(submit).not.toHaveBeenCalled();
    expect(navigation.push).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();
  });

  it("rechecks email after editing and allows submission once it is corrected", async () => {
    const user = userEvent.setup();
    render(<Component />);
    await fillForm(user, fields, values);
    await replaceField(user, "Email", "not-an-email");
    await user.tab();

    expect(screen.getByText("Enter a valid email address.")).toBeVisible();
    await user.click(screen.getByRole("button", { name: button }));
    expect(submit).not.toHaveBeenCalled();

    await replaceField(user, "Email", values.email);
    await user.tab();
    expect(screen.queryByText("Enter a valid email address.")).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: button }));
    await waitFor(() => expect(navigate).toHaveBeenCalledExactlyOnceWith(destination));
  });

  it("sends trimmed credentials and redirects only after success", async () => {
    const user = userEvent.setup();
    let finish!: () => void;
    submit.mockReturnValue(new Promise<void>((resolve) => { finish = resolve; }));
    render(<Component />);
    const paddedValues = Object.fromEntries(
      Object.entries(values).map(([field, value]) => [field, ` ${value} `]),
    );
    await fillForm(user, fields, paddedValues);

    await user.click(screen.getByRole("button", { name: button }));

    expect(submit).toHaveBeenCalledExactlyOnceWith(loginValues());
    const pendingButton = screen.getByRole("button", { name: pending });
    expect(pendingButton).toBeDisabled();
    for (const { label } of fields) {
      expect(screen.getByLabelText(label, { exact: true })).toBeDisabled();
    }
    expect(navigation.push).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();
    await user.click(pendingButton);
    expect(submit).toHaveBeenCalledTimes(1);

    await act(async () => finish());

    expect(navigate).toHaveBeenCalledExactlyOnceWith(destination);
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it.each(sharedApiErrors)("shows $name, permits retry, and redirects only on successful retry", async ({ error, message }) => {
    const user = userEvent.setup();
    submit.mockRejectedValueOnce(error);
    render(<Component />);
    await fillForm(user, fields, values);

    await user.click(screen.getByRole("button", { name: button }));

    expect(await screen.findByRole("alert")).toHaveTextContent(message);
    expect(screen.queryByText("Internal details must stay hidden")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: button })).toBeEnabled();
    for (const { label } of fields) {
      expect(screen.getByLabelText(label, { exact: true })).toBeEnabled();
    }
    expect(navigation.push).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();

    await user.click(screen.getByRole("button", { name: button }));

    await waitFor(() => expect(navigate).toHaveBeenCalledExactlyOnceWith(destination));
    expect(submit).toHaveBeenCalledTimes(2);
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it.each(accountErrors)("shows $error.failure and clears the account error when input changes", async ({ error, message }) => {
    const user = userEvent.setup();
    submit.mockRejectedValueOnce(error);
    render(<Component />);
    await fillForm(user, fields, values);
    await user.click(screen.getByRole("button", { name: button }));

    expect(await screen.findByRole("alert")).toHaveTextContent(message);
    expect(navigation.push).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();

    await replaceField(user, "Email", values.email);
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: button }));
    await waitFor(() => expect(navigate).toHaveBeenCalledExactlyOnceWith(destination));
  });

  it("dismisses a notification without submitting or navigating", async () => {
    const user = userEvent.setup();
    submit.mockRejectedValueOnce(sharedApiErrors[0].error);
    render(<Component />);
    await fillForm(user, fields, values);
    await user.click(screen.getByRole("button", { name: button }));
    await screen.findByRole("alert");

    await user.click(screen.getByRole("button", { name: "Dismiss notification" }));

    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(submit).toHaveBeenCalledTimes(1);
    expect(navigate).not.toHaveBeenCalled();
  });
});

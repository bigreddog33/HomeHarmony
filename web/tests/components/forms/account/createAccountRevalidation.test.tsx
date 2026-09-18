// @vitest-environment jsdom
import "../../../support/dom";
import { navigation } from "../../../support/navigationStub";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import CreateAccountForm from "@/components/forms/account/CreateAccountForm";
import { createAccount } from "@/services/auth/createAccount";
import { accountFields, accountValues, confirmationPairs } from "../../../fixtures/auth";
import { fillForm, replaceField } from "../../../support/form";

vi.mock("@/services/auth/createAccount", async (importOriginal) => ({
  ...await importOriginal<typeof import("@/services/auth/createAccount")>(),
  createAccount: vi.fn(),
}));

const submit = vi.mocked(createAccount);

beforeEach(() => {
  submit.mockReset();
  submit.mockResolvedValue(undefined);
});

describe.each(confirmationPairs)("$field confirmation revalidation", ({ field, label, confirmationLabel, different, error }) => {
  it("immediately rechecks a previously confirmed value when the original changes", async () => {
    const user = userEvent.setup();
    const values = accountValues();
    render(<CreateAccountForm />);
    await fillForm(user, accountFields, values);
    await user.tab();

    await replaceField(user, label, different);

    // Check before blur: changing the original must invalidate its confirmation.
    expect(screen.getByText(error)).toBeVisible();
    expect(screen.getByLabelText(confirmationLabel, { exact: true })).toHaveAttribute("aria-invalid", "true");
    await user.click(screen.getByRole("button", { name: "Create account" }));
    expect(submit).not.toHaveBeenCalled();
    expect(navigation.replace).not.toHaveBeenCalled();

    await replaceField(user, label, values[field]);

    expect(screen.queryByText(error)).not.toBeInTheDocument();
    expect(screen.getByLabelText(confirmationLabel, { exact: true })).toHaveAttribute("aria-invalid", "false");
    await user.click(screen.getByRole("button", { name: "Create account" }));
    await waitFor(() => expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount"));
  });

  it("rechecks an edited confirmation and permits submission after correction", async () => {
    const user = userEvent.setup();
    const values = accountValues();
    render(<CreateAccountForm />);
    await fillForm(user, accountFields, values);

    await replaceField(user, confirmationLabel, different);
    await user.tab();
    expect(screen.getByText(error)).toBeVisible();
    await user.click(screen.getByRole("button", { name: "Create account" }));
    expect(submit).not.toHaveBeenCalled();

    await replaceField(user, confirmationLabel, values[field]);
    await user.tab();
    expect(screen.queryByText(error)).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Create account" }));
    await waitFor(() => expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount"));
  });
});

it("rechecks password strength even when both edited passwords match", async () => {
  const user = userEvent.setup();
  render(<CreateAccountForm />);
  await fillForm(user, accountFields, accountValues());
  await replaceField(user, "Password", "weak");
  await replaceField(user, "Confirm password", "weak");
  await user.click(screen.getByRole("button", { name: "Create account" }));

  expect(screen.getByText("Password must contain at least 8 characters.")).toBeVisible();
  expect(screen.queryByText("Passwords do not match.")).not.toBeInTheDocument();
  expect(submit).not.toHaveBeenCalled();

  await replaceField(user, "Password", "Abcdef1!");
  await replaceField(user, "Confirm password", "Abcdef1!");
  await user.click(screen.getByRole("button", { name: "Create account" }));

  await waitFor(() => expect(navigation.replace).toHaveBeenCalledExactlyOnceWith("/successCreateAccount"));
  expect(submit).toHaveBeenCalledExactlyOnceWith({ email: "name@example.com", password: "Abcdef1!" });
  expect(screen.queryByText("Password must contain at least 8 characters.")).not.toBeInTheDocument();
});

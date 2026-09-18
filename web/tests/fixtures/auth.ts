import type { LoginRequest } from "@/contracts/auth/LoginRequest";
import type { CreateAccountFormValue } from "@/components/forms/account/createAccountValidation";

export function loginValues(overrides: Partial<LoginRequest> = {}): LoginRequest {
  return { email: "name@example.com", password: "Password1!", ...overrides };
}

export function accountValues(
  overrides: Partial<CreateAccountFormValue> = {},
): CreateAccountFormValue {
  return {
    ...loginValues(),
    confirmEmail: "name@example.com",
    confirmPassword: "Password1!",
    ...overrides,
  };
}

export const invalidEmails = [
  "not-an-email",
  "name@",
  "@example.com",
  "name@example",
  "name@@example.com",
  "first last@example.com",
];

export const loginFields = [
  { field: "email", label: "Email", required: "Email is required." },
  { field: "password", label: "Password", required: "Password is required." },
] as const;

export const accountFields = [
  loginFields[0],
  { field: "confirmEmail", label: "Confirm email", required: "Please retype your email." },
  loginFields[1],
  { field: "confirmPassword", label: "Confirm password", required: "Please retype your password." },
] as const;

// Each password breaks one requirement while satisfying the others.
export const invalidPasswords = [
  { password: "Abcd1!x", error: "Password must contain at least 8 characters." },
  { password: "password1!", error: "Password must contain at least one uppercase letter." },
  { password: "Password!", error: "Password must contain at least one number." },
  { password: "Password1", error: "Password must contain at least one special character." },
];

export const confirmationPairs = [
  {
    field: "email", label: "Email", confirmation: "confirmEmail",
    confirmationLabel: "Confirm email", different: "other@example.com",
    error: "Email addresses do not match.",
  },
  {
    field: "password", label: "Password", confirmation: "confirmPassword",
    confirmationLabel: "Confirm password", different: "Different1!",
    error: "Passwords do not match.",
  },
] as const;

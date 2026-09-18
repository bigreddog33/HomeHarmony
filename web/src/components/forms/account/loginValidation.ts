import type { LoginRequest } from "@/contracts/auth/LoginRequest";

export type LoginField = keyof LoginRequest;
export type LoginFieldErrors = Partial<Record<LoginField, string>>;

export function validateLoginField(
  field: LoginField,
  value: string,
): string | undefined {
  if (!value.trim()) {
    return field === "email" ? "Email is required." : "Password is required.";
  }

  if (field === "email" && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())) {
    return "Enter a valid email address.";
  }
}

export function validateLogin(values: LoginRequest): LoginFieldErrors {
  return {
    email: validateLoginField("email", values.email),
    password: validateLoginField("password", values.password),
  };
}

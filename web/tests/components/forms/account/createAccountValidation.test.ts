import { describe, expect, it } from "vitest";
import { validateCreateAccount } from "@/components/forms/account/createAccountValidation";
import {
  accountFields, accountValues, confirmationPairs, invalidEmails, invalidPasswords,
} from "../../../fixtures/auth";

const noErrors = {
  email: undefined,
  confirmEmail: undefined,
  password: undefined,
  confirmPassword: undefined,
};

describe("create account validation", () => {
  describe.each(accountFields)("required $field", ({ field, required }) => {
    it.each(["", "   "])("rejects %j", (value) => {
      const errors = validateCreateAccount(accountValues({ [field]: value }));
      expect(errors[field]).toBe(required);
    });
  });

  it.each(invalidEmails)("rejects invalid email %j", (email) => {
    expect(validateCreateAccount(accountValues({ email, confirmEmail: email }))).toEqual({
      ...noErrors,
      email: "Enter a valid email address.",
    });
  });

  it.each(invalidPasswords)("rejects $password: $error", ({ password, error }) => {
    expect(validateCreateAccount(accountValues({ password, confirmPassword: password }))).toEqual({
      ...noErrors,
      password: error,
    });
  });

  it.each(confirmationPairs)("rejects mismatched $confirmation", ({ confirmation, different, error }) => {
    expect(validateCreateAccount(accountValues({ [confirmation]: different }))).toEqual({
      ...noErrors,
      [confirmation]: error,
    });
  });

  it.each(["Abcdef1!", "Password1!", "Password1€"])("accepts valid matching values with password %j", (password) => {
    expect(validateCreateAccount(accountValues({ password, confirmPassword: password }))).toEqual(noErrors);
  });

  it("ignores surrounding spaces in validation and confirmation comparisons", () => {
    expect(validateCreateAccount(accountValues({
      email: " name@example.com ", password: " Password1! ",
    }))).toEqual(noErrors);
  });
});

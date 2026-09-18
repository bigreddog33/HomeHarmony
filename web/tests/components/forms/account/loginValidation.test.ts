import { describe, expect, it } from "vitest";
import {
  validateLogin,
  validateLoginField,
} from "@/components/forms/account/loginValidation";
import { invalidEmails, loginValues } from "../../../fixtures/auth";

describe("login validation", () => {
  it.each(["", "   "])("requires both fields when they contain %j", (value) => {
    expect(validateLogin({ email: value, password: value })).toEqual({
      email: "Email is required.",
      password: "Password is required.",
    });
  });

  it.each(invalidEmails)("rejects invalid email %j", (email) => {
    expect(validateLogin({ email, password: "password" })).toEqual({
      email: "Enter a valid email address.",
      password: undefined,
    });
  });

  it.each(["name@example.com", "Name+home@sub.example.com"])(
    "accepts valid email %j without enforcing a password policy",
    (email) => {
      expect(validateLogin({ email, password: "x" })).toEqual({
        email: undefined,
        password: undefined,
      });
    },
  );

  it("validates email after trimming the same surrounding spaces removed on submit", () => {
    expect(validateLoginField("email", "  name@example.com  ")).toBeUndefined();
  });

  it("reports only the missing password for a valid email", () => {
    expect(validateLogin({ email: "name@example.com", password: "" })).toEqual({
      email: undefined,
      password: "Password is required.",
    });
  });

  it("reports only the missing email for a valid password", () => {
    expect(validateLogin(loginValues({ email: "" }))).toEqual({
      email: "Email is required.",
      password: undefined,
    });
  });
});

"use client";

import type { ChangeEvent, FocusEvent, SubmitEvent } from "react";
import { useState } from "react";
import { createPortal } from "react-dom";
import { useRouter } from "next/navigation";
import type { LoginRequest } from "@/contracts/auth/LoginRequest";
import { ApiError } from "@/services/apiClient";
import { getApiErrorMessage } from "@/components/feedback/ApiErrorMessage";
import { login, LoginApiError } from "@/services/auth/login";
import Toast from "@/components/feedback/Toast";
import FormInput from "../FormInput";
import PasswordInput from "../PasswordInput";
import {
  validateLogin,
  validateLoginField,
  type LoginField,
  type LoginFieldErrors,
} from "./loginValidation";

const initialValues: LoginRequest = {
  email: "",
  password: "",
};

export default function LoginForm() {
  const router = useRouter();
  const [values, setValues] = useState<LoginRequest>(initialValues);
  const [fieldErrors, setFieldErrors] = useState<LoginFieldErrors>({});
  const [credentialsError, setCredentialsError] = useState<string>();
  const [toastMessage, setToastMessage] = useState<string>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    const field = event.currentTarget.name as LoginField;
    const value = event.currentTarget.value;

    setValues((current) => ({ ...current, [field]: value }));
    setFieldErrors((current) => ({ ...current, [field]: undefined }));
    setCredentialsError(undefined);
  }

  function handleBlur(event: FocusEvent<HTMLInputElement>) {
    const field = event.currentTarget.name as LoginField;
    const error = validateLoginField(field, event.currentTarget.value);

    setFieldErrors((current) => ({ ...current, [field]: error }));
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isSubmitting) return;

    const errors = validateLogin(values);
    setFieldErrors(errors);
    setCredentialsError(undefined);
    setToastMessage(undefined);

    const firstInvalidField = (Object.keys(errors) as LoginField[]).find(
      (field) => errors[field],
    );

    if (firstInvalidField) {
      document.getElementById(firstInvalidField)?.focus();
      return;
    }

    setIsSubmitting(true);

    try {
      await login({
        email: values.email.trim(),
        password: values.password.trim(),
      });
      router.push("/home");
    } catch (error) {
      if (error instanceof ApiError) {
        setToastMessage(getApiErrorMessage(error));
        return;
      }

      if (error instanceof LoginApiError) {
        if (error.failure === "invalid-credentials") {
          setCredentialsError("The email or password is incorrect.");
          return;
        }

        if (error.failure === "invalid-request") {
          setCredentialsError("Check your email and password, then try again.");
          return;
        }
      }

      setToastMessage("An unexpected error occurred. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <>
      {toastMessage &&
        createPortal(
          <Toast message={toastMessage} onClose={() => setToastMessage(undefined)} />,
          document.body,
        )}

      <form
        onSubmit={handleSubmit}
        noValidate
        aria-busy={isSubmitting}
        className="space-y-5"
      >
        <FormInput
          id="email"
          name="email"
          type="email"
          label="Email"
          autoComplete="email"
          inputMode="email"
          placeholder="you@example.com"
          value={values.email}
          onChange={handleChange}
          onBlur={handleBlur}
          error={fieldErrors.email}
          disabled={isSubmitting}
        />

        <PasswordInput
          value={values.password}
          onChange={handleChange}
          onBlur={handleBlur}
          error={fieldErrors.password}
          disabled={isSubmitting}
        />

        {credentialsError && (
          <p
            role="alert"
            className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-800"
          >
            {credentialsError}
          </p>
        )}

        <div className="flex justify-end">
          <button
            type="button"
            disabled={isSubmitting}
            className="rounded text-sm font-medium text-purple-700 underline-offset-4 transition hover:text-purple-900 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Forgot password?
          </button>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="flex h-13 w-full items-center justify-center gap-3 rounded-xl bg-indigo-800 px-5 text-base font-bold text-white shadow-lg shadow-indigo-900/15 transition hover:bg-indigo-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 active:translate-y-px disabled:cursor-wait disabled:bg-indigo-500 disabled:active:translate-y-0"
        >
          {isSubmitting && (
            <svg
              className="h-5 w-5 animate-spin"
              viewBox="0 0 24 24"
              fill="none"
              aria-hidden="true"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="9"
                stroke="currentColor"
                strokeWidth="3"
              />
              <path
                className="opacity-90"
                d="M21 12a9 9 0 0 0-9-9"
                stroke="currentColor"
                strokeWidth="3"
                strokeLinecap="round"
              />
            </svg>
          )}
          {isSubmitting ? "Logging in..." : "Login"}
        </button>

        <button
          type="button"
          disabled={isSubmitting}
          onClick={() => router.push("/createAccount")}
          className="h-13 w-full rounded-xl border-2 border-indigo-800 bg-white px-5 text-base font-semibold text-indigo-800 transition hover:bg-indigo-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 active:translate-y-px disabled:cursor-not-allowed disabled:opacity-50"
        >
          Create account
        </button>
      </form>
    </>
  );
}

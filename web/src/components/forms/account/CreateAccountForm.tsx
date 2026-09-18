"use client";

import type { ChangeEvent, FocusEvent, SubmitEvent } from "react";
import { useState } from "react";
import { createPortal } from "react-dom";
import { useRouter } from "next/navigation";
import type { CreateAccountRequest } from "@/contracts/auth/CreateAccountRequest";
import { ApiError } from "@/services/apiClient";
import { getApiErrorMessage } from "@/components/feedback/ApiErrorMessage";
import { createAccount, CreateAccountApiError } from "@/services/auth/createAccount";
import Toast from "@/components/feedback/Toast";
import FormInput from "../FormInput";
import PasswordInput from "../PasswordInput";
import {
    validateCreateAccount,
    validateCreateAccountField,
    type CreateAccountField,
    type CreateAccountFieldErrors,
    type CreateAccountFormValue,
} from "./createAccountValidation";

const initialValues: CreateAccountFormValue = {
    email: "",
    password: "",
    confirmEmail: "",
    confirmPassword: "",
};

export default function CreateAccountForm() {
    const router = useRouter();
    const [values, setValues] = useState<CreateAccountFormValue>(initialValues);
    const [fieldErrors, setFieldErrors] = useState<CreateAccountFieldErrors>({});
    const [accountError, setAccountError] = useState<string>();
    const [toastMessage, setToastMessage] = useState<string>();
    const [isSubmitting, setIsSubmitting] = useState(false);

    function handleChange(event: ChangeEvent<HTMLInputElement>) {
        const field = event.currentTarget.name as CreateAccountField;
        const value = event.currentTarget.value;

        const nextValues = { ...values, [field]: value };
        setValues(nextValues);
        setFieldErrors((current) => {
            const nextErrors = { ...current, [field]: undefined };
            const confirmationField = field === "email"
                ? "confirmEmail"
                : field === "password" ? "confirmPassword" : undefined;

            if (confirmationField && confirmationField in current) {
                nextErrors[confirmationField] = validateCreateAccountField(confirmationField, nextValues);
            }

            return nextErrors;
        });
        setAccountError(undefined);
    }

    function handleBlur(event: FocusEvent<HTMLInputElement>) {
        const field = event.currentTarget.name as CreateAccountField;
        const error = validateCreateAccountField(field, values);

        setFieldErrors((current) => ({ ...current, [field]: error }));
    }

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault();
        if (isSubmitting) return;

        const errors = validateCreateAccount(values);
        setFieldErrors(errors);
        setToastMessage(undefined);
        setAccountError(undefined);

        const firstInvalidField = (Object.keys(errors) as CreateAccountField[]).find(
            (field) => errors[field],
        );

        if (firstInvalidField) {
            document.getElementById(firstInvalidField)?.focus();
            return;
        }

        setIsSubmitting(true);

        try {
            const request: CreateAccountRequest = {
                email: values.email.trim(),
                password: values.password.trim(),
            };

            await createAccount(request);
            router.replace("/successCreateAccount");
        } catch (error) {
            setIsSubmitting(false);
            if (error instanceof ApiError) {
                setToastMessage(getApiErrorMessage(error));
                return;
            }

            if (error instanceof CreateAccountApiError) {
                if (error.failure === "creation-failed") {
                    setAccountError("We couldn't create your account. Please try again.");
                    return;
                }

                if (error.failure === "invalid-request") {
                    setAccountError("Check your email and password, then try again.");
                    return;
                }
            }

            setToastMessage("An unexpected error occurred. Please try again.");
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
                    autoComplete="off"
                    inputMode="email"
                    placeholder="you@example.com"
                    value={values.email}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={fieldErrors.email}
                    disabled={isSubmitting}
                />

                <FormInput
                    id="confirmEmail"
                    name="confirmEmail"
                    type="email"
                    label="Confirm email"
                    autoComplete="off"
                    inputMode="email"
                    placeholder="Retype your email"
                    value={values.confirmEmail}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={fieldErrors.confirmEmail}
                    disabled={isSubmitting}
                />

                <div>
                    <PasswordInput
                        autoComplete="new-password"
                        descriptionId="password-requirements"
                        value={values.password}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        error={fieldErrors.password}
                        disabled={isSubmitting}
                    />
                    <p id="password-requirements" className="mt-2 text-xs leading-5 text-slate-500">
                        Use at least 8 characters, including an uppercase letter, a number,
                        and a special character. Leading and trailing spaces are ignored.
                    </p>
                </div>

                <PasswordInput
                    id="confirmPassword"
                    label="Confirm password"
                    autoComplete="new-password"
                    value={values.confirmPassword}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={fieldErrors.confirmPassword}
                    disabled={isSubmitting}
                />

                {accountError && (
                    <p
                        role="alert"
                        className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-800"
                    >
                        {accountError}
                    </p>
                )}

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
                    {isSubmitting ? "Creating account..." : "Create account"}
                </button>

            </form>
        </>
    );
}

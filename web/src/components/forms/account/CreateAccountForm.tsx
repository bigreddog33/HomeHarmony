"use client";

import type { ChangeEvent, FocusEvent, SubmitEvent } from "react";
import { useState } from "react";
import { createPortal } from "react-dom";
import { useRouter } from "next/navigation";
import type { CreateAccountRequest } from "@/contracts/auth/CreateAccountRequest";
import type { CreateAccountResponse } from "@/contracts/auth/CreateAccountResponse";
import { ApiError } from "@/services/apiClient";
import { getApiErrorMessage } from "@/components/feedback/ApiErrorMessage";
import { createAccount, CreateAccountApiError } from "@/services/auth/createAccount";
import { resendConfirmation } from "@/services/auth/resendConfirmation";
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
    const [createdStatus, setCreatedStatus] = useState<CreateAccountResponse["status"]>();

    const [isResending, setIsResending] = useState(false);
    const [recoveryAction, setRecoveryAction] = useState<"resend-confirmation" | "reset-password" | undefined>();
    const [recoveryMessage, setRecoveryMessage] = useState<string>();
    const isBusy = isSubmitting || isResending;

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
        setRecoveryAction(undefined);
        setRecoveryMessage(undefined);
        setAccountError(undefined);
    }

    function handleBlur(event: FocusEvent<HTMLInputElement>) {
        const field = event.currentTarget.name as CreateAccountField;
        const error = validateCreateAccountField(field, values);

        setFieldErrors((current) => ({ ...current, [field]: error }));
    }

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault();
        if (isBusy || createdStatus) return;

        const errors = validateCreateAccount(values);
        setFieldErrors(errors);
        setToastMessage(undefined);
        setRecoveryMessage(undefined);
        setRecoveryAction(undefined);
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

            const result = await createAccount(request);

            try {
                sessionStorage.removeItem("accountConfirmation");
                const confirmation = JSON.stringify({ email: request.email, status: result.status });
                sessionStorage.setItem("accountConfirmation", confirmation);
                if (sessionStorage.getItem("accountConfirmation") !== confirmation) {
                    throw new Error("Confirmation details could not be saved.");
                }
            } catch {
                // Keep confirmation recovery here when the next page cannot read the details.
                setCreatedStatus(result.status);
                setValues({ ...initialValues, email: request.email });
                setIsSubmitting(false);
                return;
            }

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

                if (error.failure === "already-exists") {
                    setAccountError("There is already an account existing with this email");
                    setRecoveryAction(error.emailConfirmed === false ? "resend-confirmation" : error.emailConfirmed === true ? "reset-password" : undefined);

                    return;
                }
            }

            setToastMessage("An unexpected error occurred. Please try again.");
        }
    }

    async function handleResendConfirmation() {
        if (isBusy || (recoveryAction !== "resend-confirmation" && createdStatus !== "CreatedConfirmationFailed")) return;

        setIsResending(true);
        setToastMessage(undefined);
        setRecoveryMessage(undefined);

        try {
            await resendConfirmation(values.email.trim());
            if (createdStatus) setCreatedStatus("Created");
            setRecoveryMessage("Your confirmation request was processed");
        }
        catch (error) {
            setToastMessage(
                error instanceof ApiError ? getApiErrorMessage(error) : "An unexpected error occurred. Please try again."
            );
        }
        finally {
            setIsResending(false);
        }
    }

    return (
        <>
            {toastMessage &&
                createPortal(
                    <Toast message={toastMessage} onClose={() => setToastMessage(undefined)} />,
                    document.body,
                )}

            {createdStatus ? (
                <div className="space-y-4" role="status">
                    <p className="font-semibold text-indigo-950">Account created</p>
                    <p className="text-slate-600">
                        {createdStatus === "CreatedConfirmationFailed"
                            ? "Your account was created, but we couldn’t send your confirmation email. You don’t need to create your account again."
                            : "Follow the confirmation link in your email to activate your account before logging in."}
                    </p>
                    {createdStatus === "CreatedConfirmationFailed" && (
                        <button
                            type="button"
                            onClick={handleResendConfirmation}
                            disabled={isResending}
                            className="rounded-xl bg-indigo-800 px-4 py-3 font-semibold text-white disabled:opacity-60"
                        >
                            {isResending ? "Requesting confirmation..." : "Resend confirmation link"}
                        </button>
                    )}
                </div>
            ) : <form
                onSubmit={handleSubmit}
                noValidate
                aria-busy={isBusy}
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
                    disabled={isBusy}
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
                    disabled={isBusy}
                />

                <div>
                    <PasswordInput
                        autoComplete="new-password"
                        descriptionId="password-requirements"
                        value={values.password}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        error={fieldErrors.password}
                        disabled={isBusy}
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
                    disabled={isBusy}
                />

                {accountError && (
                    <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-800">
                        <p role="alert">{accountError}</p>

                        {recoveryAction && (
                            <button
                                type="button"
                                className="mt-2 rounded underline underline-offset-4 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2"
                                onClick={recoveryAction === "resend-confirmation" ? handleResendConfirmation : undefined}
                                disabled={isBusy}
                            >
                                {recoveryAction === "resend-confirmation"
                                    ? isResending
                                        ? "Requesting confirmation..."
                                        : "Resend confirmation link"
                                    : "Reset password"}
                            </button>
                        )}
                    </div>
                )}

                {recoveryMessage && (
                    <p role="status" className="text-sm text-green-700">
                        {recoveryMessage}
                    </p>
                )}

                <button
                    type="submit"
                    disabled={isBusy}
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

            </form>}
        </>
    );
}

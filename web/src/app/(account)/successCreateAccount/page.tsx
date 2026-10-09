"use client";

import { useEffect, useState } from "react";
import { resendConfirmation } from "@/services/auth/resendConfirmation";
import { ApiError } from "@/services/apiClient";
import { getApiErrorMessage } from "@/components/feedback/ApiErrorMessage";

import Image from "next/image";
import Link from "next/link";

type ConfirmationContext = {
    email: string;
    status: "Created" | "CreatedConfirmationFailed";
};

export default function SuccessCreateAccountPage() {
    const [confirmation, setConfirmation] = useState<ConfirmationContext | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const [isResending, setIsResending] = useState(false);
    const [resendError, setResendError] = useState<string>();
    const [resendMessage, setResendMessage] = useState<string>();

    useEffect(() => {
        try {
            const saved = sessionStorage.getItem("accountConfirmation");
            const data: unknown = saved ? JSON.parse(saved) : null;

            if (
                typeof data === "object" &&
                data !== null &&
                "email" in data &&
                typeof data.email === "string" &&
                data.email.trim() !== "" &&
                "status" in data &&
                (data.status === "Created" ||
                    data.status === "CreatedConfirmationFailed")
            ) {
                // eslint-disable-next-line react-hooks/set-state-in-effect -- Read browser-only storage once after hydration.
                setConfirmation({
                    email: data.email,
                    status: data.status,
                });
            }
        } catch {
            // Missing or unreadable context leaves general instructions.
        } finally {
            setIsLoading(false);
        }
    }, []);

    async function handleResendConfirmation() {
        if (confirmation?.status !== "CreatedConfirmationFailed" || isResending) return;

        setIsResending(true);
        setResendError(undefined);
        setResendMessage(undefined);

        try {
            await resendConfirmation(confirmation.email);
            const updated: ConfirmationContext = { ...confirmation, status: "Created" };
            setConfirmation(updated);
            try {
                sessionStorage.setItem("accountConfirmation", JSON.stringify(updated));
            } catch {
                // Keep the successful result on screen even if storage is unavailable.
            }
            setResendMessage(
                "Your confirmation request was processed. Check your inbox; if your email is already confirmed, you can log in."
            );
        } catch (error) {
            setResendError(
                error instanceof ApiError
                    ? getApiErrorMessage(error)
                    : "An unexpected error occurred. Please try again."
            );
        } finally {
            setIsResending(false);
        }
    }

    const confirmationFailed = confirmation?.status === "CreatedConfirmationFailed";

    return (
        <main className="relative isolate grid min-h-screen place-items-center overflow-hidden bg-slate-50 px-5 py-10 sm:px-8">
            <div className="pointer-events-none absolute -right-28 -top-32 -z-10 h-[34rem] w-[34rem] rotate-[22deg] opacity-25">
                <Image src="/flower-logo.svg" alt="" fill sizes="34rem" priority className="object-contain" />
            </div>
            <div className="pointer-events-none absolute -bottom-44 -left-44 -z-10 h-[32rem] w-[32rem] -rotate-12 opacity-20">
                <Image src="/leaves-bottom.svg" alt="" fill sizes="32rem" className="object-contain" />
            </div>

            <section aria-labelledby="success-title" className="w-full max-w-xl rounded-[2rem] border border-white/80 bg-white/95 px-6 py-10 text-center shadow-[0_30px_80px_-30px_rgba(30,27,75,0.35)] backdrop-blur sm:px-12 sm:py-12">
                <p className="text-sm font-semibold tracking-tight text-indigo-800">HomeHarmony</p>
                <div className="relative mx-auto mb-7 mt-9 grid h-20 w-20 place-items-center rounded-3xl bg-indigo-50 text-indigo-800 ring-1 ring-indigo-100">
                    <svg viewBox="0 0 24 24" fill="none" className="h-10 w-10" aria-hidden="true">
                        <rect x="3" y="5" width="18" height="14" rx="3" stroke="currentColor" strokeWidth="1.5" />
                        <path d="m4 7 8 6 8-6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                    <span className="absolute -bottom-2 -right-2 grid h-8 w-8 place-items-center rounded-full bg-emerald-100 text-emerald-700 ring-4 ring-white" aria-hidden="true">
                        <svg viewBox="0 0 24 24" fill="none" className="h-5 w-5">
                            <path d="m5 12 4 4L19 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                    </span>
                </div>

                {confirmation && (
                    <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">Account created</p>
                )}
                <h1 id="success-title" className="text-3xl font-bold tracking-tight text-indigo-950 sm:text-4xl">Check your inbox</h1>

                <p className="mt-4 text-base leading-7 text-slate-600">
                    {isLoading
                        ? "Loading your account details..."
                        : confirmationFailed
                            ? "Your account was created, but we couldn’t send your confirmation email. Request another link below."
                            : "You’re nearly there. Follow the confirmation link in your email to activate your account before logging in for the first time."}
                </p>

                {!isLoading && (
                    <div className="my-8 rounded-2xl border border-indigo-100 bg-indigo-50/60 p-5 text-left">
                        <h2 className="text-sm font-semibold text-indigo-950">
                            {confirmationFailed
                                ? "Request another confirmation link"
                                : "Can’t find the email?"}
                        </h2>

                        <p className="mt-2 text-sm leading-6 text-slate-600">
                            {confirmationFailed
                                ? "You don’t need to create your account again."
                                : "It may take a few minutes to arrive. Check your spam or junk folder, too."}
                        </p>

                        {confirmationFailed && (
                            <button
                                type="button"
                                onClick={handleResendConfirmation}
                                disabled={isResending}
                                className="mt-4 rounded-xl bg-indigo-800 px-4 py-3 text-sm font-semibold text-white hover:bg-indigo-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-2 disabled:cursor-wait disabled:opacity-60"
                            >
                                {isResending
                                    ? "Requesting confirmation..."
                                    : "Resend confirmation link"}
                            </button>
                        )}

                        {resendError && (
                            <p role="alert" className="mt-3 text-sm text-red-700">
                                {resendError}
                            </p>
                        )}

                        {resendMessage && (
                            <p role="status" className="mt-3 text-sm text-green-700">
                                {resendMessage}
                            </p>
                        )}
                    </div>
                )}

                <Link href="/login" className="flex h-13 w-full items-center justify-center rounded-xl bg-indigo-800 px-5 text-base font-bold text-white shadow-lg shadow-indigo-900/15 transition hover:bg-indigo-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 active:translate-y-px">
                    Go to login
                </Link>
                <p className="mt-5 text-xs leading-5 text-slate-500">Once your email is confirmed, you’re ready to log in.</p>
            </section>
        </main>
    );
}

"use client";

import type { ChangeEventHandler, FocusEventHandler } from "react";
import { useState } from "react";
import FormInput from "./FormInput";

type PasswordInputProps = {
  id?: string;
  label?: string;
  autoComplete?: "current-password" | "new-password";
  descriptionId?: string;
  value: string;
  onChange: ChangeEventHandler<HTMLInputElement>;
  onBlur: FocusEventHandler<HTMLInputElement>;
  error?: string;
  disabled?: boolean;
};

export default function PasswordInput({
  id = "password",
  label = "Password",
  autoComplete = "current-password",
  descriptionId,
  value,
  onChange,
  onBlur,
  error,
  disabled,
}: PasswordInputProps) {
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div className="relative">
      <FormInput
        id={id}
        name={id}
        type={isVisible ? "text" : "password"}
        label={label}
        autoComplete={autoComplete}
        aria-describedby={descriptionId}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        error={error}
        disabled={disabled}
        className="pr-12"
      />
      <button
        type="button"
        onClick={() => setIsVisible((current) => !current)}
        disabled={disabled}
        className="absolute right-2 top-7 grid h-9 w-9 place-items-center rounded-lg text-slate-500 transition hover:bg-slate-100 hover:text-indigo-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
        aria-label={`${isVisible ? "Hide" : "Show"} ${label.toLowerCase()}`}
        aria-controls={id}
        aria-pressed={isVisible}
      >
        {isVisible ? (
          <svg
            viewBox="0 0 24 24"
            className="h-5 w-5"
            fill="none"
            aria-hidden="true"
          >
            <path
              d="M3 3l18 18M10.6 10.7a2 2 0 0 0 2.7 2.7M9.9 4.2A10.6 10.6 0 0 1 12 4c5.5 0 9 5.3 9 5.3a13.8 13.8 0 0 1-2.2 2.8M6.6 6.6C4.3 8.1 3 10.3 3 10.3S6.5 16 12 16c1 0 1.9-.2 2.7-.5"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        ) : (
          <svg
            viewBox="0 0 24 24"
            className="h-5 w-5"
            fill="none"
            aria-hidden="true"
          >
            <path
              d="M3 12s3.5-5.5 9-5.5 9 5.5 9 5.5-3.5 5.5-9 5.5S3 12 3 12Z"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinejoin="round"
            />
            <circle
              cx="12"
              cy="12"
              r="2.5"
              stroke="currentColor"
              strokeWidth="1.8"
            />
          </svg>
        )}
      </button>
    </div>
  );
}

"use client";

import { useState } from "react";
import FormInput from "./FormInput";

export default function PasswordInput() {
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div className="relative">
      <FormInput
        id="password"
        name="password"
        type={isVisible ? "text" : "password"}
        label="Password"
        autoComplete="current-password"
        className="pr-12"
        required
      />
      <button
        type="button"
        onClick={() => setIsVisible((current) => !current)}
        className="absolute bottom-2 right-2 grid h-9 w-9 place-items-center rounded-lg text-slate-500 transition hover:bg-slate-100 hover:text-indigo-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600"
        aria-label={isVisible ? "Hide password" : "Show password"}
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

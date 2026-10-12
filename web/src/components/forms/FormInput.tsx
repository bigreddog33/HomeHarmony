import type { InputHTMLAttributes, ReactNode } from "react";

type FormInputProps = InputHTMLAttributes<HTMLInputElement> & {
  id: string;
  label: string;
  error?: string;
  endAdornment?: ReactNode;
};

export default function FormInput({
  id,
  label,
  error,
  endAdornment,
  className = "",
  ...inputProps
}: FormInputProps) {
  const errorId = `${id}-error`;

  return (
    <div>
      <label
        htmlFor={id}
        className="mb-2 block text-sm font-semibold text-slate-700"
      >
        {label}
      </label>
      <div className="relative flex items-center">
        <input
          {...inputProps}
          id={id}
          aria-invalid={Boolean(error)}
          aria-describedby={
            [inputProps["aria-describedby"], error ? errorId : undefined]
              .filter(Boolean)
              .join(" ") || undefined
          }
          className={`h-13 w-full rounded-xl border bg-white px-4 text-base text-slate-950 outline-none transition placeholder:text-slate-400 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500 ${
            error
              ? "border-red-500 focus:border-red-600 focus:ring-3 focus:ring-red-100"
              : "border-slate-300 hover:border-slate-400 focus:border-indigo-700 focus:ring-3 focus:ring-indigo-100"
          } ${className}`}
        />
        {endAdornment}
      </div>
      {error && (
        <p id={errorId} className="mt-1.5 text-sm font-medium text-red-700">
          {error}
        </p>
      )}
    </div>
  );
}

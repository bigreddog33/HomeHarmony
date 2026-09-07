import type { InputHTMLAttributes } from "react";

type FormInputProps = InputHTMLAttributes<HTMLInputElement> & {
  id: string;
  label: string;
};

export default function FormInput({
  id,
  label,
  className = "",
  ...inputProps
}: FormInputProps) {
  return (
    <div>
      <label
        htmlFor={id}
        className="mb-2 block text-sm font-semibold text-slate-700"
      >
        {label}
      </label>
      <input
        id={id}
        className={`h-13 w-full rounded-xl border border-slate-300 bg-white px-4 text-base text-slate-950 outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-indigo-700 focus:ring-3 focus:ring-indigo-100 ${className}`}
        {...inputProps}
      />
    </div>
  );
}

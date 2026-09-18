type ToastProps = {
  message: string;
  onClose: () => void;
};

export default function Toast({ message, onClose }: ToastProps) {
  return (
    <div
      role="alert"
      aria-live="assertive"
      className="fixed right-5 top-5 z-50 flex w-[calc(100%-2.5rem)] max-w-md items-start gap-3 rounded-xl border border-red-200 bg-white p-4 text-red-950 shadow-2xl shadow-slate-900/20 sm:right-8 sm:top-8"
    >
      <span className="grid h-7 w-7 shrink-0 place-items-center rounded-full bg-red-100 font-bold text-red-700">
        !
      </span>
      <p className="pt-0.5 text-sm font-medium leading-6">{message}</p>
      <button
        type="button"
        onClick={onClose}
        className="ml-2 rounded px-1 text-xl leading-none text-red-700 hover:bg-red-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-600"
        aria-label="Dismiss notification"
      >
        ×
      </button>
    </div>
  );
}

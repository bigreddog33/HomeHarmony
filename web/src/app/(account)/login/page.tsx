import LoginForm from "@/components/forms/account/LoginForm";
import AccountPageLayout from "@/components/layouts/AccountPageLayout";

export default function LoginPage() {
  return (
    <AccountPageLayout
      titleId="login-title"
      sidebar={
        <>
          <p className="mb-5 text-sm font-semibold uppercase tracking-[0.24em] text-violet-200">
            Your home, in rhythm
          </p>
          <h2 className="text-5xl font-bold leading-[1.08] tracking-[-0.04em]">
            More harmony.
            <br />
            Less household noise.
          </h2>
          <p className="mt-6 max-w-sm text-base leading-7 text-indigo-100/80">
            Keep the people, plans, and everyday details of your home together
            in one calm place.
          </p>
        </>
      }
    >
      <header className="mb-9">
        <p className="text-5xl font-extrabold leading-none tracking-[-0.055em] text-indigo-800 sm:text-6xl">
          Welcome
        </p>
        <h1
          id="login-title"
          className="mt-[-0.08em] pl-1 text-4xl font-light uppercase tracking-[0.18em] text-purple-700 sm:text-5xl"
        >
          Home
        </h1>
        <p className="mt-5 text-sm leading-6 text-slate-500 sm:text-base">
          Sign in to continue to your household.
        </p>
      </header>

      <LoginForm />

      <div className="mt-8 border-t border-slate-200 pt-6 text-center">
        <button
          type="button"
          className="rounded text-sm font-medium text-indigo-700 underline underline-offset-4 transition hover:text-indigo-950 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4"
        >
          Policies and terms
        </button>
      </div>
    </AccountPageLayout>
  );
}

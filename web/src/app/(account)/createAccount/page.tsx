import Link from "next/link";
import CreateAccountForm from "@/components/forms/account/CreateAccountForm";
import AccountPageLayout from "@/components/layouts/AccountPageLayout";

export default function CreateAccountPage() {
  return (
    <AccountPageLayout
      titleId="create-account-title"
      sidebar={
        <>
          <p className="mb-5 text-xs font-semibold uppercase tracking-[0.24em] text-violet-200">A little more together</p>
          <h2 className="text-5xl font-bold leading-[1.08] tracking-[-0.04em]">
            Make room<br />for harmony.
          </h2>
          <p className="mt-6 text-base leading-7 text-indigo-100/80">
            One place for your household, your shared plans, and the everyday details that matter.
          </p>

          <ol className="mt-10 space-y-5 text-sm">
            <li className="flex items-center gap-4">
              <span className="grid h-9 w-9 shrink-0 place-items-center rounded-full bg-white/15 font-semibold ring-1 ring-white/20">1</span>
              <span>Create your account</span>
            </li>
            <li className="flex items-center gap-4 text-indigo-200">
              <span className="grid h-9 w-9 shrink-0 place-items-center rounded-full font-semibold ring-1 ring-white/20">2</span>
              <span>Confirm your email and log in</span>
            </li>
          </ol>
        </>
      }
    >
      <header className="mb-7">
        <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-purple-700">Let’s get started</p>
        <h1 id="create-account-title" className="text-3xl font-bold tracking-tight text-indigo-950 sm:text-4xl">Create account</h1>
        <p className="mt-3 text-sm leading-6 text-slate-500">A calmer home starts with a few details.</p>
      </header>

      <CreateAccountForm />

      <p className="mt-7 border-t border-slate-200 pt-6 text-center text-sm text-slate-600">
        Already have an account?{" "}
        <Link href="/login" className="rounded font-semibold text-indigo-800 underline underline-offset-4 transition hover:text-indigo-950 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4">
          Log in
        </Link>
      </p>
    </AccountPageLayout>
  );
}

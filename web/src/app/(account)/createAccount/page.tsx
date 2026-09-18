import Image from "next/image";
import Link from "next/link";
import CreateAccountForm from "@/components/forms/account/CreateAccountForm";

export default function CreateAccountPage() {
  return (
    <main className="relative isolate flex min-h-screen items-center justify-center overflow-hidden bg-slate-50 px-5 py-8 sm:px-8 lg:px-12">
      <div className="pointer-events-none absolute -right-28 -top-32 -z-10 h-[34rem] w-[34rem] rotate-[22deg] opacity-25 lg:h-[44rem] lg:w-[44rem]">
        <Image src="/flower-logo.svg" alt="" fill sizes="44rem" priority className="object-contain" />
      </div>
      <div className="pointer-events-none absolute -bottom-44 -left-44 -z-10 h-[32rem] w-[32rem] -rotate-12 opacity-20">
        <Image src="/leaves-bottom.svg" alt="" fill sizes="32rem" className="object-contain" />
      </div>

      <section
        aria-labelledby="create-account-title"
        className="grid w-full max-w-5xl overflow-hidden rounded-[2rem] border border-white/80 bg-white/95 shadow-[0_30px_80px_-30px_rgba(30,27,75,0.35)] backdrop-blur lg:grid-cols-[0.9fr_1.1fr]"
      >
        <aside className="relative hidden overflow-hidden bg-indigo-950 px-10 py-12 text-white lg:flex lg:flex-col lg:justify-between">
          <div className="pointer-events-none absolute -right-24 -top-20 h-80 w-80 rotate-[18deg] opacity-30">
            <Image src="/flower-logo.svg" alt="" fill sizes="20rem" className="object-contain" />
          </div>
          <p className="relative text-lg font-semibold tracking-tight">HomeHarmony</p>

          <div className="relative py-16">
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
          </div>

          <p className="relative text-sm text-indigo-200/70">Made for life together.</p>
        </aside>

        <div className="px-6 py-9 sm:px-12 sm:py-12">
          <div className="mx-auto w-full max-w-md">
            <p className="mb-7 font-semibold text-indigo-800 lg:hidden">HomeHarmony</p>
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
          </div>
        </div>
      </section>
    </main>
  );
}

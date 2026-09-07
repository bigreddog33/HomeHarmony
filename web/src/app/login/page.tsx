"use client";

import type { FormEvent } from "react";
import Image from "next/image";
import FormInput from "@/components/forms/FormInput";
import PasswordInput from "@/components/forms/PasswordInput";

export default function LoginPage() {
  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
  }

  return (
    <main className="relative isolate flex min-h-screen items-center justify-center overflow-hidden bg-slate-50 px-5 py-8 sm:px-8 lg:px-12">
      <div className="pointer-events-none absolute -right-28 -top-32 -z-10 h-[34rem] w-[34rem] rotate-[22deg] opacity-25 lg:-right-16 lg:-top-40 lg:h-[44rem] lg:w-[44rem]">
        <Image
          className="object-contain"
          src="/flower-logo.svg"
          alt=""
          fill
          sizes="44rem"
          priority
        />
      </div>

      <div className="pointer-events-none absolute -bottom-44 -left-44 -z-10 h-[32rem] w-[32rem] -rotate-12 opacity-20 lg:-bottom-52 lg:-left-28 lg:h-[44rem] lg:w-[44rem]">
        <Image
          className="object-contain"
          src="/leaves-bottom.svg"
          alt=""
          fill
          sizes="44rem"
        />
      </div>

      <section
        className="grid w-full max-w-5xl overflow-hidden rounded-[2rem] border border-white/80 bg-white/90 shadow-[0_30px_80px_-30px_rgba(30,27,75,0.35)] backdrop-blur sm:min-h-[640px] lg:grid-cols-[1.05fr_1fr]"
        aria-labelledby="login-title"
      >
        <div className="relative hidden overflow-hidden bg-indigo-950 px-12 py-14 text-white lg:flex lg:flex-col lg:justify-between">
          <div className="absolute -right-24 -top-20 h-80 w-80 rotate-[18deg] opacity-30">
            <Image
              className="object-contain"
              src="/flower-logo.svg"
              alt=""
              fill
              sizes="20rem"
            />
          </div>

          <div className="relative flex items-center gap-3">
            <span className="grid h-11 w-11 place-items-center rounded-2xl bg-white/10 ring-1 ring-white/20">
              <svg
                viewBox="0 0 24 24"
                className="h-6 w-6"
                fill="none"
                aria-hidden="true"
              >
                <path
                  d="M3 11.5 12 4l9 7.5"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <path
                  d="M5.5 10.5V20h13v-9.5M9.5 20v-5h5v5"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </span>
            <span className="text-lg font-semibold tracking-tight">
              HomeHarmony
            </span>
          </div>

          <div className="relative max-w-md">
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
          </div>

          <p className="relative text-sm text-indigo-200/70">
            Made for life together.
          </p>
        </div>

        <div className="flex items-center justify-center px-6 py-10 sm:px-14 sm:py-14 lg:px-16">
          <div className="w-full max-w-md">
            <div className="mb-8 flex items-center gap-3 text-indigo-800 lg:hidden">
              <span className="grid h-10 w-10 place-items-center rounded-xl bg-indigo-50 ring-1 ring-indigo-100">
                <svg
                  viewBox="0 0 24 24"
                  className="h-5 w-5"
                  fill="none"
                  aria-hidden="true"
                >
                  <path
                    d="M3 11.5 12 4l9 7.5"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                  <path
                    d="M5.5 10.5V20h13v-9.5M9.5 20v-5h5v5"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </span>
              <span className="font-semibold tracking-tight">HomeHarmony</span>
            </div>

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

            <form onSubmit={handleSubmit} className="space-y-5">
              <FormInput
                id="email"
                name="email"
                type="email"
                label="Email"
                autoComplete="email"
                placeholder="you@example.com"
                required
              />

              <PasswordInput />

              <div className="flex justify-end">
                <button
                  type="button"
                  className="rounded text-sm font-medium text-purple-700 underline-offset-4 transition hover:text-purple-900 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4"
                >
                  Forgot password?
                </button>
              </div>

              <button
                type="submit"
                className="h-13 w-full rounded-xl bg-indigo-800 px-5 text-base font-bold text-white shadow-lg shadow-indigo-900/15 transition hover:bg-indigo-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 active:translate-y-px"
              >
                Login
              </button>

              <button
                type="button"
                className="h-13 w-full rounded-xl border-2 border-indigo-800 bg-white px-5 text-base font-semibold text-indigo-800 transition hover:bg-indigo-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4 active:translate-y-px"
              >
                Create user
              </button>
            </form>

            <div className="mt-8 border-t border-slate-200 pt-6 text-center">
              <button
                type="button"
                className="rounded text-sm font-medium text-indigo-700 underline underline-offset-4 transition hover:text-indigo-950 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-600 focus-visible:ring-offset-4"
              >
                Policies and terms
              </button>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}

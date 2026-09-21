import Image from "next/image";
import type { ReactNode } from "react";

type AccountPageLayoutProps = {
  titleId: string;
  sidebar: ReactNode;
  children: ReactNode;
};

export default function AccountPageLayout({
  titleId,
  sidebar,
  children,
}: AccountPageLayoutProps) {
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

      {/* Reserve room for the longer registration form; allow errors and zoom to expand it. */}
      <section
        className="grid min-h-[60rem] w-full max-w-5xl overflow-hidden rounded-[2rem] border border-white/80 bg-white/90 shadow-[0_30px_80px_-30px_rgba(30,27,75,0.35)] backdrop-blur sm:min-h-[56rem] lg:grid-cols-[1.05fr_1fr]"
        aria-labelledby={titleId}
      >
        <div className="relative hidden overflow-hidden bg-indigo-950 px-12 py-14 text-white lg:flex lg:flex-col lg:justify-between">
          <div className="pointer-events-none absolute -right-24 -top-20 h-80 w-80 rotate-[18deg] opacity-30">
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

          <div className="relative max-w-md">{sidebar}</div>

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

            {children}
          </div>
        </div>
      </section>
    </main>
  );
}

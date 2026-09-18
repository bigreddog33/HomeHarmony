import { beforeEach, vi } from "vitest";

const router = vi.hoisted(() => ({
  push: vi.fn(),
  replace: vi.fn(),
  redirect: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => router,
  redirect: router.redirect,
}));

export const navigation = router;

beforeEach(() => {
  navigation.push.mockReset();
  navigation.replace.mockReset();
  navigation.redirect.mockReset();
});

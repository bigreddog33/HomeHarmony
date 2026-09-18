// @vitest-environment jsdom
import "../support/dom";
import { navigation } from "../support/navigationStub";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import Home from "@/app/page";
import LoginPage from "@/app/(account)/login/page";
import CreateAccountPage from "@/app/(account)/createAccount/page";
import SuccessCreateAccountPage from "@/app/(account)/successCreateAccount/page";

describe("account navigation", () => {
  it("redirects the root route to login", () => {
    const redirect = new Error("Redirect ends rendering");
    navigation.redirect.mockImplementation(() => { throw redirect; });

    expect(() => Home()).toThrow(redirect);
    expect(navigation.redirect).toHaveBeenCalledExactlyOnceWith("/login");
  });

  it("opens account creation from the login page without submitting", async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.click(screen.getByRole("button", { name: "Create account" }));

    expect(navigation.push).toHaveBeenCalledExactlyOnceWith("/createAccount");
    expect(screen.queryByText("Email is required.")).not.toBeInTheDocument();
  });

  it.each([
    { name: "registration", Page: CreateAccountPage, link: "Log in" },
    { name: "registration success", Page: SuccessCreateAccountPage, link: "Go to login" },
  ])("links from $name back to login", ({ Page, link }) => {
    render(<Page />);

    expect(screen.getByRole("link", { name: link })).toHaveAttribute("href", "/login");
  });
});

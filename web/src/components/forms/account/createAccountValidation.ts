import type { CreateAccountRequest } from "@/contracts/auth/CreateAccountRequest";

export type CreateAccountFormValue = CreateAccountRequest & {
    confirmEmail: string;
    confirmPassword: string;
};

export type CreateAccountField = keyof CreateAccountFormValue;
export type CreateAccountFieldErrors = Partial<Record<CreateAccountField, string>>;

export function validateCreateAccountField(
    field: CreateAccountField,
    value: CreateAccountFormValue
): string | undefined {
    switch (field) {
        case "email":
            if (!value.email.trim()) return "Email is required.";
            if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.email.trim())) {
                return "Enter a valid email address.";
            }
            return undefined;

        case "confirmEmail":
            if (!value.confirmEmail.trim()) return "Please retype your email.";
            if (value.confirmEmail.trim() !== value.email.trim()) {
                return "Email addresses do not match.";
            }
            return undefined;

        case "password":
            if (!value.password.trim()) return "Password is required.";
            if (value.password.trim().length < 8) return "Password must contain at least 8 characters.";
            if (!/[A-Z]/.test(value.password.trim())) return "Password must contain at least one uppercase letter.";
            if (!/[0-9]/.test(value.password.trim())) return "Password must contain at least one number.";
            if (!/[\p{P}\p{S}]/u.test(value.password.trim())) return "Password must contain at least one special character.";
            return undefined;

        case "confirmPassword":
            if (!value.confirmPassword.trim()) return "Please retype your password.";
            if (value.confirmPassword.trim() !== value.password.trim()) {
                return "Passwords do not match.";
            }
            return undefined;
    }
}

export function validateCreateAccount(value: CreateAccountFormValue): CreateAccountFieldErrors {
    return {
        email: validateCreateAccountField("email", value),
        confirmEmail: validateCreateAccountField("confirmEmail", value),
        password: validateCreateAccountField("password", value),
        confirmPassword: validateCreateAccountField("confirmPassword", value)
    };
}

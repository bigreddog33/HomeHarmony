import type { CreateAccountRequest } from "@/contracts/auth/CreateAccountRequest";
import { CreateAccountResponse } from "@/contracts/auth/CreateAccountResponse";
import { ApiError, postJson } from "@/services/apiClient";

export type CreateAccountFailure = "creation-failed" | "invalid-request" | "already-exists";

export class CreateAccountApiError extends Error {
    constructor(public readonly failure: CreateAccountFailure, public readonly emailConfirmed?: boolean) {
        super(failure);
        this.name = "CreateAccountApiError";
    }
}

export async function createAccount(request: CreateAccountRequest): Promise<CreateAccountResponse> {
    try {
        const body = await postJson("/api/account/createuser", request, true);

        if (
            typeof body !== "object" ||
            body === null ||
            !("status" in body) ||
            (body.status !== "Created" && body.status !== "CreatedConfirmationFailed")
        ) {
            throw new Error("Unexpected account creation response.");
        }

        return { status: body.status };
    } catch (error) {
        if (!(error instanceof ApiError)) {
            throw error;
        }

        // The controller currently returns 401 when account creation fails.
        if (error.status === 401) {
            throw new CreateAccountApiError("creation-failed");
        }

        if (error.status === 400) {
            throw new CreateAccountApiError("invalid-request");
        }

        if (error.status === 409) {
            const body = error.body;
            const emailConfirmed = typeof body === "object" && body != null && "emailConfirmed" in body && typeof body.emailConfirmed === "boolean" ? body.emailConfirmed : undefined;

            throw new CreateAccountApiError("already-exists", emailConfirmed);
        }

        throw error;
    }
}

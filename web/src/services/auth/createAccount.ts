import type { CreateAccountRequest } from "@/contracts/auth/CreateAccountRequest";
import { ApiError, postJson } from "@/services/apiClient";

export type CreateAccountFailure = "creation-failed" | "invalid-request";

export class CreateAccountApiError extends Error {
    constructor(public readonly failure: CreateAccountFailure) {
        super(failure);
        this.name = "CreateAccountApiError";
    }
}

export async function createAccount(request: CreateAccountRequest): Promise<void> {
    try {
        await postJson("/api/account/createuser", request);
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

        throw error;
    }
}

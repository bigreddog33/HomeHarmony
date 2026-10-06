import { ApiError, postJson } from "../apiClient";

export async function resendConfirmation(email: string): Promise<void> {
    await postJson("/api/account/resendconfirmation", {email});
}
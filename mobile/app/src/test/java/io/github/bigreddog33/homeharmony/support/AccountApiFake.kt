package io.github.bigreddog33.homeharmony.support

import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountResponse
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import io.github.bigreddog33.homeharmony.data.account.dto.ResendConfirmationRequest

internal class AccountApiFake : AccountApi {
    val creations = mutableListOf<CreateAccountRequest>()
    val resends = mutableListOf<ResendConfirmationRequest>()
    var create: suspend () -> CreateAccountResponse = { CreateAccountResponse("Created") }
    var resend: suspend () -> Unit = {}

    override suspend fun createAccount(request: CreateAccountRequest): CreateAccountResponse {
        creations += request
        return create()
    }

    override suspend fun resendConfirmation(request: ResendConfirmationRequest) {
        resends += request
        resend()
    }

    override suspend fun login(request: LoginRequest) {
        error("Registration and confirmation must not call login.")
    }
}

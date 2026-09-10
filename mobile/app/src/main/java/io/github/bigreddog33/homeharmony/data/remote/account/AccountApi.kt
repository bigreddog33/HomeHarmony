package io.github.bigreddog33.homeharmony.data.remote.account

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface AccountApi {
    @POST("api/account/login")
    suspend fun login(
        @Body request: LoginRequest
    ): Response<Unit>
}

package io.github.bigreddog33.homeharmony.data.remote

import io.github.bigreddog33.homeharmony.BuildConfig
import io.github.bigreddog33.homeharmony.data.remote.account.AccountApi
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {
    private val retrofit = Retrofit.Builder()
        .baseUrl(BuildConfig.API_BASE_URL)
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    val accountApi: AccountApi = retrofit.create(AccountApi::class.java)
}

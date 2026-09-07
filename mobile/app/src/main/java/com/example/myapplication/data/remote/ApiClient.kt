package com.example.myapplication.data.remote

import com.example.myapplication.BuildConfig
import com.example.myapplication.data.remote.account.AccountApi
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {
    private val retrofit = Retrofit.Builder()
        .baseUrl(BuildConfig.API_BASE_URL)
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    val accountApi: AccountApi = retrofit.create(AccountApi::class.java)
}

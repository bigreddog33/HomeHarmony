package io.github.bigreddog33.homeharmony.data.account

import com.google.gson.JsonParser
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import io.github.bigreddog33.homeharmony.data.network.ApiResult
import io.github.bigreddog33.homeharmony.data.network.apiCall
import kotlinx.coroutines.test.runTest
import okhttp3.OkHttpClient
import okhttp3.Protocol
import okhttp3.Request
import okhttp3.Response
import okhttp3.ResponseBody.Companion.toResponseBody
import okio.Buffer
import org.junit.Assert.assertEquals
import org.junit.Test
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.io.IOException

class AccountApiTest {
    @Test
    fun `login posts credentials and accepts an empty 200 response`() = runTest {
        val requests = mutableListOf<Request>()
        val api = accountApi { request ->
            requests += request
            response(request, 200)
        }

        assertEquals(ApiResult.Success(Unit), apiCall {
            api.login(LoginRequest("person@example.com", " x "))
        })
        val request = requests.single()
        assertEquals("POST", request.method)
        assertEquals("/api/account/login", request.url.encodedPath)
        val buffer = Buffer()
        requireNotNull(request.body).writeTo(buffer)
        val json = JsonParser.parseString(buffer.readUtf8()).asJsonObject
        assertEquals("person@example.com", json["email"].asString)
        assertEquals(" x ", json["password"].asString)
    }

    @Test
    fun `login accepts a 204 response`() = runTest {
        val api = accountApi { response(it, 204) }

        assertEquals(ApiResult.Success(Unit), apiCall {
            api.login(LoginRequest("person@example.com", "password"))
        })
    }

    @Test
    fun `HTTP failures retain their status`() = runTest {
        for (status in listOf(400, 401, 403, 500)) {
            val api = accountApi { response(it, status) }

            assertEquals(ApiResult.HttpError(status), apiCall {
                api.login(LoginRequest("person@example.com", "password"))
            })
        }
    }

    @Test
    fun `connection failure maps to a network outcome`() = runTest {
        val api = accountApi { throw IOException("Private host details") }

        assertEquals(ApiResult.NetworkError, apiCall {
            api.login(LoginRequest("person@example.com", "password"))
        })
    }

    private fun accountApi(respond: (Request) -> Response): AccountApi {
        // Exercise Retrofit serialization and HTTP handling without opening a socket.
        val httpClient = OkHttpClient.Builder()
            .addInterceptor { chain -> respond(chain.request()) }
            .build()
        return Retrofit.Builder()
            .baseUrl("https://example.test/")
            .client(httpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(AccountApi::class.java)
    }

    private fun response(request: Request, code: Int): Response = Response.Builder()
        .request(request)
        .protocol(Protocol.HTTP_1_1)
        .code(code)
        .message("Test response")
        .body("".toResponseBody())
        .build()
}

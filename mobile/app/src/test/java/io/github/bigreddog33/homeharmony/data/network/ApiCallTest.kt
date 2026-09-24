package io.github.bigreddog33.homeharmony.data.network

import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.test.runTest
import okhttp3.ResponseBody.Companion.toResponseBody
import org.junit.Assert.assertEquals
import org.junit.Assert.assertSame
import org.junit.Assert.fail
import org.junit.Test
import retrofit2.HttpException
import retrofit2.Response
import java.io.IOException

class ApiCallTest {
    @Test
    fun `successful requests preserve their body`() = runTest {
        assertEquals(ApiResult.Success("body"), apiCall { "body" })
    }

    @Test
    fun `HTTP failures retain their status for feature mapping`() = runTest {
        val result = apiCall<Unit> {
            throw HttpException(Response.error<Unit>(409, "Conflict".toResponseBody()))
        }

        assertEquals(ApiResult.HttpError(409), result)
    }

    @Test
    fun `IO failures become network errors`() = runTest {
        assertEquals(ApiResult.NetworkError, apiCall<Unit> { throw IOException() })
    }

    @Test
    fun `unexpected failures become safe errors`() = runTest {
        assertEquals(ApiResult.UnexpectedError, apiCall<Unit> { error("Private details") })
    }

    @Test
    fun `cancellation propagates unchanged`() = runTest {
        val cancellation = CancellationException("Cancelled")

        try {
            apiCall<Unit> { throw cancellation }
            fail("Cancellation must propagate")
        } catch (exception: CancellationException) {
            assertSame(cancellation, exception)
        }
    }
}

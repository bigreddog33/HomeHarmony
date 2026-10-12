package io.github.bigreddog33.homeharmony.data.account

import org.junit.Assert.*
import org.junit.Test

class AccountErrorParserTest {
    @Test
    fun acceptsOnlyAnExplicitJsonBoolean() {
        assertEquals(true, parseEmailConfirmed("""{"status":"AlreadyExists","emailConfirmed":true}"""))
        assertEquals(false, parseEmailConfirmed("""{"emailConfirmed":false}"""))
    }

    @Test
    fun missingMalformedOrWrongTypeMeansUnknownRatherThanUnconfirmed() {
        for (body in listOf(null, "", " ", "{", "not JSON", "null", "[]", "true", "123",
            "{}", """{"emailConfirmed":null}""", """{"emailConfirmed":"false"}""",
            """{"emailConfirmed":0}""", """{"emailConfirmed":{}}""",
            """{"emailConfirmed":[]}""", """{"EmailConfirmed":false}""")) {
            assertNull("Body: $body", parseEmailConfirmed(body))
        }
    }
}

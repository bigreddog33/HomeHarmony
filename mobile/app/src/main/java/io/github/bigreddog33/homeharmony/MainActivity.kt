package io.github.bigreddog33.homeharmony

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import io.github.bigreddog33.homeharmony.ui.navigation.AppNavigation
import io.github.bigreddog33.homeharmony.ui.theme.HomeHarmonyTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            HomeHarmonyTheme {
                AppNavigation()
            }
        }
    }
}
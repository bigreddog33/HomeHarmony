package io.github.bigreddog33.homeharmony.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import io.github.bigreddog33.homeharmony.ui.screens.home.HomeScreen
import io.github.bigreddog33.homeharmony.ui.screens.login.LoginScreen
import io.github.bigreddog33.homeharmony.ui.screens.createAccount.CreateAccountScreen

@Composable
fun AppNavigation() {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = "login"
    ) {
        composable("login") {
            LoginScreen(
                onLoginSuccess = {
                    navController.navigate("home") {
                        popUpTo("login") {
                            inclusive = true
                        }
                    }
                }
            )
        }

        composable("home") {
            HomeScreen()
        }
        
        composable("createAccount") {
            CreateAccountScreen()
        }
    }
}
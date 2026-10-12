package io.github.bigreddog33.homeharmony.ui.navigation

import androidx.compose.runtime.Composable
import android.net.Uri
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavType
import androidx.navigation.navArgument
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import io.github.bigreddog33.homeharmony.ui.screens.home.HomeScreen
import io.github.bigreddog33.homeharmony.ui.screens.login.LoginScreen
import io.github.bigreddog33.homeharmony.ui.screens.createAccount.CreateAccountScreen
import io.github.bigreddog33.homeharmony.ui.screens.successCreateAccount.SuccessCreateAccountScreen
import io.github.bigreddog33.homeharmony.ui.confirmation.ResendConfirmationViewModel

@Composable
fun AppNavigation(viewModelFactory: ViewModelProvider.Factory? = null) {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = "login"
    ) {
        composable("login") {
            LoginScreen(
                viewModel = viewModel(factory = viewModelFactory),
                onLoginSuccess = {
                    navController.navigate("home") {
                        popUpTo("login") {
                            inclusive = true
                        }
                    }
                },
                onCreateAccountClick = {
                    navController.navigate("createAccount") {
                        launchSingleTop = true
                    }
                }
            )
        }

        composable("home") {
            HomeScreen()
        }
        
        composable("createAccount") {
            CreateAccountScreen(
                viewModel = viewModel(factory = viewModelFactory),
                resendViewModelFactory = viewModelFactory ?: ResendConfirmationViewModel.Factory,
                onCreateAccountSuccess = { email, status ->
                    val route = "successCreateAccount/${Uri.encode(email)}/${Uri.encode(status)}"

                    navController.navigate(route) {
                        popUpTo("createAccount") {
                            inclusive = true
                        }
                        launchSingleTop = true
                    }
                },
                onBackToLoginClick = {
                    navController.popBackStack("login", false)
                }
            )
        }

        composable(
            route = "successCreateAccount/{email}/{status}",
            arguments = listOf(navArgument("email") {type = NavType.StringType},navArgument("status") {type = NavType.StringType})
            ) {
                backStackEntry -> 
                    val email = requireNotNull(backStackEntry.arguments?.getString("email"))
                    val status = requireNotNull(backStackEntry.arguments?.getString("status"))
                    
                SuccessCreateAccountScreen(
                    resendViewModelFactory = viewModelFactory ?: ResendConfirmationViewModel.Factory,
                    email=email,
                    status=status,
                    onBackToLoginClick = {navController.popBackStack("login", false)}    
                )
        }
    }
}

package io.github.bigreddog33.homeharmony.ui.screens.successCreateAccount

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import io.github.bigreddog33.homeharmony.R

import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.compose.ui.semantics.LiveRegionMode
import androidx.compose.ui.semantics.liveRegion
import androidx.compose.ui.semantics.semantics
import io.github.bigreddog33.homeharmony.ui.confirmation.ResendConfirmationViewModel

@Composable
fun SuccessCreateAccountScreen(
    email: String,
    status: String,
    onBackToLoginClick: () -> Unit,
    resendViewModel: ResendConfirmationViewModel = viewModel(key = "resendConfirmation:$email")
) {
    val resendState = resendViewModel.uiState
    val confirmationFailed = status == "CreatedConfirmationFailed" && !resendState.isSuccessful
    
    Scaffold { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .consumeWindowInsets(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 24.dp, vertical = 32.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Card(
                modifier = Modifier
                    .widthIn(max = 480.dp)
                    .fillMaxWidth()
            ) {
                Column(
                    modifier = Modifier.padding(24.dp),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(20.dp)
                ) {
                    Text(
                        text = stringResource(R.string.app_name),
                        style = MaterialTheme.typography.titleMedium,
                        color = MaterialTheme.colorScheme.primary
                    )

                    Text(
                        text = stringResource(R.string.confirmation_account_created),
                        style = MaterialTheme.typography.labelLarge
                    )

                    Text(
                        text = stringResource(R.string.confirmation_title),
                        style = MaterialTheme.typography.headlineMedium,
                        textAlign = TextAlign.Center
                    )

                    Text(
                        text = stringResource(
                            if (confirmationFailed) R.string.confirmation_failed
                            else R.string.confirmation_sent
                        ),
                        style = MaterialTheme.typography.bodyLarge,
                        textAlign = TextAlign.Center
                    )

                    Text(
                        text = stringResource(R.string.confirmation_email, email),
                        style = MaterialTheme.typography.bodyMedium,
                        textAlign = TextAlign.Center
                    )

                    Text(
                        text = stringResource(
                            if (confirmationFailed) R.string.confirmation_failed_hint
                            else R.string.confirmation_inbox_hint
                        ),
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center
                    )
                    
                    if (confirmationFailed) {
                        OutlinedButton(
                            onClick = { resendViewModel.resend(email) },
                            enabled = !resendState.isLoading,
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text(
                                stringResource(
                                    if (resendState.isLoading) R.string.confirmation_resending
                                    else R.string.confirmation_resend_button
                                )
                            )
                        }

                        resendState.errorMessage?.let { message ->
                            Text(
                                text = stringResource(message),
                                color = MaterialTheme.colorScheme.error,
                                textAlign = TextAlign.Center,
                                modifier = Modifier.semantics {
                                    liveRegion = LiveRegionMode.Polite
                                }
                            )
                        }
                    }

                    if (resendState.isSuccessful) {
                        Text(
                            text = stringResource(R.string.confirmation_resend_success),
                            color = MaterialTheme.colorScheme.primary,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.semantics {
                                liveRegion = LiveRegionMode.Polite
                            }
                        )
                    }

                    Button(
                        onClick = onBackToLoginClick,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(stringResource(R.string.confirmation_go_to_login))
                    }
                }
            }
        }
    }
}

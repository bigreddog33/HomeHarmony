import type { ApiError } from "@/services/apiClient";

export function getApiErrorMessage(error: ApiError): string {
  switch (error.failure) {
    case "timeout":
      return "The server is taking too long to respond. Please try again.";

    case "network":
      return "We couldn't connect right now. Check your internet connection and try again.";

    case "configuration":
      return "The service is currently unavailable. Please try again later.";

    case "http":
      return "We couldn't complete your request. Please try again later.";
  }
}
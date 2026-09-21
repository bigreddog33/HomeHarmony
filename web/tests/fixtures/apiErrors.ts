import { ApiError } from "@/services/apiClient";

// Expected user-facing messages are independent of the production message mapper.
export const sharedApiErrors = [
  {
    name: "timeout", error: new ApiError("timeout"),
    message: "The server is taking too long to respond. Please try again.",
  },
  {
    name: "network unavailable", error: new ApiError("network"),
    message: "We couldn't connect right now. Check your internet connection and try again.",
  },
  {
    name: "service unavailable", error: new ApiError("http", 503),
    message: "We couldn't complete your request. Please try again later.",
  },
  {
    name: "missing configuration", error: new ApiError("configuration"),
    message: "The service is currently unavailable. Please try again later.",
  },
  {
    name: "unexpected failure", error: new Error("Internal details must stay hidden"),
    message: "An unexpected error occurred. Please try again.",
  },
];

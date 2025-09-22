import axios from "axios";
import { getAccessToken } from "../../auth/services/authService";
import { logger } from "../utils/logger";
import { toastService } from "../../shared/utils/toastService";
import type { ApiResponse } from "./types";
import { loadAppConfig } from "../../config/appConfig";

const apiClient = axios.create({
  baseURL: loadAppConfig().API_BASE_URL,
  headers: { "Content-Type": "application/json" },
  timeout: 15000,
});

apiClient.interceptors.request.use(
  async (config) => {
    const token = await getAccessToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    logger.error("Request setup error:", error);
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => {
    const api = response.data as ApiResponse<any> | any;

    if (api && typeof api === "object" && "success" in api && "data" in api) {
      if (!api.success) {
        const mapped = {
          status: response.status,
          message: api.message || "Something went wrong",
          errorCode: api.errorCode,
          data: api.data,
        };
        logger.warn("API returned success=false", mapped);
        toastService.error(mapped.message);
        return Promise.reject(mapped);
      }
      response.data = api.data;
    }

    return response;
  },
  (error) => {
    const status = error?.response?.status ?? 0;
    const errBody = error?.response?.data;
    const serverMsg = errBody?.message || error?.message || "Unexpected error";

    let friendly = serverMsg;
    if (status === 0) friendly = "Network error. Check your connection.";
    else if (status === 400) friendly = "Bad request.";
    else if (status === 401) friendly = "Your session expired. Please sign in.";
    else if (status === 403) friendly = "You don't have permission to do that.";
    else if (status === 404) friendly = "Not found.";
    else if (status >= 500) friendly = "Server error. Please try again later.";

    toastService.error(friendly);
    logger.error(`[HTTP ${status}]`, errBody || error);

    // if (status === 401) window.location.href = "/login";
    // if (status === 403) window.location.href = "/forbidden";

    return Promise.reject({
      status,
      message: friendly,
      data: errBody,
      raw: error,
    });
  }
);

export default apiClient;

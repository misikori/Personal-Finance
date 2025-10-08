import axios, { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from "axios";
import { logger } from "../../shared/utils/logger";
import { toastService } from "../../shared/utils/toastService";
import { ApiResponse } from "../../shared/api/types";
import { authStore } from "../../auth/store/authStore";
import { ensureAccessToken } from "./tokenProvider";

let isRefreshing = false;
let waiters: Array<() => void> = [];
const queue = () => new Promise<void>(resolve => waiters.push(resolve));
const flush = () => { waiters.forEach(fn => fn()); waiters = []; };

export function createApiClient(baseURL: string): AxiosInstance {
  const client = axios.create({
    baseURL,
    headers: { "Content-Type": "application/json" },
    timeout: 15000,
  });

  client.interceptors.request.use(
    async (config) => {
      if (!config.headers?.["X-Skip-Auth"]) {
        const token = await ensureAccessToken();
        if (token && config.headers) config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => {
      logger.error("Request setup error:", error);
      return Promise.reject(error);
    }
  );

  client.interceptors.response.use(
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
        response.data = api.data; // unwrap
      }
      return response;
    },
    async (error: AxiosError) => {
      const status = error?.response?.status ?? 0;
      const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean });

      // skip 401 retry for requests that opted out
      const skipAuth = !!original?.headers?.["X-Skip-Auth"];

      if (status === 401 && original && !original._retry && !skipAuth) {
        original._retry = true;

        try {
          if (isRefreshing) {
            await queue();
          } else {
            isRefreshing = true;
            try {
              const token = await ensureAccessToken();
              if (!token) {
                authStore.clear();
                throw error;
              }
            } finally {
              isRefreshing = false;
              flush();
            }
          }
          return client(original);
        } catch (e) {
          authStore.clear();
          return Promise.reject(e);
        }
      }

      const errBody: any = (error as any)?.response?.data;
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

      return Promise.reject({ status, message: friendly, data: errBody, raw: error });
    }
  );

  return client;
}

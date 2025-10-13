import type { AxiosInstance } from "axios";
import { logger } from "../../shared/utils/logger";

export class BaseService<T = unknown> {
  constructor(protected client: AxiosInstance, protected baseUrl = "") {}

  protected buildUrl(endpoint: string): string {
    if (!this.baseUrl) return endpoint;
    return `${this.baseUrl.replace(/\/$/, "")}/${endpoint.replace(/^\//, "")}`;
  }

  private async safeRequest<TResult>(fn: () => Promise<TResult>): Promise<TResult> {
    try {
      return await fn();
    } catch (error: any) {
      const status = error?.status ?? error?.response?.status ?? 0;
      const errData = error?.data ?? error?.response?.data;
      logger.error(`[HTTP ERROR] ${status}:`, errData);
      throw {
        status,
        message: error?.message || errData?.message || "Unexpected error",
        data: errData,
      };
    }
  }

  async get<TResult = T>(url: string, params?: any): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.get<TResult>(this.buildUrl(url), { params });
      return res.data as TResult;
    });
  }
  async post<TResult = T>(url: string, body?: any): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.post<TResult>(this.buildUrl(url), body);
      return res.data as TResult;
    });
  }
  async postForm<TResult = T>(url: string, formData: FormData): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.post<TResult>(this.buildUrl(url), formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      return res.data as TResult;
    });
  }
  async put<TResult = T>(url: string, body: any): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.put<TResult>(this.buildUrl(url), body);
      return res.data as TResult;
    });
  }
  async patch<TResult = T>(url: string, body: any): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.patch<TResult>(this.buildUrl(url), body);
      return res.data as TResult;
    });
  }
  async delete<TResult = T>(url: string): Promise<TResult> {
    return this.safeRequest(async () => {
      const res = await this.client.delete<TResult>(this.buildUrl(url));
      return res.data as TResult;
    });
  }
}

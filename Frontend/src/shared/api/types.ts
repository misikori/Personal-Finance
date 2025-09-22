export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  errorCode?: number | string;
  data: T;
  metadata?: any;
}
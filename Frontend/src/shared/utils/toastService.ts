type ToastLevel = "success" | "info" | "warning" | "error";

export const toastService = {
  show: (msg: string, level: ToastLevel = "info") => {
    console.log(`[TOAST:${level.toUpperCase()}]`, msg);
  },
  success: (msg: string) => toastService.show(msg, "success"),
  info:    (msg: string) => toastService.show(msg, "info"),
  warn:    (msg: string) => toastService.show(msg, "warning"),
  error:   (msg: string) => toastService.show(msg, "error"),
};

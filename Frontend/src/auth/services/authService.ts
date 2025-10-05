import { apiFetch } from "../../core/apiClient";
import { authStore, getCurrentUser, setTokensAndUserFromToken } from "../store/authStore";

export type LoginRequest = { userName: string; password: string };
export type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  user: { id: string; email: string; roles: string[] };
};
export type SignupRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  userName: string;
  phoneNumber: string;
};

const buildSignupPayload = (req: SignupRequest) => ({
  firstName: req.firstName,
  lastName: req.lastName,
  userName: req.userName,
  email: req.email,
  phoneNumber: req.phoneNumber,
  password: req.password,
});

export async function login(req: LoginRequest) {
  const data = await apiFetch<LoginResponse>("/api/v1/Authentication/Login", {
    method: "POST",
    body: req,
  });
  
  authStore.setTokens(data.accessToken, data.refreshToken ?? null);
  if (data.user) {
    authStore.setUser(data.user);
  } else {
    setTokensAndUserFromToken(data.accessToken, data.refreshToken ?? null);
  }
  return data.user ?? getCurrentUser();
}

export async function signup(req: SignupRequest) {
  const payload = buildSignupPayload(req);
  const data = await apiFetch<LoginResponse | undefined | string>("/api/v1/Authentication/RegisterUser", {
    method: "POST",
    body: payload,
  });

  // Some APIs return nothing on successful signup → just return
  if (!data) return;

  // If it returns tokens, store them
  if (typeof data === "object" && "accessToken" in data) {
    const d = data as LoginResponse;
    authStore.setTokens(d.accessToken, d.refreshToken ?? null);
    authStore.setUser(d.user);
  }
}



export async function logout() {
  try {
    await apiFetch<void>("/Authentication/logout", { method: "POST" });
  } finally {
    authStore.clear();
  }
}

export async function refresh() {
  const data = await apiFetch<{ accessToken: string; refreshToken?: string }>(
    "/api/v1/Authentication/refresh",
    { method: "POST", body: { refreshToken: authStore.refreshToken } }
  );
  authStore.setTokens(data.accessToken, data.refreshToken ?? authStore.refreshToken);
}

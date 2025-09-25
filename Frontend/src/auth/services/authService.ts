import { apiFetch } from "../../core/apiClient";
import { authStore } from "../store/authStore";

export type LoginRequest = { email: string; password: string };
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

export async function login(req: LoginRequest) {
  const data = await apiFetch<LoginResponse>("/api/v1/Authentication/login", { method: "POST", body: req });
  authStore.setTokens(data.accessToken, data.refreshToken);
  authStore.setUser(data.user);
  return data.user;
}

export async function signup(req: SignupRequest) {
  // backend expects { newUser: NewUserDto, password: string }
  const data = await apiFetch<any>("/api/v1/Authentication/RegisterUser", {
    method: "POST",
    body: {
      firstName:   req.firstName,
      lastName:    req.lastName,
      userName:    req.userName,
      password:    req.password,
      email:       req.email,
      phoneNumber: req.phoneNumber,
    },
  });

  // if your API returns tokens+user on signup, you can set them here:
  if (data?.accessToken) authStore.setTokens(data.accessToken, data.refreshToken ?? null);
  if (data?.user) authStore.setUser(data.user);

  return data;
}

export async function me() {
  const user = await apiFetch<LoginResponse["user"]>("/api/v1/");
  authStore.setUser(user);
  return user;
}

export async function logout() {
  try {
    await apiFetch<void>("/Authentication/logout", { method: "POST" });
  } finally {
    authStore.clear();
  }
}

export async function refresh() {
  // Usually called only by apiClient; exposed if you ever want manual refresh.
  const data = await apiFetch<{ accessToken: string; refreshToken?: string }>("/Authentication/refresh", {
    method: "POST",
    body: { refreshToken: authStore.refreshToken }
  });
  authStore.setTokens(data.accessToken, data.refreshToken ?? authStore.refreshToken);
}

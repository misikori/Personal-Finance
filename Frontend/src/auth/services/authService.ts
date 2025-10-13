import { authApi } from "../../core/http/apiClients";
import { refreshTokensOnce } from "../../core/http/tokenProvider";
import { authStore, getCurrentUser, setTokensAndUserFromToken } from "../store/authStore";

export type LoginRequest = { userName: string; password: string };
export type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  user: { id: string; email: string; username:string; roles: string[] };
};
export type SignupRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  userName: string;
  phoneNumber: string;
};

export type UserInfoResponse = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string;

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
  const { data } = await authApi.post<LoginResponse>("/api/v1/Authentication/Login", req, {
    headers: { "X-Skip-Auth": true },
  });

  authStore.setTokens(data.accessToken, data.refreshToken ?? null);
  if (data.user) {
    authStore.setUser(data.user);
  } else {

    setTokensAndUserFromToken(data.accessToken, data.refreshToken ?? null);
  }

  const userName = req.userName;

    try {
    const profile = await fetchUserInfoByUsername(userName);
    const existing = getCurrentUser();
    authStore.setUser({
      ...existing,
      ...profile,
      roles: existing?.roles ?? [],
    });
  } catch (e) {
  }
  return data.user ?? getCurrentUser();
}

export async function signup(req: SignupRequest) {
  const payload = buildSignupPayload(req);

  const { data } = await authApi.post<LoginResponse | string | undefined>(
    "/api/v1/Authentication/RegisterUser",
    payload,
    { headers: { "X-Skip-Auth": true } }
  );

  if (!data) return;

  if (typeof data === "object" && "accessToken" in data) {
    const d = data as LoginResponse;
    authStore.setTokens(d.accessToken, d.refreshToken ?? null);
    if (d.user) authStore.setUser(d.user);
  }
}


export async function fetchUserInfoByUsername(userName: string) {
  const { data } = await authApi.get<UserInfoResponse>(`/api/v1/User/${encodeURIComponent(userName)}`);
  return data;
}

export async function logout() {
  try {
    await authApi.post<void>("/Authentication/logout", undefined, {
      headers: { "X-Skip-Auth": true },
    });
  } finally {
    authStore.clear();
  }
}

export async function refresh() {
  await refreshTokensOnce();
}

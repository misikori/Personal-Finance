import { authStore } from "../store/authStore";

export async function mockLogin(roles: string[] = ["Users"], email = "demo@demo.test") {
  await new Promise(r => setTimeout(r, 200));
  const token = "fake-jwt-" + Math.random().toString(36).slice(2);
  authStore.setUser({ id: crypto.randomUUID?.() ?? Date.now().toString(), email, roles });
  authStore.setTokens(token, null);
  return { token, roles };
}

export async function mockLogout() {
  await new Promise(r => setTimeout(r, 100));
  authStore.clear();
}

export async function getAccessToken(): Promise<string | null> {
  return authStore.accessToken;
}

export function getCurrentUser() {
  return authStore.user;
}

export function isAuthenticated(): boolean {
  return !!authStore.accessToken;
}

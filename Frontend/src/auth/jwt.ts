export type JwtPayload = Record<string, any>;

function base64UrlDecode(input: string) {

  input = input.replace(/-/g, "+").replace(/_/g, "/");
  const pad = input.length % 4;
  if (pad) input += "=".repeat(4 - pad);
  const decoded = atob(input);
  try {
    return decodeURIComponent(
      decoded
        .split("")
        .map(c => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
  } catch {
    return decoded;
  }
}

export function decodeJwt(token: string | null | undefined): JwtPayload | null {
  if (!token) return null;
  const parts = token.split(".");
  if (parts.length !== 3) return null;
  const payload = base64UrlDecode(parts[1]);
  try { return JSON.parse(payload); } catch { return null; }
}

export function getTokenExpiry(token?: string | null): number | null {
  const p = decodeJwt(token);
  return p?.exp ?? null;
}

export function isTokenExpired(token?: string | null): boolean {
  const exp = getTokenExpiry(token);
  if (!exp) return false;
  const now = Math.floor(Date.now() / 1000);
  return exp <= now;
}

export function getRolesFromPayload(p?: JwtPayload | null): string[] {
  if (!p) return [];
  const candidates = [
    "role",
    "roles",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  ];
  for (const key of candidates) {
    const val = p[key];
    if (!val) continue;
    if (Array.isArray(val)) return val.map(String);
    return [String(val)];
  }
  return [];
}

export function getEmailFromPayload(p?: JwtPayload | null): string | null {
  return (
    p?.email ??
    p?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ??
    null
  );
}

export function getNameFromPayload(p?: JwtPayload | null): string | null {
  return (
    p?.name ??
    p?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ??
    null
  );
}

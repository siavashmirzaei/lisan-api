const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? '';

type TokenGetter = () => Promise<string | null>;
let _getToken: TokenGetter | null = null;

export function registerTokenGetter(fn: TokenGetter): void {
  _getToken = fn;
}

export async function apiFetch(
  path: string,
  init: RequestInit = {},
): Promise<Response> {
  const token = _getToken ? await _getToken() : null;
  const headers = new Headers(init.headers as HeadersInit);
  headers.set('Content-Type', 'application/json');
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  return fetch(`${BASE_URL}${path}`, { ...init, headers });
}

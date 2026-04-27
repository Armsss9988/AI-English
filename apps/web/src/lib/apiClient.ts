const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://127.0.0.1:5237";

function getErrorMessage(status: number, bodyText: string): string {
  if (!bodyText) {
    return `API error: ${status}`;
  }

  try {
    const errorBody = JSON.parse(bodyText) as {
      message?: unknown;
      detail?: unknown;
      title?: unknown;
    };

    if (typeof errorBody.message === "string") return errorBody.message;
    if (typeof errorBody.detail === "string") return errorBody.detail;
    if (typeof errorBody.title === "string") return errorBody.title;
  } catch {
    return bodyText;
  }

  return `API error: ${status}`;
}

export async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${BASE_URL}${path}`;

  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      "X-User-Id": "00000000-0000-0000-0000-000000000001",
      "X-User-Role": "Admin",
      ...options.headers,
    },
  });

  const responseText = await response.text();

  if (!response.ok) {
    throw new Error(getErrorMessage(response.status, responseText));
  }

  // Some endpoints return an empty body.
  if (!responseText) {
    return {} as T;
  }

  return JSON.parse(responseText) as T;
}

export const apiClient = {
  get: <T>(path: string, options?: RequestInit) =>
    request<T>(path, { ...options, method: "GET" }),
  post: <T>(path: string, body: unknown, options?: RequestInit) =>
    request<T>(path, {
      ...options,
      method: "POST",
      body: JSON.stringify(body),
    }),
  put: <T>(path: string, body: unknown, options?: RequestInit) =>
    request<T>(path, { ...options, method: "PUT", body: JSON.stringify(body) }),
  delete: <T>(path: string, options?: RequestInit) =>
    request<T>(path, { ...options, method: "DELETE" }),
};

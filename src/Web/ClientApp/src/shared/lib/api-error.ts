// The backend's FluentValidation pipeline returns a standard ASP.NET Core ValidationProblemDetails
// body (`{ errors: { FieldName: ["message"] } }`) on a 400. NSwag's Fetch template throws the raw
// response as a SwaggerException with the body still as a JSON string on `.response`, so callers
// need to parse it themselves to show field-level errors.
export function getValidationErrors(error: unknown): Record<string, string[]> | undefined {
  if (typeof error !== "object" || error === null || !("response" in error)) {
    return undefined;
  }

  const response = (error as { response?: unknown }).response;
  if (typeof response !== "string") {
    return undefined;
  }

  try {
    const parsed = JSON.parse(response);
    return typeof parsed?.errors === "object" ? parsed.errors : undefined;
  } catch {
    return undefined;
  }
}

export function getFieldError(error: unknown, field: string): string | undefined {
  return getValidationErrors(error)?.[field]?.[0];
}

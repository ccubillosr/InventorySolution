namespace InventoryBackend.Middleware;

/// <summary>Adds OWASP-recommended security response headers to every response.</summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // OWASP A03: Prevent MIME-type sniffing (browser should respect Content-Type)
        headers["X-Content-Type-Options"] = "nosniff";

        // OWASP A04: Prevent clickjacking attacks
        headers["X-Frame-Options"] = "DENY";

        // OWASP A07: Control referrer information leakage
        headers["Referrer-Policy"] = "no-referrer";

        // OWASP A07: Disable legacy XSS filter (modern browsers use CSP instead)
        headers["X-XSS-Protection"] = "0";

        // OWASP A01: Restrict access to browser features (geolocation, camera, microphone)
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=()";

        // OWASP A01: Content Security Policy to prevent XSS and data exfiltration
        headers["Content-Security-Policy"] = "default-src 'none'; style-src 'self'; script-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

        // OWASP A02: Enforce HTTPS only (Strict-Transport-Security)
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

        // Remove fingerprinting headers
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");

        // Prevent cache of sensitive data
        headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
        headers["Pragma"] = "no-cache";
        headers["Expires"] = "0";

        await _next(context);
    }
}

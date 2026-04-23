using System.Net.Http.Headers;

namespace Wms.Theme.Web.Services.Authen;

public class AuthHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            if (context.Request.Cookies.TryGetValue("access_token", out var accessToken) && !string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                // Token missing: 
                // Depending on requirements, we could redirect here or just let the request fail with 401.
                // Since this is a Service call (API), we can't easily redirect the *Browser* from here directly 
                // if this is an AJAX call, but we can if it's a Server-Side Page load.
                // For now, we proceed. If the API requires auth, it will return 401, 
                // and the calling Service/Controller should handle it.
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

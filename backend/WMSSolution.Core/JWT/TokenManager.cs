using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WMSSolution.Core.Utility;
using WMSSolution.Shared;

namespace WMSSolution.Core.JWT;

/// <summary>
/// token manager
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="tokenSettings">token setting s</param>
/// <param name="accessor">Inject IHttpContextAccessor</param>
public class TokenManager(IOptions<TokenSettings> tokenSettings
        , IHttpContextAccessor accessor) : ITokenManager
{
    private readonly IOptions<TokenSettings> _tokenSettings = tokenSettings;//token setting
    private readonly IHttpContextAccessor _accessor = accessor; // Inject IHttpContextAccessor

    /// <summary>
    /// Method of refreshing token
    /// </summary>
    /// <returns></returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
    /// <summary>
    /// Method of generating AccessToken
    /// </summary>
    /// <param name="userClaims">Custom information</param>
    /// <param name="integrationApp"></param>
    /// <returns>(token, valid minutes)</returns>
    public (string token, int expire) GenerateToken(CurrentUser userClaims, string integrationApp = "")
    {
        //string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        //                                                                     issuer: _tokenSettings.Value.Issuer,
        //                                                                     audience: _tokenSettings.Value.Audience,
        //                                                                     claims: SetClaims(userClaims),
        //                                                                     expires: DateTime.UtcNow.AddMinutes(_tokenSettings.Value.ExpireMinute),
        //                                                                     signingCredentials: new SigningCredentials(
        //                                                                                                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalConsts.SigningKey)),
        //                                                                                                                SecurityAlgorithms.HmacSha256)
        var signingKey = _tokenSettings.Value.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
            throw new ArgumentException("SigningKey phải có ít nhất 32 ký tự cho HS256.");

        string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: _tokenSettings.Value.Issuer,
            audience: _tokenSettings.Value.Audience,
            claims: SetClaims(userClaims, integrationApp),
            expires: DateTime.UtcNow.AddMinutes(_tokenSettings.Value.ExpireMinute),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256)
        ));
        //    ));

        return (token, _tokenSettings.Value.ExpireMinute);
    }
    /// <summary>
    /// Get the current user information in the token
    /// </summary>
    /// <returns></returns>
    public CurrentUser GetCurrentUser()
    {
        if (_accessor.HttpContext == null)
        {
            return new CurrentUser();
        }
        var token = _accessor.HttpContext.Request.Headers.Authorization.ObjToString();
        if (!token.StartsWith("Bearer"))
        {
            return new CurrentUser();
        }
        token = token.Replace("Bearer ", "");
        if (token.Length > 0)
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token,
                                                                    new TokenValidationParameters
                                                                    {
                                                                        ValidateAudience = false,
                                                                        ValidateIssuer = false,
                                                                        ValidateIssuerSigningKey = true,
                                                                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalConsts.SigningKey)),
                                                                        ValidateLifetime = false
                                                                    },
                                                                    out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var user = JsonHelper.DeserializeObject<CurrentUser>(principal.Claims.First(claim => claim.Type == ClaimValueTypes.Json).Value);
                if (user != null)
                {
                    return user;
                }
                else
                {
                    return new CurrentUser();
                }
            }
            return new CurrentUser();
        }
        else
        {
            return new CurrentUser();
        }

    }

    /// <summary>
    /// Get the current user information in the token
    /// </summary>
    /// <returns></returns>
    public CurrentUser GetCurrentUser(string token)
    {
        if (token.Length > 0)
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token,
                                                                    new TokenValidationParameters
                                                                    {
                                                                        ValidateAudience = false,
                                                                        ValidateIssuer = false,
                                                                        ValidateIssuerSigningKey = true,
                                                                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalConsts.SigningKey)),
                                                                        ValidateLifetime = false
                                                                    },
                                                                    out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return new CurrentUser();
            }

            var user = JsonHelper.DeserializeObject<CurrentUser>(principal.Claims.First(claim => claim.Type == ClaimValueTypes.Json).Value);
            if (user != null)
            {
                return user;
            }
            else
            {
                return new CurrentUser();
            }
        }
        else
        {
            return new CurrentUser();
        }

    }
    /// <summary>
    /// Method of refreshing token
    /// </summary>
    /// <returns></returns>
    public int GetRefreshTokenExpireMinute()
    {
        return _tokenSettings.Value.ExpireMinute + 1;
    }

    /// <summary>
    /// Setting Custom Information
    /// </summary>
    /// <param name="userClaims">Custom Information</param>
    /// <param name="integrationApp"></param>
    /// <returns></returns>
    private static IEnumerable<Claim> SetClaims(CurrentUser userClaims, string integrationApp)
    {
        string strApp = string.IsNullOrEmpty(integrationApp) ? SystemDefine.AppWMS : SystemDefine.Integration;

        return
        [
            new Claim(ClaimTypes.Sid, $"{userClaims.user_id:X2}-{Guid.NewGuid()}-{userClaims.tenant_id:X2}"),
            new Claim(ClaimTypes.Version, SystemDefine.SystemVersion),
            new Claim(ClaimTypes.Role, string.IsNullOrEmpty(integrationApp) ? userClaims.user_role : SystemDefine.Integration),
            new Claim(SystemDefine.CustomApp, $"{strApp}"),
            new Claim(ClaimValueTypes.Json,JsonHelper.SerializeObject(userClaims), ClaimValueTypes.Json)
        ];
    }
}

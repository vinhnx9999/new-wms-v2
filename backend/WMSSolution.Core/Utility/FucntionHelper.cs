using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Utility;

namespace WMSSolution.Core;

/// <summary>
/// Function helper
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="dBContext"></param>
/// <param name="accessor"></param>
/// <param name="tokenSettings"></param>
public class FunctionHelper(SqlDBContext dBContext, IHttpContextAccessor accessor, IOptions<TokenSettings> tokenSettings)
{
    private readonly SqlDBContext _dbContext = dBContext;
    private readonly IHttpContextAccessor _accessor = accessor;
    private readonly TokenSettings _tokenSettings = tokenSettings.Value;

    /// <summary>
    /// Update the token validation logic to handle missing 'kid' in the token.
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
            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SigningKey)),
                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            // Luôn trả về khóa tĩnh, bỏ qua kid
                            return [new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SigningKey))];
                        },
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        RequireSignedTokens = false
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
            catch (SecurityTokenInvalidSignatureException ex)
            {
                // Log the exception and return a default CurrentUser
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return new CurrentUser();
            }
        }
        else
        {
            return new CurrentUser();
        }
    }

    /// <summary>
    /// Get document number from serial table
    /// </summary>
    /// <param name="table_name">Table name</param>
    /// <param name="prefix_char">Prefix</param>
    /// <param name="reset_rule">Reset rule</param>
    /// <returns></returns>
    public async Task<string> GetFormNoAsync(string table_name, string prefix_char = "", ResetRule reset_rule = ResetRule.Day)
    {
        var current_user = GetCurrentUser();
        var nums = await GetFormNoListAsync(table_name, 1, current_user.tenant_id, prefix_char, reset_rule);
        if (nums == null)
        {
            return "";
        }
        else
        {
            return nums[0];
        }
    }

    /// <summary>
    /// Get document numbers from serial table
    /// </summary>
    /// <param name="table_name">Table name</param>
    /// <param name="tenant_id">Tenant id</param>
    /// <param name="prefix_char">Prefix</param>
    /// <param name="Qty">Quantity of numbers</param>
    /// <param name="reset_rule">Reset rule</param>
    /// <returns></returns>
    public async Task<List<string>> GetFormNoListAsync(string table_name, int Qty = 1, long tenant_id = 1, string prefix_char = "", ResetRule reset_rule = ResetRule.Day)
    {
        List<string> nums = [];
        string _reset_rule = "yyyyMMdd";
        if (reset_rule == ResetRule.Year) _reset_rule = "yyyy";
        else if (reset_rule == ResetRule.Month) _reset_rule = "yyyyMM";

        var dbSet = _dbContext.Set<GlobalUniqueSerialEntity>();
        // GlobalUniqueSerialEntity? entity = await dbSet.FirstOrDefaultAsync(t => t.table_name == table_name && t.reset_rule == _reset_rule);
        GlobalUniqueSerialEntity? entity = await dbSet.FirstOrDefaultAsync(t => t.table_name == table_name && t.prefix_char == prefix_char && t.reset_rule == _reset_rule);
        if (entity == null)
        {
            for (int index = 1; index <= Qty; index++)
            {
                nums.Add($"{prefix_char}{DateTime.UtcNow.ToString(_reset_rule)}-{index.ToString().PadLeft(4, '0')}");
            }
            entity = new GlobalUniqueSerialEntity
            {
                table_name = table_name,
                prefix_char = prefix_char,
                reset_rule = _reset_rule,
                current_no = Qty + 1,
                last_update_time = DateTime.UtcNow,
                tenant_id = tenant_id
            };
            dbSet.Add(entity);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            int current_no = entity.current_no;
            if (!DateTime.UtcNow.ToString(_reset_rule).Equals(entity.last_update_time.ToString(_reset_rule)))
            {
                current_no = 1;
            }
            for (int index = 1; index <= Qty; index++)
            {
                nums.Add($"{prefix_char}{DateTime.UtcNow.ToString(_reset_rule)}-{current_no.ToString().PadLeft(4, '0')}");
                current_no++;
            }
            entity.current_no = current_no;
            entity.last_update_time = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        return nums;
    }

    /// <summary>
    /// Reset rule
    /// </summary>
    public enum ResetRule
    {
        /// <summary>
        /// Year
        /// </summary>
        Year,

        /// <summary>
        /// Month
        /// </summary>
        Month,

        /// <summary>
        /// Day
        /// </summary>
        Day
    }

    /// <summary>
    /// Create ASN Batch No 
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static async Task<string> CreateAsnBatchNoTimestamp(string prefix = "IMP")
        => $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{new Random().Next(100, 999)}".ToUpper();
}
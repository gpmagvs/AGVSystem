using AGVSystemCommonNet6.User;
using System.Security.Claims;

namespace AGVSystem.Service
{
    public class UserValidationService
    {
        public bool UserValidation(HttpContext _httpContex)
        {
            ClaimsIdentity identity = _httpContex.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userRole = claims.FirstOrDefault(c => c.Type == "Role")?.Value;

                if (userRole == ERole.VISITOR.ToString())
                    return false;

                return true;
            }
            else
                return false;
        }
    }
}

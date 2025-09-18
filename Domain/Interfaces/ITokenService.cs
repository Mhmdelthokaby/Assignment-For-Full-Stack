using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
        Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

}

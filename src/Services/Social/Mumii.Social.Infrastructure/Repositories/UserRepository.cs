using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Mumii.Shared.Common.DTOs;
using Mumii.Social.Domain.Interfaces;

namespace Mumii.Social.Infrastructure.Repositories
{
    /// <summary>
    /// UserRepository dạng API Adapter, call API Gateway -> Auth Service
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly HttpClient _httpClient;

        public UserRepository(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            // Dùng named client đã cấu hình BaseAddress tại Program.cs
            _httpClient = httpClientFactory.CreateClient("gateway");
        }

        public async Task<List<UserDto>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
        {
            // API giả định: /auth/users/ids?ids=1,2,3
            var idsStr = string.Join(",", userIds);
            var url = $"/api/auth/users/ids?ids={idsStr}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<UserDto>>(cancellationToken: cancellationToken);
            return data ?? new List<UserDto>();
        }

        public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/auth/users/{id}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);
        }
    }
}

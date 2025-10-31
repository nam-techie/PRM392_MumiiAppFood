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
    /// RestaurantRepository dạng API Adapter, call API Gateway -> Discovery Service
    /// </summary>
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly HttpClient _httpClient;

        public RestaurantRepository(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            // Dùng named client đã cấu hình BaseAddress tại Program.cs
            _httpClient = httpClientFactory.CreateClient("gateway");
        }

        public async Task<List<RestaurantDto>> GetByIdsAsync(IEnumerable<int> restaurantIds, CancellationToken cancellationToken = default)
        {
            // API giả định: /discovery/restaurants/ids?ids=1,2,3
            var idsStr = string.Join(",", restaurantIds);
            var url = $"/api/discovery/restaurants/ids?ids={idsStr}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<RestaurantDto>>(cancellationToken: cancellationToken);
            return data ?? new List<RestaurantDto>();
        }

        public async Task<RestaurantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/discovery/restaurants/{id}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RestaurantDto>(cancellationToken: cancellationToken);
        }
    }
}

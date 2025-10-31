using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
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
            // Hiện Discovery chưa có endpoint batch /api/restaurants/ids,
            // gọi tuần tự/parallel từng id và tổng hợp kết quả, tránh làm fail toàn bộ nếu 404.
            var ids = restaurantIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0) return new List<RestaurantDto>();

            var tasks = ids.Select(async id =>
            {
                try
                {
                    var resp = await _httpClient.GetAsync($"/api/restaurants/{id}", cancellationToken);
                    if (!resp.IsSuccessStatusCode) return null;
                    return await resp.Content.ReadFromJsonAsync<ApiResponse<RestaurantDto>>(cancellationToken: cancellationToken)?.ContinueWith(t => t.Result?.Data, cancellationToken).Result;
                }
                catch
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList()!;
        }

        public async Task<RestaurantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = $"/api/restaurants/{id}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RestaurantDto>(cancellationToken: cancellationToken);
        }
    }
}

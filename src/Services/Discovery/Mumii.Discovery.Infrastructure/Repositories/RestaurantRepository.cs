using Microsoft.EntityFrameworkCore;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Discovery.Infrastructure.Data;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Discovery.Infrastructure.Repositories;

/// <summary>
/// Implementation của IRestaurantRepository
/// </summary>
public class RestaurantRepository : IRestaurantRepository
{
    private readonly DiscoveryDbContext _context;

    public RestaurantRepository(DiscoveryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tìm nhà hàng theo ID
    /// </summary>
    public async Task<Restaurant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách nhà hàng có phân trang
    /// </summary>
    public async Task<PagedResult<Restaurant>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Restaurants
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Restaurant>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Tìm kiếm nhà hàng
    /// </summary>
    public async Task<PagedResult<Restaurant>> SearchAsync(
        SearchRestaurantsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.Restaurants
            .Where(r => !r.IsDeleted);

        // Tìm kiếm theo từ khóa
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var searchTerm = query.Query.ToLower();
            dbQuery = dbQuery.Where(r => 
                r.Name.ToLower().Contains(searchTerm) ||
                r.Address.ToLower().Contains(searchTerm) ||
                (r.Description != null && r.Description.ToLower().Contains(searchTerm))
            );
        }

        // Lọc theo vùng
        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            dbQuery = dbQuery.Where(r => r.Region == query.Region);
        }

        // Lọc theo giá
        if (query.MinPrice.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.AvgPrice >= query.MinPrice);
        }

        if (query.MaxPrice.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.AvgPrice <= query.MaxPrice);
        }

        // Lọc theo rating
        if (query.MinRating.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.Rating >= query.MinRating);
        }

        // Lọc theo vị trí nếu có
        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            // Sử dụng Haversine formula để tính khoảng cách
            // Đây là approximation cho MySQL, trong thực tế nên dùng spatial functions
            var lat = (double)query.Latitude.Value;
            var lng = (double)query.Longitude.Value;
            var radius = (double)query.RadiusKm.Value;

            dbQuery = dbQuery.Where(r => 
                r.Latitude.HasValue && r.Longitude.HasValue &&
                (6371 * Math.Acos(
                    Math.Cos(Math.PI * lat / 180) * 
                    Math.Cos(Math.PI * (double)r.Latitude / 180) * 
                    Math.Cos(Math.PI * (double)r.Longitude / 180 - Math.PI * lng / 180) + 
                    Math.Sin(Math.PI * lat / 180) * 
                    Math.Sin(Math.PI * (double)r.Latitude / 180)
                )) <= radius
            );
        }

        // Sắp xếp
        dbQuery = dbQuery.OrderByDescending(r => r.Rating)
                        .ThenByDescending(r => r.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        
        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new PagedResult<Restaurant>(items, totalCount, query.Page, query.PageSize, totalPages);
    }

    /// <summary>
    /// Tìm nhà hàng gần vị trí
    /// </summary>
    public async Task<List<Restaurant>> GetNearbyAsync(
        NearbyRestaurantsQuery query,
        CancellationToken cancellationToken = default)
    {
        var lat = (double)query.Latitude;
        var lng = (double)query.Longitude;
        var radius = (double)query.RadiusKm;

        var restaurants = await _context.Restaurants
            .Where(r => !r.IsDeleted && 
                       r.Latitude.HasValue && 
                       r.Longitude.HasValue)
            .ToListAsync(cancellationToken);

        // Tính khoảng cách và filter trong memory để chính xác hơn
        var nearbyRestaurants = restaurants
            .Select(r => new { 
                Restaurant = r, 
                Distance = r.CalculateDistanceTo(query.Latitude, query.Longitude) 
            })
            .Where(x => x.Distance.HasValue && x.Distance <= radius)
            .OrderBy(x => x.Distance)
            .Take(query.Limit)
            .Select(x => x.Restaurant)
            .ToList();

        return nearbyRestaurants;
    }

    /// <summary>
    /// Thêm nhà hàng mới
    /// </summary>
    public async Task<Restaurant> AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        await _context.Restaurants.AddAsync(restaurant, cancellationToken);
        return restaurant;
    }

    /// <summary>
    /// Cập nhật nhà hàng
    /// </summary>
    public async Task<Restaurant> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        _context.Restaurants.Update(restaurant);
        return await Task.FromResult(restaurant);
    }

    /// <summary>
    /// Xóa nhà hàng (soft delete)
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var restaurant = await GetByIdAsync(id, cancellationToken);
        if (restaurant != null)
        {
            restaurant.Delete();
            _context.Restaurants.Update(restaurant);
        }
    }

    /// <summary>
    /// Kiểm tra nhà hàng có tồn tại không
    /// </summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .AnyAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

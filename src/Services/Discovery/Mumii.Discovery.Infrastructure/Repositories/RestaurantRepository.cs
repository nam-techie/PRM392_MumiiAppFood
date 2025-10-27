using MongoDB.Driver;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Mumii.Discovery.Infrastructure.Repositories;

/// <summary>
/// Implementation của IRestaurantRepository
/// </summary>
public class RestaurantRepository : IRestaurantRepository
{
    private readonly IMongoCollection<Restaurant> _restaurants;

    public RestaurantRepository(IMongoDatabase database)
    {
        _restaurants = database.GetCollection<Restaurant>("restaurants");
    }

    /// <summary>
    /// Tìm nhà hàng theo ID
    /// </summary>
    public async Task<Restaurant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Restaurant>.Filter.In(r => r.Id, ids);
        return await _restaurants.Find(filter).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách nhà hàng có phân trang
    /// </summary>
    public async Task<PagedResult<Restaurant>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var find = _restaurants.Find(_ => true);
        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);
        var items = await find.SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Restaurant>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<PagedResult<Restaurant>> GetPagedByStatusAsync(
    int page, int pageSize, string? status, CancellationToken cancellationToken = default)
    {
        var filter = string.IsNullOrWhiteSpace(status) 
            ? Builders<Restaurant>.Filter.Empty 
            : Builders<Restaurant>.Filter.Eq(r => r.Status, status);

        var find = _restaurants.Find(filter);
        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);
        var items = await find.SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
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
        var filters = new List<FilterDefinition<Restaurant>>();
        var builder = Builders<Restaurant>.Filter;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            filters.Add(builder.Eq(r => r.Status, query.Status));
        }

        // Tìm kiếm theo từ khóa
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var regex = new BsonRegularExpression(query.Query, "i");
            filters.Add(builder.Or(
                builder.Regex(r => r.Name, regex),
                builder.Regex(r => r.Address, regex),
                builder.Regex(r => r.Description, regex)
            ));
        }

        // Lọc theo vùng
        // Region removed in new schema

        // Lọc theo giá
        if (query.MinPrice.HasValue) filters.Add(builder.Gte(r => r.AvgPrice, (double)query.MinPrice.Value));

        if (query.MaxPrice.HasValue) filters.Add(builder.Lte(r => r.AvgPrice, (double)query.MaxPrice.Value));

        // Lọc theo rating
        if (query.MinRating.HasValue) filters.Add(builder.Gte(r => r.Rating, (float)query.MinRating.Value));

        // Lọc theo vị trí nếu có
        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            // Approx: filter by bounding box (fast); exact distance can be computed client-side if needed
            var lat = (double)query.Latitude.Value;
            var lng = (double)query.Longitude.Value;
            var radiusKm = (double)query.RadiusKm.Value;
            var latDelta = radiusKm / 110.574; // ~km per degree
            var lngDelta = radiusKm / (111.320 * Math.Cos(lat * Math.PI / 180));

            filters.Add(builder.And(
                builder.Gte(r => r.Latitude, lat - latDelta),
                builder.Lte(r => r.Latitude, lat + latDelta),
                builder.Gte(r => r.Longitude, lng - lngDelta),
                builder.Lte(r => r.Longitude, lng + lngDelta)
            ));
        }

        // Sắp xếp
        var filter = filters.Count == 0 ? Builders<Restaurant>.Filter.Empty : Builders<Restaurant>.Filter.And(filters);
        var totalCount = (int)await _restaurants.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _restaurants.Find(filter)
            .SortByDescending(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
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
        var radiusKm = (double)query.RadiusKm;
        var latDelta = radiusKm / 110.574;
        var lngDelta = radiusKm / (111.320 * Math.Cos(lat * Math.PI / 180));

        var boundingBoxFilter = Builders<Restaurant>.Filter.And(
            Builders<Restaurant>.Filter.Gte(r => r.Latitude, lat - latDelta),
            Builders<Restaurant>.Filter.Lte(r => r.Latitude, lat + latDelta),
            Builders<Restaurant>.Filter.Gte(r => r.Longitude, lng - lngDelta),
            Builders<Restaurant>.Filter.Lte(r => r.Longitude, lng + lngDelta)
        );

        var statusFilter = string.IsNullOrWhiteSpace(query.Status) 
            ? Builders<Restaurant>.Filter.Empty 
            : Builders<Restaurant>.Filter.Eq(r => r.Status, query.Status);
        
        var finalFilter = Builders<Restaurant>.Filter.And(boundingBoxFilter, statusFilter);

        var restaurants = await _restaurants.Find(finalFilter)
            .Limit(query.Limit)
            .ToListAsync(cancellationToken);

        return restaurants;
    }

    /// <summary>
    /// Thêm nhà hàng mới
    /// </summary>
    public async Task<Restaurant> AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        if (restaurant.Id == 0)
        {
            restaurant = Restaurant.Create(
                id: await GetNextIdAsync("restaurants", cancellationToken),
                partnerId: restaurant.PartnerId,
                name: restaurant.Name,
                address: restaurant.Address,
                latitude: restaurant.Latitude,
                longitude: restaurant.Longitude,
                description: restaurant.Description,
                avgPrice: restaurant.AvgPrice
            );
        }
        await _restaurants.InsertOneAsync(restaurant, cancellationToken: cancellationToken);
        return restaurant;
    }

    /// <summary>
    /// Cập nhật nhà hàng
    /// </summary>
    public async Task<Restaurant> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        await _restaurants.ReplaceOneAsync(r => r.Id == restaurant.Id, restaurant, cancellationToken: cancellationToken);
        return restaurant;
    }

    /// <summary>
    /// Xóa nhà hàng (soft delete)
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _restaurants.DeleteOneAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Kiểm tra nhà hàng có tồn tại không
    /// </summary>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var count = await _restaurants.CountDocumentsAsync(r => r.Id == id, cancellationToken: cancellationToken);
        return count > 0;
    }

    /// <summary>
    /// Lấy danh sách nhà hàng theo Partner ID
    /// </summary>
    public async Task<List<Restaurant>> GetByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default)
    {
        return await _restaurants.Find(r => r.PartnerId == partnerId).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task<int> GetNextIdAsync(string sequenceName, CancellationToken cancellationToken)
    {
        var counters = _restaurants.Database.GetCollection<BsonDocument>("counters");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        var result = await counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result.GetValue("seq", 1).AsInt32;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Interfaces;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/partner/moods")]
[Authorize(Roles = "Partner")] // Chỉ Partner
public class PartnerMoodsController : ControllerBase
{
    private readonly IMoodRepository _moodRepository;

    public PartnerMoodsController(IMoodRepository moodRepository)
    {
        _moodRepository = moodRepository;
    }

    /// <summary>
    /// (Partner) Lấy danh sách tất cả các mood
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MoodDto>>>> GetAllMoods()
    {
        var moods = await _moodRepository.GetAllAsync();
        var dtos = moods.Select(m => new MoodDto(m.Id, m.Name, m.Description, m.CreatedAt));
        return Ok(ApiResponse<IEnumerable<MoodDto>>.SuccessResult(dtos));
    }
    
    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new InvalidOperationException("User ID claim (NameIdentifier) not found in token.");
        }
        return int.Parse(userIdStr);
    }
}

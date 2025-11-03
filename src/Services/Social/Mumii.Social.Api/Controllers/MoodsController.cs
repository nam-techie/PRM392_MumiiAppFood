using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/moods")]
public class MoodsController : ControllerBase
{
    private readonly IMoodRepository _moodRepository;

    public MoodsController(IMoodRepository moodRepository)
    {
        _moodRepository = moodRepository;
    }

    /// <summary>
    /// Lấy danh sách tất cả các mood có sẵn
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MoodDto>>>> GetAllMoods()
    {
        var moods = await _moodRepository.GetAllAsync();
        var dtos = moods.Select(m => new MoodDto(m.Id, m.Name, m.Description, m.CreatedAt));
        return Ok(ApiResponse<IEnumerable<MoodDto>>.SuccessResult(dtos));
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một mood theo ID
    /// </summary>
    [HttpGet("{id:int}")] // <<< ENDPOINT MỚI
    public async Task<ActionResult<ApiResponse<MoodDto>>> GetMoodById(int id)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy mood."));
        }

        var dto = new MoodDto(mood.Id, mood.Name, mood.Description, mood.CreatedAt);
        return Ok(ApiResponse<MoodDto>.SuccessResult(dto));
    }
}

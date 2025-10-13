using Microsoft.AspNetCore.Mvc;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/moods")]
public class MoodsController : ControllerBase
{
    private readonly IMoodRepository _moods;

    public MoodsController(IMoodRepository moods)
    {
        _moods = moods;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MoodDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _moods.GetAllAsync(0, 200, cancellationToken);
        var dtos = list.Select(m => new MoodDto(m.Id, m.Name, m.Description, m.CreatedAt)).ToList();
        return Ok(ApiResponse<List<MoodDto>>.SuccessResult(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MoodDto>>> Create([FromBody] CreateMoodRequest request, CancellationToken cancellationToken)
    {
        var mood = Mood.Create(request.Name, request.Description);
        mood = await _moods.AddAsync(mood, cancellationToken);
        var dto = new MoodDto(mood.Id, mood.Name, mood.Description, mood.CreatedAt);
        return Ok(ApiResponse<MoodDto>.SuccessResult(dto, "Tạo mood thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
    {
        await _moods.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResult("Đã xóa mood"));
    }
}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Auth.Domain.Interfaces;


namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/admin/moods")]
[Authorize(Roles = "Admin")] // Chỉ Admin
public class AdminMoodsController : ControllerBase
{
    private readonly IMoodRepository _moodRepository;
    private readonly IPostRepository _postRepository; // <<< THÊM
    private readonly IMongoIdGenerator _idGenerator;

    public AdminMoodsController(
        IMoodRepository moodRepository, 
        IPostRepository postRepository, // <<< THÊM
        IMongoIdGenerator idGenerator)
    {
        _moodRepository = moodRepository;
        _postRepository = postRepository; // <<< THÊM
        _idGenerator = idGenerator;
    }

    /// <summary>
    /// (Admin) Lấy danh sách tất cả các mood
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MoodDto>>>> GetAllMoods()
    {
        var moods = await _moodRepository.GetAllAsync();
        var dtos = moods.Select(m => new MoodDto(m.Id, m.Name, m.Description, m.CreatedAt));
        return Ok(ApiResponse<IEnumerable<MoodDto>>.SuccessResult(dtos));
    }

    /// <summary>
    /// (Admin) Tạo một mood mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MoodDto>>> CreateMood([FromBody] CreateMoodRequest request)
    {
        try
        {
            var newId = await _idGenerator.GetNextIdAsync("moods");
            var mood = Mood.Create(newId, request.Name, request.Description);
            var newMood = await _moodRepository.AddAsync(mood);
            var dto = new MoodDto(newMood.Id, newMood.Name, newMood.Description, newMood.CreatedAt);
            return Ok(ApiResponse<MoodDto>.SuccessResult(dto, "Tạo mood thành công."));
        }
        catch (ArgumentException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }

    /// <summary>
    /// (Admin) Cập nhật một mood
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateMood(int id, [FromBody] UpdateMoodRequest request)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy mood."));

        try
        {
            mood.Update(request.Name, request.Description);
            await _moodRepository.UpdateAsync(mood);
            return Ok(ApiResponse.SuccessResult("Cập nhật mood thành công."));
        }
        catch (ArgumentException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }

    /// <summary>
    /// (Admin) Xóa một mood
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteMood(int id)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null) 
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy mood."));
        }
        
        // >>> LOGIC KIỂM TRA MỚI <<<
        var isMoodInUse = await _postRepository.IsMoodInUseAsync(id);
        if (isMoodInUse)
        {
            return BadRequest(ApiResponse.ErrorResult(
                "Không thể xóa", 
                "Mood này đang được sử dụng bởi một hoặc nhiều bài đăng. Vui lòng gỡ mood khỏi các bài đăng trước khi xóa."));
        }
        // >>> ------------------- <<<
        
        await _moodRepository.DeleteAsync(id);
        return Ok(ApiResponse.SuccessResult("Xóa mood thành công."));
    }
}

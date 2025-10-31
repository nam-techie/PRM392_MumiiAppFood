using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mumii.Auth.Api.Controllers;

[ApiController]
[Route("api/auth/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("ids")]
    [AllowAnonymous]
    public async Task<ActionResult<List<UserDto>>> GetUsersByIds([FromQuery] string ids, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ids)) return Ok(new List<UserDto>());
        var idList = ids.Split(',').Select(s => s.Trim()).Where(s => int.TryParse(s, out _)).Select(int.Parse).Distinct().ToList();
        if (idList.Count == 0) return Ok(new List<UserDto>());

        var users = await _userRepository.GetByIdsAsync(idList, cancellationToken);

        var result = users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.Fullname,
            u.Role,
            u.IsActive,
            u.LoginMethod,
            u.CreatedAt,
            null // Không load profile ở lookup này để nhẹ
        )).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();

        var dto = new UserDto(
            user.Id,
            user.Email,
            user.Fullname,
            user.Role,
            user.IsActive,
            user.LoginMethod,
            user.CreatedAt,
            null
        );

        return Ok(dto);
    }
}



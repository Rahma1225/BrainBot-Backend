using Back_end_chat.Dtos;
using Back_end_chat.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back_end_chat.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			var result = await _userService.GetAllUsersAsync();
			return Ok(result);
		}

		[HttpPost("create")]
		public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
		{
			var result = await _userService.CreateUserAsync(dto);
			if (!result.Succeeded)
				return BadRequest(result.Errors);
			return Ok(new { message = "Utilisateur créé avec succès" });
		}

		[HttpPut("edit")]
		public async Task<IActionResult> EditUser([FromBody] EditUserDto dto)
		{
			var result = await _userService.EditUserAsync(dto);
			if (!result.Succeeded)
				return BadRequest(new { error = result.Errors });

			return Ok(new { message = "Utilisateur modifié avec succès" });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			try
			{
				var (token, user) = await _userService.LoginAndGenerateTokenAsync(dto);
				return Ok(new
				{
					message = "Connexion réussie",
					token,
					currentUser = new
					{
						user.Id,
						user.UserName,
						user.Email,
						user.Roles
					}
				});
			}
			catch (UnauthorizedAccessException ex)
			{
				return Unauthorized(new { message = ex.Message });
			}
		}

		[HttpPut("update-password")]
		public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateDto dto)
		{
			var result = await _userService.UpdatePasswordAsync(dto);
			if (!result.Succeeded)
				return BadRequest(new { error = result.Errors });

			return Ok(new { message = "Mot de passe mis à jour" });
		}

		[HttpPut("lock")]
		public async Task<IActionResult> LockUser([FromBody] LockUserDto dto)
		{
			await _userService.LockOrUnlockUserAsync(dto);
			return Ok(new { message = dto.Lock ? "Utilisateur bloqué" : "Utilisateur débloqué" });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var result = await _userService.DeleteUserAsync(id);
			if (!result.Succeeded)
				return BadRequest(new { error = result.Errors });

			return Ok(new { message = "Utilisateur supprimé" });
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			try
			{
				var message = await _userService.SendPasswordResetEmailAsync(dto);
				return Ok(new { message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}
	}
}

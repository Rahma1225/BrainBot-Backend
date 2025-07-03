using Back_end_chat.Dtos;
using Back_end_chat.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Back_end_chat.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<ApplicationRole> _roleManager;
		private readonly EmailService _emailService;
		private readonly TokenService _tokenService;

		public UserService(UserManager<ApplicationUser> userManager,
						   RoleManager<ApplicationRole> roleManager,
						   EmailService emailService,
						   TokenService tokenService)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_emailService = emailService;
			_tokenService = tokenService;
		}

		public async Task<IEnumerable<UserWithRolesDto>> GetAllUsersAsync()
		{
			var users = _userManager.Users.ToList();
			var result = new List<UserWithRolesDto>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				result.Add(new UserWithRolesDto
				{
					Id = user.Id.ToString(),
					UserName = user.UserName,
					Email = user.Email,
					Roles = roles,
					IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
				});
			}
			return result;
		}

		public async Task<IdentityResult> CreateUserAsync(CreateUserDto dto)
		{
			var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
			var result = await _userManager.CreateAsync(user, dto.Password);
			if (!result.Succeeded) return result;

			var roleToAssign = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
			if (!await _roleManager.RoleExistsAsync(roleToAssign))
				await _roleManager.CreateAsync(new ApplicationRole(roleToAssign));

			await _userManager.AddToRoleAsync(user, roleToAssign);

			var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
			var html = $@"
				<h2>Bienvenue {dto.UserName} 👋</h2>
				<p>Votre compte a été créé avec succès.</p>
				<p><strong>Identifiants :</strong></p>
				<ul>
					<li><strong>Nom d'utilisateur:</strong> {dto.UserName}</li>
					<li><strong>Mot de passe:</strong> {dto.Password}</li>
				</ul>
				<p>Merci de rejoindre notre application !</p>
				##IMAGE##
			";

			await _emailService.SendEmailWithInlineImageAsync(
				dto.Email,
				"Bienvenue sur notre plateforme",
				html,
				logoPath
			);

			return result;
		}

		public async Task<IdentityResult> EditUserAsync(EditUserDto dto)
		{
			var user = await _userManager.FindByIdAsync(dto.Id);
			if (user == null)
				return IdentityResult.Failed(new IdentityError { Description = "User not found" });

			user.UserName = dto.NewUserName;
			user.Email = dto.NewEmail;
			var updateResult = await _userManager.UpdateAsync(user);
			if (!updateResult.Succeeded) return updateResult;

			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles);

			if (!await _roleManager.RoleExistsAsync(dto.NewRole))
				await _roleManager.CreateAsync(new ApplicationRole(dto.NewRole));

			return await _userManager.AddToRoleAsync(user, dto.NewRole);
		}

		public async Task<string> SendPasswordResetEmailAsync(ForgotPasswordDto dto)
		{
			var users = _userManager.Users.Where(u => u.Email == dto.Email).ToList();

			if (!users.Any())
				throw new Exception("Email non trouvé");
			if (users.Count > 1)
				throw new Exception("Plusieurs comptes avec cet email existent");

			var user = users.First();
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var resetLink = $"https://votre-site.com/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

			var html = $@"
			<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
				<h2>Réinitialisation du mot de passe</h2>
				<p>Cliquez sur le lien ci-dessous pour réinitialiser votre mot de passe :</p>
				<p><a href='{resetLink}'>{resetLink}</a></p>
				##IMAGE##
			</div>";

			var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

			await _emailService.SendEmailWithInlineImageAsync(
				dto.Email,
				"Réinitialisation de mot de passe",
				html,
				logoPath
			);

			return "Lien de réinitialisation envoyé par email";
		}

		public async Task<IdentityResult> UpdatePasswordAsync(PasswordUpdateDto dto)
		{
			var user = await _userManager.FindByNameAsync(dto.UserName);
			if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

			var passwordValid = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
			if (!passwordValid) return IdentityResult.Failed(new IdentityError { Description = "Incorrect old password" });

			return await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
		}

		public async Task LockOrUnlockUserAsync(LockUserDto dto)
		{
			var user = await _userManager.FindByNameAsync(dto.UserName);
			if (user == null) return;

			if (dto.Lock)
				await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
			else
				await _userManager.SetLockoutEndDateAsync(user, null);
		}

		public async Task<IdentityResult> DeleteUserAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

			return await _userManager.DeleteAsync(user);
		}

		public async Task<List<string>> GetRolesAsync(ApplicationUser user)
		{
			return (await _userManager.GetRolesAsync(user)).ToList();
		}

		public async Task<(string token, ApplicationUser user)> LoginAndGenerateTokenAsync(LoginDto dto)
		{
			var user = await _userManager.FindByNameAsync(dto.UserName);
			if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
				throw new UnauthorizedAccessException("Invalid credentials");

			if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
				throw new UnauthorizedAccessException("Account is locked");

			var roles = await _userManager.GetRolesAsync(user);
			var role = roles.FirstOrDefault() ?? "User";
			var token = _tokenService.CreateToken(user.Id.ToString(), user.UserName, role);

			return (token, user);
		}
	}
}

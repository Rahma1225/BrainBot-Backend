using Back_end_chat.Dtos;
using Back_end_chat.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back_end_chat.Services
{
	public interface IUserService
	{
		Task<IEnumerable<UserWithRolesDto>> GetAllUsersAsync();
		Task<IdentityResult> CreateUserAsync(CreateUserDto dto);
		Task<IdentityResult> UpdatePasswordAsync(PasswordUpdateDto dto);
		Task LockOrUnlockUserAsync(LockUserDto dto);
		Task<IdentityResult> DeleteUserAsync(string userId);
		Task<string> SendPasswordResetEmailAsync(ForgotPasswordDto dto);
		Task<IdentityResult> EditUserAsync(EditUserDto dto);
		Task<(string token, ApplicationUser user)> LoginAndGenerateTokenAsync(LoginDto dto);
		Task<List<string>> GetRolesAsync(ApplicationUser user);
	}
}

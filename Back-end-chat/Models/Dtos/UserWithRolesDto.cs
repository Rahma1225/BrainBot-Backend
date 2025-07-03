using System.Collections.Generic;

namespace Back_end_chat.Dtos
{
	public class UserWithRolesDto
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public IEnumerable<string> Roles { get; set; }
		public bool IsLocked { get; set; }
	}
}

namespace Back_end_chat.Dtos
{
	public class CreateUserDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
	}
}

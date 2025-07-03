namespace Back_end_chat.Dtos
{
	public class PasswordUpdateDto
	{
		public string UserName { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}
}

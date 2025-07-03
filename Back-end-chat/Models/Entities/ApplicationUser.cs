using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;

[CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
	public string FullName { get; set; } = string.Empty;
}

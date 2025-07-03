using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;

[CollectionName("Roles")]
public class ApplicationRole : MongoIdentityRole<Guid>
{
	public ApplicationRole() : base() { }
	public ApplicationRole(string roleName) : base(roleName) { }
}

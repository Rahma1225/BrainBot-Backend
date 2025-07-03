using Back_end_chat.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Identity with MongoDB
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
		builder.Configuration.GetConnectionString("MongoDb"),
		"brianbot")
	.AddDefaultTokenProviders();

// ✅ Custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();

// ✅ Controllers & JSON
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	});

// ✅ CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowLocal", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

// ✅ Auth
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat API", Version = "v1" });
});

var app = builder.Build();

// ✅ Middleware
app.UseCors("AllowLocal");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using FluentValidation;
using Microsoft.Extensions.Azure;
using TestTaskTraineeCamp.Server;
using TestTaskTraineeCamp.Server.Services;
using TestTaskTraineeCamp.Server.Validators;

var builder = WebApplication.CreateBuilder(args);

var azureBlobSettingsOption = builder.Configuration.GetSection(AzureBlobSettingsOption.ConfigKey)
    .Get<AzureBlobSettingsOption>()!;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(azureBlobSettingsOption);
builder.Services.AddAzureClients(x => x
                                      .AddBlobServiceClient(azureBlobSettingsOption.ConnectionString)
                                      .WithName(azureBlobSettingsOption.ConnectionName));


builder.Services.AddTransient<IBlobService, BlobService>();
builder.Services.AddTransient<IValidator<IFormFile>, FileUploadValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("https://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using ValleyVisionSolution.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//Add session state
builder.Services.AddSession();

builder.Services.AddSingleton<BlobServiceClient>(provider => {
    var connectionString = builder.Configuration.GetValue<string>("AzureBlobStorage:ConnectionString");
    return new BlobServiceClient(connectionString);
});
builder.Services.AddSingleton<IBlobService, BlobService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();




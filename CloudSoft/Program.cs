using CloudSoft.Services;
using CloudSoft.Repositories;
using CloudSoft.Models;
using CloudSoft.Configurations;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register MVC Controllers with Views
builder.Services.AddControllersWithViews();

// ✅ Check if MongoDB should be used (default to false if not specified)
bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    // ✅ Configure MongoDB options
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    // ✅ Configure MongoDB client
    builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
        var mongoDbOptions = builder.Configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>();

        return new MongoClient(mongoDbOptions?.ConnectionString);
    });

    // ✅ Configure MongoDB collection
    builder.Services.AddSingleton<IMongoCollection<Subscriber>>(serviceProvider => {
        var mongoDbOptions = builder.Configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>();

        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase(mongoDbOptions?.DatabaseName);

        return database.GetCollection<Subscriber>(mongoDbOptions?.SubscribersCollectionName);
    });

    // ✅ Register MongoDB repository
    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();

    Console.WriteLine(" Using MongoDB repository");
}
else
{
    // ✅ Register in-memory repository as fallback
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

    Console.WriteLine("Using in-memory repository");
}

// ✅ Register the service (depends on repository)
builder.Services.AddScoped<INewsletterService, NewsletterService>();

// ✅ Build the application
var app = builder.Build();

// ✅ Middleware configuration happens AFTER building the app

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

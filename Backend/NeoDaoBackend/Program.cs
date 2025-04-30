using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.HttpLogging;
using NeoDaoBackend.Background;
using NeoDaoBackend.Extensions;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Handlers;
using NeoDaoBackend.Mapping;
using NeoDaoBackend.Metrics;
using NeoDaoBackend.Middlewares;
using NeoDaoBackend.Models;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Service;
using NeoDaoBackend.Service.GraphQL;
using NeoDaoBackend.Services.OpenMatch;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Graylog;
using static NeoDaoBackend.Models.Constants;

var builder = WebApplication.CreateBuilder(args);

#region Logging

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("Platform");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});
LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs\\NeoDaoBackend-.log", rollingInterval: RollingInterval.Day);

if (!builder.Environment.IsDevelopment())
{
    loggerConfiguration.WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = "graylog.metacity.svc.cluster.local", //Environment.GetEnvironmentVariable("GRAYLOG_URI"),
        Port = 12201,
        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp
    });
}

if (builder.Environment.IsDevelopment() && false)
{
    loggerConfiguration.MinimumLevel.Debug();
}
else
{
    loggerConfiguration.MinimumLevel.Information();
}

var logger = loggerConfiguration.CreateLogger();
builder.Host.UseSerilog(logger);

#endregion

builder.Services.AddDbContext<NeoDaoDbContext>();
//Add repository
builder.Services
        .AddScoped<IUserSessionRepository, UserSessionRepository>()
        .AddScoped<IUserAvatarRepository, UserAvatarRepository>()
        .AddScoped<IUserDataRepository, UserDataRepository>()
        .AddScoped<IGroupRepository, GroupRepository>()
        .AddScoped<IUserInventoryRepository, UserInventoryRepository>()
        .AddScoped<IUserEquipmentRepository, UserEquipmentRepository>()
        .AddScoped<IUserLocationRepository, UserLocationRepository>()
        .AddScoped<IUserMissionRepository, UserMissionRepository>()
        .AddScoped<IUserBalanceRepository, UserBalanceRepository>()
        .AddScoped<IUserRelationsRepository, UserRelationsRepository>()
        .AddScoped<IUserTransportRepository, UserTransportRepository>()
        .AddScoped<IChatRepository, ChatRepository>()
        .AddScoped<IStoreRepository, StoreRepository>()
        .AddScoped<IUserEmotionRepository, UserEmotionRepository>()
        .AddScoped<IStreamRepository, StreamRepository>()
        .AddScoped<IAuctionRepository, AuctionRepository>()
        .AddScoped<IUserCustomizationRepository, UserCustomizationRepository>()
        .AddScoped<INotificationRepository, NotificationRepositoryInMemory>();

//Add services
builder.Services
        .AddSingleton<WebSocketConnectionManager>()
        .AddScoped<AuthorizationService>()
        .AddScoped<NotificationService>();
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test")
{
    builder.Services.AddScoped<IOpenMatchTicketService, RealOpenMatchTicketService>();
    builder.Services.AddScoped<IBridgeService, FakeBridgeService>();
}
else
{
    builder.Services.AddScoped<IOpenMatchTicketService, FakeOpenMatchTicketService>();
    builder.Services.AddScoped<IBridgeService, FakeBridgeService>();
}
builder.Services
        .AddScoped<UserDataService>()
        .AddScoped<UserRelationsService>()
        .AddScoped<GroupService>()
        .AddScoped<UserInventoryService>()
        .AddScoped<UserEquipmentService>()
        .AddScoped<UserLocationService>()
        .AddScoped<UserMissionService>()
        .AddScoped<UserBalanceService>()
        .AddScoped<UserTransportService>()
        .AddScoped<StoresService>()
        .AddScoped<ChatService>()
        .AddScoped<MatchmakingService>()
        .AddScoped<StreamingService>()
        .AddScoped<AuctionService>()
        .AddScoped<WebSocketHandler>()
        .AddScoped<UserEmotionService>()
        .AddScoped<UserCustomizationService>()
        .AddEndpointsApiExplorer()
        .AddSwaggerGen()
        .AddHostedService<SessionExpirationJob>()
        .AddHostedService<NotificationJob>()
        .AddHostedService<ApplicationLifetime>()
        .AddHostedService<StreamJobService>()
        .AddHostedService<СalculatingAuctionJob>()
        .AddControllers(options =>
        {
            options.Filters.Add<ExceptionFilter>();
        })
        .AddNewtonsoftJson(opts =>
        {
            opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            opts.SerializerSettings.ContractResolver = new OrderedContractResolver();
            opts.SerializerSettings.Converters.Add(new StringEnumConverter());
        });

#region Telemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder.AddPrometheusExporter();
        meterProviderBuilder.AddHttpClientInstrumentation();
        meterProviderBuilder.AddRuntimeInstrumentation();
        meterProviderBuilder.AddProcessInstrumentation();
        meterProviderBuilder.AddAspNetCoreInstrumentation();
        meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "BackendNeoDao.WebSocket");
    });

//AddMetrics
builder.Services.AddSingleton<UserWSMetrics>();
#endregion

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddScoped(s => new GraphQLHttpClient(Environment.GetEnvironmentVariable("GRAPHQL_URI") ?? builder.Configuration["GraphQLURI"]!, new NewtonsoftJsonSerializer()));
builder.Services.AddScoped<IValidationStorage, ValidationStorage>();

JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    ContractResolver = new OrderedContractResolver(),
    DateFormatString = DateTimeOffsetFormatString,
};
builder.Services.AddScoped(_ => jsonSerializerSettings);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});


var app = builder.Build();

app.MigrateDatabase<NeoDaoDbContext>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeoDaoBackend.API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.UseMiddleware<BridgeAuthenticationMiddleware>();
app.MapControllers();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
};
app.UseWebSockets(webSocketOptions);

app.UseHttpLogging();
app.UseRouting();
app.MapGet("/api/public/ping", () => "O Captain! My Captain!");
app.UseOpenTelemetryPrometheusScrapingEndpoint();



app.Run();

public partial class Program { }

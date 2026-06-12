using BookingDDD.Api;
using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Application;
using BookingDDD.Core.Domain;
using BookingDDD.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDb")
    ?? throw new InvalidOperationException(
        "Connection string 'BookingDb' is missing.");

builder.Services.AddSingleton(new SqlServerOptions(connectionString));
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<DapperUnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<DapperUnitOfWork>());
builder.Services.AddScoped<IResourceRepository, DapperResourceRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

builder.Services.AddSingleton<IAuditLog, SqlAuditLog>();
builder.Services.AddSingleton<IBookingCalendar, SqlBookingCalendar>();
builder.Services.AddSingleton<IBookingNotification, ConsoleBookingNotification>();

builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>,
    AuditBookingCreatedHandler>();
builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>,
    AddBookingToCalendarHandler>();
builder.Services.AddTransient<
    IDomainEventHandler<BookingCreated>,
    SendBookingConfirmationHandler>();
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>,
    AuditBookingCancelledHandler>();
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>,
    RemoveBookingFromCalendarHandler>();
builder.Services.AddTransient<
    IDomainEventHandler<BookingCancelled>,
    SendBookingCancellationHandler>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost(
    "/api/resources/{resourceId:guid}/bookings",
    BookingEndpoints.CreateBookingAsync);

app.MapDelete(
    "/api/resources/{resourceId:guid}/bookings/{bookingId:guid}",
    BookingEndpoints.CancelBookingAsync);

app.Run();

public partial class Program;

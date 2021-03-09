using Accessibility.Application.Configuration.Database;
using Accessibility.Domain.SeedWork;
using Accessibility.Infrastructure.Database;
using Accessibility.Infrastructure.Domain;
using Accessibility.Infrastructure.Processing;
using Accessibility.Infrastructure.Processing.Outbox;
using Accessibility.Infrastructure.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using Accessibility.Domain.Schedules;
using Accessibility.Application.Schedules.DomainServices;
using Accessibility.Infrastructure.Domain.Schedules;
using Microsoft.Extensions.Configuration;
using Accessibility.Application.Schedules.Commands.CreateSchedule;
using Accessibility.Infrastructure.IntegrationEvents.Facilities;
using Accessibility.Application.Facilities;
using Accessibility.Domain.Bookings;
using Accessibility.Infrastructure.Domain.Bookings;
using MassTransit;
using Accessibility.Application.Bookings.IntegrationEvents.EventHandling;
using Accessibility.Application.Facilities.IntegrationEvents.EventHandling;
using Accessibility.Application.Schedules;
using Accessibility.Infrastructure.Application.Schedules;
using Accessibility.Infrastructure.Application.Bookings;
using Accessibility.Application.Bookings.Queries;

namespace Accessibility.Infrastructure
{
    public static class Startup
    {
        // TODO: split registrations into modules
        public static IServiceCollection ConfigureAccessibility(this IServiceCollection services, IConfiguration configuration, Assembly applicationAssembly)
        {
            var connectionString = configuration.GetConnectionString("Accessibility");

            services.AddDbContext<AccessibilityContext>(options =>
                options
                    .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()
                    .UseNpgsql(connectionString))
                .AddMediatR(typeof(CreateScheduleCommand).Assembly, typeof(ScheduleCreatedEvent).Assembly, typeof(ProcessOutboxCommand).Assembly)
                .AddTransient<IScheduleRepository, ScheduleRepository>()
                .AddTransient<IScheduleQueryRepository, ScheduleQueryRepository>()
                .AddTransient<ISchedulePeriodOfTimeChecker, SchedulePeriodOfTimeChecker>()
                .AddTransient<IBookingRepository, BookingRepository>()
                .AddTransient<IBookingQueryRepository, BookingQueryRepository>()
                .AddTransient<IOfferRepository, OfferRepository>()
                .AddTransient<IUnitOfWork, UnitOfWork>()
                .AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>()
                //.AddHostedService<ProcessOutboxHostedService>()
                .AddScoped<ISqlConnectionFactory>(x => new SqlConnectionFactory(connectionString))
                .AddSingleton<IAssemblyProvider>(x => new AssemblyProvider(applicationAssembly))
                .ConfigureEventBus(configuration);

            return services;
        }

        private static IServiceCollection ConfigureEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddMassTransit(x =>
            {
                x.AddConsumer<ProcessBookingOrderConsumer>()
                    .Endpoint(e => {e.ConcurrentMessageLimit = 1;});
                x.AddConsumer<OfferCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:HostName"], cfgH =>
                    {
                        cfgH.Username(configuration["RabbitMQ:Username"]);
                        cfgH.Password(configuration["RabbitMQ:Password"]);
                    });
                    
                    cfg.ReceiveEndpoint("booking-orders-listener", e =>
                        e.ConfigureConsumer<ProcessBookingOrderConsumer>(context));
                    
                    cfg.ReceiveEndpoint("offer-created-listener", e =>
                        e.ConfigureConsumer<OfferCreatedConsumer>(context));
                });
            })
            .AddMassTransitHostedService();
        }
    }
}
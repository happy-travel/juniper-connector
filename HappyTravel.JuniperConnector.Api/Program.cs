using System.Diagnostics;
using HappyTravel.BaseConnector.Api.Infrastructure.Extensions;

namespace HappyTravel.JuniperConnector.Api;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseKestrel(options =>
                    {
                        options.ConfigureBaseConnector();
                    })
                    .UseDefaultServiceProvider(s =>
                    {
                        s.ValidateScopes = true;
                        s.ValidateOnBuild = true;
                    })
                    .UseSentry(options =>
                    {
                        options.Dsn = Environment.GetEnvironmentVariable("HTDC_JUNIPER_SENTRY_ENDPOINT");
                        options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        options.IncludeActivityData = true;
                        options.BeforeSend = sentryEvent =>
                        {
                            if (Activity.Current is null)
                                return sentryEvent;

                            foreach (var (key, value) in Activity.Current.Baggage)
                                sentryEvent.SetTag(key, value ?? string.Empty);

                            sentryEvent.SetTag("TraceId", Activity.Current.TraceId.ToString());
                            sentryEvent.SetTag("SpanId", Activity.Current.SpanId.ToString());

                            return sentryEvent;
                        };
                    });
            })
            .ConfigureBaseConnector(ConnectorName);



    public const string ConnectorName = "juniper-connector";
}
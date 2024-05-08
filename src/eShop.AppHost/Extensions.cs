using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspire.Hosting.Lifecycle;

namespace eShop.AppHost;

internal static class Extensions
{
    /// <summary>
    /// Adds a hook to set the ASPNETCORE_FORWARDEDHEADERS_ENABLED environment variable to true for all projects in the application.
    /// </summary>
    public static IDistributedApplicationBuilder AddForwardedHeaders(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    private class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            foreach (var p in appModel.GetProjectResources())
            {
                p.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
                {
                    context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = "true";
                }));
            }

            return Task.CompletedTask;
        }
    }

    public static IResourceBuilder<PostgresServerResource> ConfigureForAzure(this IResourceBuilder<PostgresServerResource> postgres)
    {
        var template = postgres.ApplicationBuilder.AddBicepTemplateString("vector-extension", """
    param postgresServerName string

    @description('')
    param location string = resourceGroup().location

    resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
        name: postgresServerName
    }

    resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = {
      parent: postgresServer
      name: 'azure.extensions'
      properties: {
        value: 'VECTOR'
        source: 'user-override'
      }
    }
    """);

#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        postgres.PublishAsAzurePostgresFlexibleServer((resource, construct, server) =>
        {
            construct.AddOutput(server.AddOutput("name", data => data.Name));
            template.WithParameter("postgresServerName", resource.GetOutput("name"));
        });
#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        return postgres;
    }

    public static IResourceBuilder<RedisResource> ConfigureForAzure(this IResourceBuilder<RedisResource> redis) => redis.PublishAsAzureRedis();
}

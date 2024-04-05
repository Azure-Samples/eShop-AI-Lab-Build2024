﻿using eShop.AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddForwardedHeaders();

var redis = builder.AddRedis("redis");
var rabbitMq = builder.AddRabbitMQ("eventbus");

var postgres = builder.AddPostgres("postgres");

if (!builder.ExecutionContext.IsPublishMode)
{
    postgres.WithImage("pgvector/pgvector").WithImageTag("pg16");
}
else
{
    postgres.ConfigureForAzure();
    redis.ConfigureForAzure();
}

var catalogDb = postgres.AddDatabase("catalogdb");
var identityDb = postgres.AddDatabase("identitydb");
var orderDb = postgres.AddDatabase("orderingdb");
var webhooksDb = postgres.AddDatabase("webhooksdb");

// Services
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api", "http")
    .WithReference(identityDb);

var idpHttps = identityApi.GetEndpoint("http");

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Identity__Url", idpHttps);

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitMq)
    .WithReference(catalogDb);

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq)
    .WithReference(orderDb)
    .WithEnvironment("Identity__Url", idpHttps);

builder.AddProject<Projects.OrderProcessor>("order-processor")
    .WithReference(rabbitMq)
    .WithReference(orderDb);

builder.AddProject<Projects.PaymentProcessor>("payment-processor")
    .WithReference(rabbitMq);

var webHooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(rabbitMq)
    .WithReference(webhooksDb)
    .WithEnvironment("Identity__Url", idpHttps);

// Reverse proxies
builder.AddProject<Projects.Mobile_Bff_Shopping>("mobile-bff")
    .WithReference(catalogApi)
    .WithReference(identityApi);

// Apps
var webhooksClient = builder.AddProject<Projects.WebhookClient>("webhooksclient")
    .WithReference(webHooksApi)
    .WithEnvironment("IdentityUrl", idpHttps);

var webApp = builder.AddProject<Projects.WebApp>("webapp", "http")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(rabbitMq)
    .WithEnvironment("IdentityUrl", idpHttps);

// set to true if you want to use OpenAI
bool useOpenAI = true;
if (useOpenAI)
{
    const string openAIName = "openai";
    const string textEmbeddingName = "text-embedding-ada-002";
    const string chatModelName = "gpt-35-turbo-16k";

    // to use an existing OpenAI resource, add the following to the AppHost user secrets:
    // "ConnectionStrings": {
    //   "openai": "Key=<API Key>" (to use https://api.openai.com/)
    //     -or-
    //   "openai": "Endpoint=https://<name>.openai.azure.com/" (to use Azure OpenAI)
    // }
    if (builder.Configuration.GetConnectionString(openAIName) is not null)
    {
        var openAI = builder.AddConnectionString(openAIName);

        catalogApi
            .WithReference(openAI)
            .WithEnvironment("AI__OPENAI__EMBEDDINGNAME", textEmbeddingName);

        webApp
            .WithReference(openAI)
            .WithEnvironment("AI__OPENAI__CHATMODEL", chatModelName); ;
    }
    else
    {
        // to use Azure provisioning, add the following to the AppHost user secrets:
        // "Azure": {
        //   "SubscriptionId": "<your subscription ID>"
        //   "Location": "<location>"
        // }
        var chatAIResource = builder.AddAzureOpenAI(openAIName)
            .AddDeployment(new AzureOpenAIDeployment(chatModelName, "gpt-35-turbo", "0613"));

        var embeddingAIResource = builder.AddAzureOpenAI($"embedding")
            .AddDeployment(new AzureOpenAIDeployment(textEmbeddingName, "text-embedding-ada-002", "2"));

        catalogApi
            .WithReference(chatAIResource)
            .WithReference(embeddingAIResource)
            .WithEnvironment("AI__OPENAI__EMBEDDINGNAME", textEmbeddingName);

        webApp
            .WithReference(chatAIResource)
            .WithReference(embeddingAIResource)
            .WithEnvironment("AI__OPENAI__CHATMODEL", chatModelName);
    }
}

// Wire up the callback urls (self referencing)
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint("http"));
webhooksClient.WithEnvironment("CallBackUrl", webhooksClient.GetEndpoint("http"));

// Identity has a reference to all of the apps for callback urls, this is a cyclic reference
identityApi.WithEnvironment("BasketApiClient", basketApi.GetEndpoint("http"))
           .WithEnvironment("OrderingApiClient", orderingApi.GetEndpoint("http"))
           .WithEnvironment("WebhooksApiClient", webHooksApi.GetEndpoint("http"))
           .WithEnvironment("WebhooksWebClient", webhooksClient.GetEndpoint("http"))
           .WithEnvironment("WebAppClient", webApp.GetEndpoint("http"));

builder.Build().Run();

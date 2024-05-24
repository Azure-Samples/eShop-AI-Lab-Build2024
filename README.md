# LAB: Learn how to implement AI with the eShop Reference Application - "Northern Mountains"
![eShop Reference Application architecture diagram](/img/eshop_architecture.png)

![eShop homepage screenshot](/img/eshop_homepage.png)

## Getting Started

### Prerequisites

- Clone this repository
- (Windows only) Install Visual Studio. Visual Studio contains tooling support for .NET Aspire that you will want to have. [Visual Studio 2022 version 17.10 Preview](https://visualstudio.microsoft.com/vs/preview/).
  - During installation, ensure that the following are selected:
    - `ASP.NET and web development` workload.
    - `.NET Aspire SDK` component in `Individual components`.
- Install the latest [.NET 8 SDK](https://github.com/dotnet/installer#installers-and-binaries)
- On Mac/Linux (or if not using Visual Studio), install the Aspire workload with the following commands:

```powershell
dotnet workload update
dotnet workload install aspire
dotnet restore eShop.Web.slnf
```

- Install & start Docker Desktop: https://docs.docker.com/engine/install/

### Running the solution

> [!WARNING]
> Remember to ensure that Docker is started

- (Windows only) Run the application from Visual Studio:

* Open the `eShop.Web.slnf` file in Visual Studio
* Ensure that `eShop.AppHost.csproj` is your startup project
* Hit Ctrl-F5 to launch Aspire

- Or run the application from your terminal:

```powershell
dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj
```

then look for lines like this in the console output in order to find the URL to open the Aspire dashboard:

```sh
Now listening on: http://localhost:18848
```

### Sample data

The sample catalog data is defined in [catalog.json](https://github.com/dotnet/eShop/blob/main/src/Catalog.API/Setup/catalog.json). Those product names, descriptions, and brand names are fictional and were generated using [GPT-35-Turbo](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt), and the corresponding [product images](https://github.com/dotnet/eShop/tree/main/src/Catalog.API/Pics) were generated using [DALL·E 3](https://openai.com/dall-e-3).

## Implement the AI

[Follow the guide to implement AI here!](../main/docs/ai-lab/README.md) and learn more about AI with a Microsoft Learn session: [Infusing your .NET Apps with AI: Practical Tools and Techniques](https://build.microsoft.com/sessions/4bf46250-6959-4df4-957f-b355e723c5c6).

## Contributing

For more information on contributing to this repo, please read [the contribution documentation](./CONTRIBUTING.md) and [the Code of Conduct](CODE-OF-CONDUCT.md).

## eShop on Azure

For a version of this app configured for deployment on Azure, please view [the eShop on Azure](https://github.com/Azure-Samples/eShopOnAzure) repo.

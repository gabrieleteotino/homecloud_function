# homecloud_function
Function app code for Homecloud

## Azure Functions

Commands for dotnet

Create a new function project

```
func init LocalFunctionProj --dotnet
```

Create a new function class

```
func new --name HttpExample --template "HTTP trigger" --authlevel "anonymous"
func new --name Devops --template "HTTP trigger" --authlevel "function"
```

View templates

```
func templates list
```

Execute function locally

```
func start
```

### DI and Automapper

Install nuget

```
dotnet add package Microsoft.Azure.Functions.Extensions
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.NET.Sdk.Functions
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

Add a Startup.cs class

```csharp
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Homecloud.Function.Startup))]
namespace Homecloud.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
        }
    }
}
```

Add profiles for Automapper

```csharp
using AutoMapper;

namespace Homecloud.Models.Profiles
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Homecloud.Models.Devops.OrganizationProject, Homecloud.Models.DevopsApi.OrganizationProject>();
        }
    }
}
```

Register profiles in Startup

```csharp
    builder.Services.AddAutoMapper(typeof(Startup));
```

## DevOps API

- [Git Reference](https://docs.microsoft.com/en-us/rest/api/azure/devops/git/?view=azure-devops-rest-6.0)
- [Pipeline Reference](https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/?view=azure-devops-rest-6.0)
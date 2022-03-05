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

## DevOps API

- [Git Reference](https://docs.microsoft.com/en-us/rest/api/azure/devops/git/?view=azure-devops-rest-6.0)
- [Pipeline Reference](https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/?view=azure-devops-rest-6.0)
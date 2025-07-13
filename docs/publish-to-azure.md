# Publishing an ASP.NET Core App to Azure (Using Visual Studio Code)

This guide explains how to publish your ASP.NET Core app to Azure App Service using only Visual Studio Code and the command line.

## Prerequisites
- Azure account ([https://portal.azure.com](https://portal.azure.com))
- Azure App Service created (Web App)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

## Steps

### 1. Build and Publish Your App
Open a terminal in your project folder and run:

```
dotnet publish -c Release -o ./publish
```
This creates a `publish` folder with all the files needed for deployment.

### 2. Sign in to Azure
```
az login
```
Follow the instructions to sign in.

### 3. Deploy to Azure App Service
Replace `<app-name>` with your Azure Web App name and `<resource-group>` with your resource group:

```
az webapp deploy --resource-group <resource-group> --name <app-name> --src-path ./publish
```

Alternatively, you can use FTP or Zip Deploy from the Azure Portal's Deployment Center.

### 4. Configure App Settings (Optional)
- In the Azure Portal, go to your App Service > Configuration.
- Set environment variables or connection strings as needed.

### 5. Browse Your Site
- Find your app’s URL in the Azure Portal and open it in your browser.

## Notes
- You do **not** need `launchSettings.json` or other development-only files for deployment.
- For production, use `appsettings.Production.json` or configure settings in Azure.
- You can automate deployment with GitHub Actions or Azure DevOps for CI/CD.

## References
- [Deploy ASP.NET Core app to Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore)
- [Azure CLI WebApp Docs](https://learn.microsoft.com/en-us/cli/azure/webapp)

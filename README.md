# About
This example project will show you how the tokens in Xena OAuth works and how to interact with Xena from a .NET Core application.


# Running the demo
This is rather easy: you can download the source and open it in Visual Studio or you can simply use Docker to build and host the sample. Either way we have to do bit of setup. No worries, we'll walk you though it in a few easy steps!

## Step 1 - Create an App in Xena
Create the app and add http://localhost:5002 as Callback URL. If you plan to use Docker you'll need to use your Docker host IP instead of localhost, usually that is 10.0.75.2, e.g. http://10.0.75.2:5002. You can use another port if you'd like, but you'll need to change the project and configuration likewise then.

## Step 2 - Checkout the code
Checkout the code from the github repository, either download as a zip or clone the repository.

## Step 3 - Configure `appsettings.json`
It's important to update `\src\appsettings.json` to your environment. The section looks like this:
```
  "XenaProvider": {
    "Authority": "https://login.xena.biz",
    "ClientID": "[your-clientid-here]",
    "ClientSecret": "[your-clientsecret-here]"
  }
```
You'll need to copy/paste the ClientID and Client Secret from the App you've created in Step 1 into this configuration. Also make sure that the Callback URL matches. If any of these three are wrong you'll get **Unauthorized - Invalid client** when trying to login!

## Step 4 - Build'n'run
Then build and run the docker image with:
```
# docker build -t Xena.ExampleApp.CSharp .
# docker run -p 5002:5002 Xena.ExampleApp.CSharp
```

**Profit!**
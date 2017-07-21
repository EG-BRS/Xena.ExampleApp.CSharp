FROM microsoft/dotnet:1.1.2-sdk

WORKDIR /app

COPY src/ .

RUN dotnet restore
RUN dotnet publish -c Debug -o bin

ENTRYPOINT dotnet bin/ExampleProject.dll

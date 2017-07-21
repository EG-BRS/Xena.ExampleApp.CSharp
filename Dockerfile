FROM microsoft/dotnet:2-sdk

WORKDIR /app

COPY src/ .

RUN dotnet restore
RUN dotnet publish -c Release -o bin

ENTRYPOINT dotnet bin/ExampleProject.dll

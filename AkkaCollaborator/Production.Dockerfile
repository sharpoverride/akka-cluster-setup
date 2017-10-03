FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY bin/Release/PublishOutput ./
ENTRYPOINT ["dotnet", "Collaborator.dll"]

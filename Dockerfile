FROM mcr.microsoft.com/dotnet/aspnet:8.0
USER $APP_UID
WORKDIR /app
EXPOSE 8080
COPY --from=evil-giraf-base /app/publish .
ENTRYPOINT ["dotnet", "EvilGiraf.dll"]

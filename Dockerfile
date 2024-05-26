FROM node:18-alpine AS febuild
WORKDIR "/app"
COPY . .
WORKDIR "/app/FCloud3.AppFront/FCloud3Front"
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS bebuild
WORKDIR "/src"
COPY . .
WORKDIR "/src/FCloud3.App"
RUN dotnet publish "./FCloud3.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN sed -i 's|\[openssl_init\]|&\nssl_conf = ssl_configuration\n[ssl_configuration]\nsystem_default = tls_system_default\n[tls_system_default]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0|' /etc/ssl/openssl.cnf
WORKDIR "/app"
COPY --from=bebuild /app/publish .
COPY --from=febuild /app/FCloud3.App/wwwroot ./wwwroot
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "FCloud3.App.dll"]
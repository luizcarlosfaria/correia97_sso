#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV TZ=America/Sao_Paulo
RUN cat /usr/share/zoneinfo/$TZ > /etc/localtime \
		&& cat /usr/share/zoneinfo/$TZ > /etc/timezone \
		&& update-ca-certificates
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ./src/Cadastro.WorkerService/Cadastro.WorkerService.csproj ./Cadastro.WorkerService/
COPY ./src/Cadastro.Domain/Cadastro.Domain.csproj ./Cadastro.Domain/
COPY ./src/Cadastro.Data/Cadastro.Data.csproj ./Cadastro.Data/
COPY ./src/Cadastro.Configuracoes/Cadastro.Configuracoes.csproj ./Cadastro.Configuracoes/

RUN dotnet restore ./Cadastro.WorkerService/Cadastro.WorkerService.csproj

COPY ./src/Cadastro.WorkerService/ ./Cadastro.WorkerService/
COPY ./src/Cadastro.Configuracoes/ ./Cadastro.Configuracoes/
COPY ./src/Cadastro.Domain/ ./Cadastro.Domain/
COPY ./src/Cadastro.Data/ ./Cadastro.Data/
RUN dotnet build ./Cadastro.WorkerService/Cadastro.WorkerService.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./Cadastro.WorkerService/Cadastro.WorkerService.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cadastro.WorkerService.dll"]

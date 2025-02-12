FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Configurar locale para pt_BR.UTF-8
RUN apt-get update && apt-get install -y locales vim &&\
    sed -i 's/# pt_BR.UTF-8 UTF-8/pt_BR.UTF-8 UTF-8/g' /etc/locale.gen &&\
    locale-gen &&\
    update-locale LANG=pt_BR.UTF-8

ENV LANG=pt_BR.UTF-8
ENV LC_ALL=pt_BR.UTF-8

RUN apt-get update && apt-get install -y mupdf-tools git vim sqlite3 &&\
    dotnet tool install csharpier -g &&\
    dotnet tool install dotnet-ef -g

# Copiar csproj e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Build da aplicacao
COPY . ./
RUN dotnet publish -c Release -o out

# Build da imagem
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Configurar locale para pt_BR.UTF-8
RUN apt-get update && apt-get install -y locales vim &&\
    sed -i 's/# pt_BR.UTF-8 UTF-8/pt_BR.UTF-8 UTF-8/g' /etc/locale.gen &&\
    locale-gen &&\
    update-locale LANG=pt_BR.UTF-8

ENV LANG=pt_BR.UTF-8
ENV LC_ALL=pt_BR.UTF-8

RUN apt-get update && apt-get install -y mupdf-tools

COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Stocks.dll"]

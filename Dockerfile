FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY Master40.sln ./Master40.sln
COPY . ./
# COPY program.tests/program.tests.csproj ./program.tests/program.tests.csproj
RUN dotnet restore

# copy everything else and build
COPY . ./
# RUN dotnet test program.tests -c debug
# ENTRYPOINT ["dotnet", "test --filter Master40.XUnitTest.Online"]
ENTRYPOINT ["./"]
# RUN dotnet publish program -c Release -o /app/out

# build runtime image
# FROM mcr.microsoft.com/dotnet/core/runtime:3.1
# WORKDIR /app
# COPY --from=build-env /app/out ./

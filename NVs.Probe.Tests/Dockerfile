FROM mcr.microsoft.com/dotnet/sdk:3.1
WORKDIR /src
COPY ["NVs.Probe.Tests/NVs.Probe.Tests.csproj", "NVs.Probe.Tests/"]
COPY ["NVs.Probe/NVs.Probe.csproj", "NVs.Probe/"]
RUN dotnet restore "NVs.Probe.Tests/NVs.Probe.Tests.csproj"
COPY . .
WORKDIR "/src/NVs.Probe.Tests"
RUN dotnet test --filter Category=Linux

ENTRYPOINT ["dotnet", "test --filter Category=Linux"]
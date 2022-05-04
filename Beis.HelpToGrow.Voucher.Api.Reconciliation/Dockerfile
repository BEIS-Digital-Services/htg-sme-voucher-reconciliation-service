FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Beis.HelpToGrow.Voucher.Api.Reconciliation/Beis.HelpToGrow.Voucher.Api.Reconciliation.csproj","Beis.HelpToGrow.Voucher.Api.Reconciliation/"]
RUN dotnet restore "Beis.HelpToGrow.Voucher.Api.Reconciliation/Beis.HelpToGrow.Voucher.Api.Reconciliation.csproj"
COPY . .
WORKDIR "/src/Beis.HelpToGrow.Voucher.Api.Reconciliation"
RUN dotnet publish "Beis.HelpToGrow.Voucher.Api.Reconciliation.csproj" -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Beis.HelpToGrow.Voucher.Api.Reconciliation.dll"]
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Beis.HelpToGrow.Voucher.API.Reconciliation/Beis.HelpToGrow.Voucher.API.Reconciliation.csproj","Beis.HelpToGrow.Voucher.API.Reconciliation/"]
RUN dotnet restore "Beis.HelpToGrow.Voucher.API.Reconciliation/Beis.HelpToGrow.Voucher.API.Reconciliation.csproj"
COPY . .
WORKDIR "/src/Beis.HelpToGrow.Voucher.API.Reconciliation"
RUN dotnet publish "Beis.HelpToGrow.Voucher.API.Reconciliation.csproj" -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Beis.HelpToGrow.Voucher.API.Reconciliation.dll"]
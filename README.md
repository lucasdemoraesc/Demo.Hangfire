
# Demo.Hangfire
C# | Projeto para estudo da biblioteca Hangfire com dotnet

## Etapas
1. Instalar pacotes necessários:
	- Newtonsoft.Json
	- Hangfire.Core
	- Hangfire.Storage.SQLite
	- Hangfire.AspNetCore
2. Adicionar e configurar o serviço do Hangfire no método ConfigureServices
```csharp
services.AddHangfire(configuration  =>  configuration
	.UseRecommendedSerializerSettings()
	.UseSQLiteStorage());
services.AddHangfireServer();
```
... continua
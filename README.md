# Solución de Inventario - Full Stack .NET

Una solución completa de inventario multi-capa con seguridad OWASP implementada en todo el stack.

## 📋 Características

### Modelos de Datos
- **Productos**: Gestión de artículos con SKU único, precio y cantidad
- **Categorías**: Clasificación jerárquica de productos
- **Stock**: Control de inventario por ubicación de almacén con auditoría

### Seguridad OWASP
- Validación de entrada en frontend y backend
- Protección contra inyección SQL (EF Core parametrizado)
- Prevención de XSS con CSP
- CORS restrictivo
- Rate limiting (100 req/min)
- HTTPS obligatorio con HSTS
- Manejo centralizado de errores sin exposición de detalles
- Security headers completos

## 🏗️ Arquitectura

```
InventorySolution/
├── InventoryBackend/              # API REST ASP.NET Core
│   ├── Controllers/               # Controladores API
│   ├── Services/                  # Lógica de negocio
│   ├── Repositories/              # Acceso a datos
│   ├── Models/                    # Entidades de dominio
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Data/                      # DbContext y migraciones
│   └── Middleware/                # Middleware de seguridad
├── InventoryBackend.Tests/        # Pruebas unitarias
├── InventoryFrontEnd/             # UI Blazor WebAssembly
│   ├── Pages/                     # Componentes Razor
│   ├── Services/                  # Servicios HTTP
│   └── Models/                    # DTOs frontend
├── InventoryDatabase/             # Proyecto SQL Server
└── SECURITY.md                    # Documentación de seguridad
```

## 🚀 Requisitos

- .NET SDK 10.0+
- SQL Server 2019 o superior (LocalDB o expresión)
- Visual Studio Code o Visual Studio 2022
- Node.js (opcional, para tooling adicional)

## 📦 Instalación

### 1. Clonar el repositorio

```bash
git clone <repository-url>
cd InventorySolution
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar la base de datos

Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "InventoryDb": "Server=(localdb)\\mssqllocaldb;Database=InventoryDB;Trusted_Connection=true;"
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:5002"]
  }
}
```

### 4. Crear la base de datos

```bash
cd InventoryBackend
dotnet ef database update
cd ..
```

## 🎯 Ejecución

### Backend API

```bash
cd InventoryBackend
dotnet run
# API disponible en: https://localhost:5001/swagger
```

### Frontend

```bash
cd InventoryFrontEnd
dotnet run
# UI disponible en: https://localhost:5002
```

### Pruebas

```bash
dotnet test
# Resultado esperado: 36 tests correctos
```

## 📚 Endpoints API

### Productos

```http
GET    /api/products              # Listar todos
GET    /api/products/{id}         # Obtener uno
POST   /api/products              # Crear
PUT    /api/products/{id}         # Actualizar
DELETE /api/products/{id}         # Eliminar
```

### Categorías

```http
GET    /api/categories            # Listar todos
GET    /api/categories/{id}       # Obtener uno
POST   /api/categories            # Crear
PUT    /api/categories/{id}       # Actualizar
DELETE /api/categories/{id}       # Eliminar
```

### Stock

```http
GET    /api/stocks                # Listar todos
GET    /api/stocks/{id}           # Obtener uno
GET    /api/stocks/product/{productId}  # Por producto
POST   /api/stocks                # Crear
PUT    /api/stocks/{id}           # Actualizar
POST   /api/stocks/{id}/audit     # Auditar
DELETE /api/stocks/{id}           # Eliminar
```

## 🧪 Pruebas

### Ejecutar todas las pruebas

```bash
dotnet test InventoryBackend.Tests
```

### Resultado esperado

```
Pruebas correctas! - Errores: 0, Superado: 36, Total: 36
```

### Cobertura

```bash
dotnet test InventoryBackend.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🔒 Seguridad

Ver [SECURITY.md](SECURITY.md) para documentación completa de controles OWASP.

### Validación de Entrada

Todos los DTOs incluyen validación:

```csharp
[Required]
[StringLength(120, MinimumLength = 2)]
public string Name { get; set; }

[Range(0.0, 1000000.0)]
public decimal UnitPrice { get; set; }
```

### Protección CORS

```csharp
policy.WithOrigins(allowedOrigins)
      .AllowAnyHeader()
      .WithMethods("GET", "POST", "PUT", "DELETE")
      .AllowCredentials();
```

### Rate Limiting

- 100 requests por minuto
- 10 solicitudes en cola de espera
- Status 429 cuando se excede

### Headers de Seguridad

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'none'; ...
Strict-Transport-Security: max-age=31536000; ...
```

## 📊 Estructura de Datos

### Producto

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }           // Máx 120 chars
    public string Sku { get; set; }            // Único, alphanumérco
    public string? Description { get; set; }   // Máx 500 chars
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? CategoryId { get; set; }       // FK a Category
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    public Category? Category { get; set; }
    public ICollection<Stock> Stocks { get; set; }
}
```

### Categoría

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }           // Máx 100 chars, único
    public string? Description { get; set; }   // Máx 500 chars
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    public ICollection<Product> Products { get; set; }
}
```

### Stock

```csharp
public class Stock
{
    public int Id { get; set; }
    public int ProductId { get; set; }         // FK a Product
    public string WarehouseLocation { get; set; } // Máx 50 chars
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int MinimumThreshold { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastAuditedAtUtc { get; set; }
    
    public Product? Product { get; set; }
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "InventoryDb": "..."
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:5002"]
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "InventoryDb": "Server=production-server;Database=InventoryDB;User Id=app_user;Password=...;Encrypt=true;"
  }
}
```

## 📝 Archivos Importantes

- `Directory.Packages.props` - Versionamiento centralizado de paquetes
- `NuGet.config` - Configuración de seguridad de fuentes NuGet
- `api.http` - Colección de requests para probar API (VS Code REST Client)
- `SECURITY.md` - Documentación completa de controles OWASP

## 🐛 Solución de Problemas

### Error: "Could not open a connection to the database"

```bash
# Actualizar conexión en appsettings.json
# Verificar que SQL Server está ejecutándose
```

### Error en migrations

```bash
cd InventoryBackend
dotnet ef database drop
dotnet ef database update
```

### Pruebas fallando

```bash
# Limpiar y reconstruir
dotnet clean
dotnet build
dotnet test
```

## 📚 Documentación Adicional

- [Documentación ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Blazor WebAssembly](https://docs.microsoft.com/aspnet/core/blazor)
- [OWASP Top 10](https://owasp.org/Top10/)

## 📄 Licencia

MIT License

## ✍️ Autor

Copilot SDK para VS Code

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## ⚠️ Seguridad

Lea [SECURITY.md](SECURITY.md) para más información sobre los controles de seguridad implementados y recomendaciones de producción.

---

**Última actualización:** 2024-11-11
**Versión:** 1.0

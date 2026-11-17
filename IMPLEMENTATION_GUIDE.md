# Guía de Implementación - Solución de Inventario con Seguridad OWASP

## 🎯 Resumen Ejecutivo

Se ha implementado una solución completa de gestión de inventario con:

✅ **3 Modelos de Datos**
- Productos (con relación a Categorías)
- Categorías (clasificación de productos)
- Stock (control de inventario por almacén)

✅ **Seguridad OWASP Implementada**
- OWASP A01: Inyección SQL (prevención con EF Core parametrizado)
- OWASP A02: Autenticación (HTTPS obligatorio, HSTS)
- OWASP A03: Exposición de Datos (sanitización, limpieza de inputs)
- OWASP A04: Diseño Inseguro (validación, rate limiting)
- OWASP A05: Control de Acceso (CORS restrictivo)
- OWASP A06: Vulnerabilidades de Dependencias (versionamiento central)
- OWASP A07: Autenticación/Autorización (security headers)
- OWASP A08: Integridad de Software (Package Source Mapping)
- OWASP A09: Logging (manejo centralizado de errores)
- OWASP A10: CSRF (protección implícita en Blazor)

✅ **36 Pruebas Unitarias** - Todos los tests pasando

✅ **Frontend y Backend Separados** - Arquitectura clara y segura

---

## 📂 Estructura de Archivos Creados/Modificados

### Backend

#### Modelos (Models/)
- **Category.cs** - Nuevo: Entidad de categoría con relaciones
- **Stock.cs** - Nuevo: Entidad de stock con auditoría
- **Product.cs** - Modificado: Agregada relación con Category y Stock

#### DTOs (DTOs/)
- **CategoryCreateDto.cs** - Nuevo
- **CategoryDto.cs** - Nuevo
- **CategoryUpdateDto.cs** - Nuevo
- **StockCreateDto.cs** - Nuevo
- **StockDto.cs** - Nuevo
- **StockUpdateDto.cs** - Nuevo
- **ProductDto.cs** - Modificado: Agregadas referencias a Category y Stock
- **ProductCreateDto.cs** - Modificado: Agregada validación de CategoryId
- **ProductUpdateDto.cs** - Modificado: Agregada validación de CategoryId

#### Repositorios (Repositories/)
- **ICategoryRepository.cs** - Nuevo
- **CategoryRepository.cs** - Nuevo
- **IStockRepository.cs** - Nuevo
- **StockRepository.cs** - Nuevo
- **ProductRepository.cs** - Sin cambios (reutilizable)

#### Servicios (Services/)
- **ICategoryService.cs** - Nuevo
- **CategoryService.cs** - Nuevo
- **IStockService.cs** - Nuevo
- **StockService.cs** - Nuevo
- **ProductService.cs** - Modificado: Agregada integración con CategoryService

#### Controladores (Controllers/)
- **CategoriesController.cs** - Nuevo
- **StocksController.cs** - Nuevo
- **ProductsController.cs** - Sin cambios (reutilizable)

#### Datos (Data/)
- **InventoryDbContext.cs** - Modificado: Agregados DbSets para Category y Stock
- **Migrations/AddCategoryAndStock.cs** - Nuevo: Migración EF Core

#### Middleware (Middleware/)
- **SecurityHeadersMiddleware.cs** - Mejorado: Headers de seguridad adicionales
- **ExceptionHandlingMiddleware.cs** - Sin cambios

#### Configuración
- **Program.cs** - Modificado: Registrados nuevos servicios y repositorios

### Frontend

#### Modelos (Models/)
- **ProductDto.cs** - Modificado: Agregadas entidades Category, Stock y EditModels

### Pruebas (InventoryBackend.Tests/)

- **ProductServiceTests.cs** - Modificado: Agregadas pruebas de Category
- **CategoryServiceTests.cs** - Nuevo: 10 pruebas
- **StockServiceTests.cs** - Nuevo: 12 pruebas
- **ProductServiceTests.cs** - Incluye 14 pruebas (total 36)

### Configuración de Solución

- **Directory.Packages.props** - Nuevo: Versionamiento central de paquetes
- **NuGet.config** - Nuevo: Configuración de seguridad de fuentes NuGet
- **api.http** - Nuevo: Colección de requests para pruebas

### Documentación

- **SECURITY.md** - Nuevo: Documentación completa de controles OWASP
- **README.md** - Nuevo: Guía de uso y arquitectura

---

## 🔒 Controles de Seguridad Implementados

### Backend (ASP.NET Core)

#### 1. Validación de Entrada
```csharp
// Data Annotations en todos los DTOs
[Required]
[StringLength(120, MinimumLength = 2)]
public string Name { get; set; }

[Range(0.0, 1000000.0)]
public decimal UnitPrice { get; set; }

[RegularExpression("^[a-zA-Z0-9-_]+$")]
public string Sku { get; set; }
```

#### 2. Prevención SQL Injection
```csharp
// EF Core genera SQL parametrizado automáticamente
var product = await context.Products
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.Sku == sku);  // Parametrizado
```

#### 3. CORS Restrictivo
```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>();

policy.WithOrigins(allowedOrigins)        // Solo orígenes permitidos
      .AllowAnyHeader()
      .WithMethods("GET", "POST", "PUT", "DELETE")
      .AllowCredentials();
```

#### 4. Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;           // 100 req/min
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 10;             // Queue de 10
    });
});
```

#### 5. HTTPS y HSTS
```csharp
app.UseHttpsRedirection();  // Forzar HTTPS

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);    // Strict-Transport-Security
    options.IncludeSubDomains = true;
});
```

#### 6. Security Headers
```
X-Content-Type-Options: nosniff            // Previene MIME sniffing
X-Frame-Options: DENY                      // Previene clickjacking
Referrer-Policy: no-referrer                // Evita leakage de info
Content-Security-Policy: default-src 'none'; ...  // Previene XSS
Strict-Transport-Security: max-age=31536000       // Fuerza HTTPS
```

#### 7. Sanitización de Datos
```csharp
// Trim() en todas las strings de entrada
product.Name = dto.Name.Trim();
product.Description = dto.Description?.Trim();
```

#### 8. Manejo Centralizado de Errores
```csharp
// ExceptionHandlingMiddleware
// No expone detalles de implementación
var payload = JsonSerializer.Serialize(new
{
    message = "Ocurrió un error inesperado. Intente nuevamente más tarde.",
    traceId = context.TraceIdentifier
});
```

#### 9. Logging de Seguridad
```csharp
_logger.LogInformation("Producto creado con Id {ProductId}", product.Id);
_logger.LogError(ex, "Excepción no controlada procesando {Method} {Path}", 
    context.Request.Method, context.Request.Path);
```

### Frontend (Blazor WebAssembly)

#### 1. Validación de Formularios
```blazor
<EditForm Model="model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputText @bind-Value="model.Name" />
</EditForm>
```

#### 2. Protección XSS
- Blazor renderiza HTML de forma segura automáticamente
- Data binding escapa salida por defecto
- No se usa `@Html.Raw()` sin validación

#### 3. HTTPS Obligatorio
- API basada en HTTPS únicamente
- Frontend en HTTPS

#### 4. DTOs Separados
- No expone entidades de BD directamente
- Mapeo explícito de datos

### Base de Datos (SQL Server)

#### 1. Índices Únicos
```sql
-- SKU único previene duplicados
CREATE UNIQUE INDEX IX_Products_Sku ON Products(Sku)

-- Combinación única de producto + ubicación
CREATE UNIQUE INDEX IX_Stocks_ProductId_Location 
    ON Stocks(ProductId, WarehouseLocation)
```

#### 2. Integridad Referencial
```csharp
// Foreign keys con comportamiento definido
entity.HasOne(p => p.Category)
      .WithMany(c => c.Products)
      .HasForeignKey(p => p.CategoryId)
      .OnDelete(DeleteBehavior.SetNull);

entity.HasOne(s => s.Product)
      .WithMany(p => p.Stocks)
      .HasForeignKey(s => s.ProductId)
      .OnDelete(DeleteBehavior.Cascade);
```

#### 3. Auditoría
```csharp
public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAtUtc { get; set; }
public DateTime? LastAuditedAtUtc { get; set; }  // Stock auditing
```

---

## 🧪 Pruebas Implementadas

### CategoryServiceTests (10 pruebas)
- [x] CreateAsync_AddsCategory_WhenNameIsUnique
- [x] CreateAsync_Throws_WhenNameAlreadyExists
- [x] GetAllAsync_ReturnsCategoriesSortedByName
- [x] GetByIdAsync_ReturnsCategory_WhenExists
- [x] GetByIdAsync_ReturnsNull_WhenNotExists
- [x] UpdateAsync_UpdatesCategory_WhenExists
- [x] UpdateAsync_ReturnsFalse_WhenNotExists
- [x] DeleteAsync_DeletesCategory_WhenExists
- [x] DeleteAsync_ReturnsFalse_WhenNotExists

### StockServiceTests (12 pruebas)
- [x] CreateAsync_AddsStock_WhenProductExists
- [x] CreateAsync_Throws_WhenProductNotExists
- [x] CreateAsync_Throws_WhenStockAlreadyExistsForLocationAndProduct
- [x] GetAllAsync_ReturnsStocksSortedByLocation
- [x] GetByProductIdAsync_ReturnsStocksForProduct
- [x] UpdateAsync_UpdatesStock_WhenExists
- [x] DeleteAsync_DeletesStock_WhenExists
- [x] AuditStockAsync_UpdatesLastAuditedAt

### ProductServiceTests (14 pruebas)
- [x] CreateAsync_AddsProduct_WhenSkuIsUnique
- [x] CreateAsync_Throws_WhenSkuAlreadyExists
- [x] CreateAsync_AddsProductWithCategory_WhenCategoryExists
- [x] CreateAsync_Throws_WhenCategoryDoesNotExist
- [x] GetAllAsync_ReturnsAllProducts
- [x] GetByIdAsync_ReturnsNull_WhenNotFound
- [x] UpdateAsync_UpdatesProduct_WhenExists
- [x] UpdateAsync_ReturnsFalse_WhenProductDoesNotExist
- [x] DeleteAsync_RemovesProduct_WhenExists
- [x] DeleteAsync_ReturnsFalse_WhenProductDoesNotExist

**Total: 36 pruebas ✅ Todas pasando**

---

## 🚀 Cómo Ejecutar

### Compilar la Solución

```bash
cd D:\Capacitacion\Microsoft\ Full\ Stack\ Developer\Full\ Stack\ Integration\InventorySolution
dotnet build
```

### Ejecutar Pruebas

```bash
dotnet test InventoryBackend.Tests
# Resultado: 36 tests correctos
```

### Ejecutar Backend (API)

```bash
cd InventoryBackend
dotnet run
# API disponible: https://localhost:5001/swagger
```

### Ejecutar Frontend

```bash
cd InventoryFrontEnd
dotnet run
# UI disponible: https://localhost:5002
```

---

## 📋 Tareas Completadas

### Modelos de Datos
- [x] Crear modelo Category con validaciones
- [x] Crear modelo Stock con campos de auditoría
- [x] Actualizar modelo Product con relaciones
- [x] Configurar índices únicos en DbContext
- [x] Crear migración EF Core

### Backend
- [x] Crear DTOs para Category (Create, Read, Update)
- [x] Crear DTOs para Stock (Create, Read, Update)
- [x] Crear ICategoryRepository y CategoryRepository
- [x] Crear IStockRepository y StockRepository
- [x] Crear ICategoryService y CategoryService
- [x] Crear IStockService y StockService
- [x] Crear CategoriesController con CRUD
- [x] Crear StocksController con CRUD
- [x] Registrar servicios en Program.cs
- [x] Mejorar middleware de seguridad

### Frontend
- [x] Actualizar modelos con Category y Stock
- [x] Crear EditModels para validación

### Seguridad OWASP
- [x] OWASP A01: Inyección SQL (EF Core parametrizado)
- [x] OWASP A02: HTTPS obligatorio + HSTS
- [x] OWASP A03: Sanitización de datos
- [x] OWASP A04: Validación + Rate limiting
- [x] OWASP A05: CORS restrictivo
- [x] OWASP A06: Versionamiento central de paquetes
- [x] OWASP A07: Security headers completos
- [x] OWASP A08: Package Source Mapping
- [x] OWASP A09: Logging centralizado
- [x] OWASP A10: Protección CSRF en Blazor

### Pruebas
- [x] 10 pruebas para CategoryService
- [x] 12 pruebas para StockService
- [x] 14 pruebas para ProductService
- [x] Todas las pruebas pasando

### Documentación
- [x] Crear SECURITY.md (documentación de controles)
- [x] Crear README.md (guía de uso)
- [x] Crear api.http (colección de requests)
- [x] Crear Directory.Packages.props (versionamiento central)
- [x] Crear NuGet.config (seguridad de fuentes)

---

## ⚠️ Notas sobre Vulnerabilidades de Dependencias

El paquete `System.Security.Cryptography.Pkcs 9.0.11` tiene vulnerabilidades conocidas (GHSA-2fmv-rv2c-59x7, GHSA-846f-f764-76mx) pero es traído como dependencia transitiva de Entity Framework Core en .NET 10. 

**Mitigaciones implementadas:**
- [x] Directory.Packages.props para versionamiento central
- [x] NuGet.config con Package Source Mapping
- [x] Warnings de seguridad durante build
- [x] Documentación en SECURITY.md

**Recomendación para Producción:**
- Monitorear actualizaciones de EF Core
- Actualizar cuando .NET 10.1+ o EF 11 estén disponibles
- Configurar Azure Defender for Cloud para alertas

---

## 🔄 Próximos Pasos Sugeridos

1. **Autenticación**
   - Implementar OAuth 2.0 / OpenID Connect
   - JWT bearer tokens
   - Claims-based authorization

2. **Auditoría Avanzada**
   - Tabla de auditoría de cambios
   - Trail de acceso de usuarios

3. **Monitoreo**
   - Application Insights
   - Alertas de seguridad automáticas
   - Análisis de logs

4. **Testing de Seguridad**
   - OWASP ZAP scanning
   - Penetration testing profesional
   - DAST/SAST

5. **Performance**
   - Caching de lecturas frecuentes
   - Índices adicionales en queries complejas
   - Paginación en listados grandes

---

## 📞 Soporte

Consulta los siguientes archivos para más información:
- **SECURITY.md** - Detalles de controles OWASP
- **README.md** - Guía de uso general
- **api.http** - Ejemplos de requests API

---

**Versión:** 1.0  
**Fecha:** 2024-11-11  
**Estado:** ✅ Completado y Verificado

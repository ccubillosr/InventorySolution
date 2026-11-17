# 📚 Guía de Mantenibilidad y Estándares de Código

**Última Actualización:** Noviembre 2026

---

## 🎯 Principios de Diseño Implementados

### 1. Separación de Responsabilidades (SoC)

**Estructura Actual:**
```
Controllers (API Endpoints)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
DbContext (EF Core)
    ↓
Database
```

**Beneficio:** Cada capa tiene una responsabilidad clara, facilitando testing y mantenimiento.

---

### 2. Repository Pattern

**Interfaz clara:**
```csharp
public interface IProductRepository
{
    // Lectura (AsNoTracking)
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken);
    
    // Actualización/Eliminación (Tracked)
    Task<Product?> GetByIdTrackedAsync(int id, CancellationToken);
    
    // Mutaciones
    Task AddAsync(Product product, CancellationToken);
    void Remove(Product product);
    Task<bool> SaveChangesAsync(CancellationToken);
}
```

**Ventajas:**
- Abstracción de EF Core
- Fácil de testear con mocks
- Cambios en BD no afectan servicios

---

### 3. Validación Temprana

**En ProductService.UpdateAsync:**
```csharp
// Validación 1: SKU único (solo si cambió)
if (dto.Sku != product.Sku && 
    await _repository.SkuExistsAsync(dto.Sku, cancellationToken))
{
    throw new ConflictException(...);  // Error específico
}

// Validación 2: Categoría existe (solo si cambió)
if (dto.CategoryId != product.CategoryId && dto.CategoryId.HasValue && 
    !await _categoryRepository.ExistsAsync(dto.CategoryId.Value, cancellationToken))
{
    throw new ConflictException(...);
}
```

**Princip**: Validar y fallar rápido, evitando cambios parciales.

---

### 4. Documentación XML

**Todas las clases públicas tienen:**
```csharp
/// <summary>
/// Descripción clara de qué hace.
/// </summary>
public sealed class ProductService : IProductService
{
    /// <summary>
    /// Obtiene todos los productos con categoría y stocks incluidos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de productos no rastreados por EF Core.</returns>
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Implementación
    }
}
```

**Beneficios:**
- Documentación automática (IntelliSense)
- Facilita onboarding de nuevos desarrolladores
- Genera documentación automática (Swagger, etc.)

---

## 🔍 Convenciones de Código Implementadas

### 1. Extensiones de String

```csharp
// Siempre usar SafeTrim() para evitar null exceptions
var name = dto.Name.SafeTrim()!;      // ✅ Correcto
var name = dto.Name?.Trim() ?? "";    // ❌ Evitar

// SafeTrim retorna null si input es null/empty, evitando allocaciones
public static string? SafeTrim(this string? value)
{
    return string.IsNullOrEmpty(value) ? value : value.Trim();
}
```

### 2. DTOs a Entidades

```csharp
// Usar MapToDto() para conversiones null-safe
var productDto = product.MapToDto(ToDto);  // ✅ Correcto

// MapToDto maneja null automáticamente
public static TDto? MapToDto<T, TDto>(this T? entity, Func<T, TDto> mapper) 
    where T : class
{
    return entity is null ? default : mapper(entity);
}
```

### 3. Conversiones a IReadOnlyList

```csharp
// Usar AsReadOnlyList() para evitar double-materialization
return products.Select(ToDto).AsReadOnlyList();  // ✅ Correcto
return products.Select(ToDto).ToList();          // ❌ Evitar

// AsReadOnlyList no materializa dos veces
public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> collection)
{
    return collection is IReadOnlyList<T> roList 
        ? roList 
        : collection.ToList().AsReadOnly();
}
```

### 4. EF Core Queries

```csharp
// Lectura: Usar AsNoTracking()
var products = await _context.Products
    .AsNoTracking()                  // ✅ Lectura, no necesita tracking
    .Include(p => p.Category)        // Include lo que necesitas
    .Include(p => p.Stocks)
    .ToListAsync();

// Actualización: NO usar AsNoTracking()
var product = await _context.Products
    .Include(p => p.Category)        // Include solo lo necesario
    .FirstOrDefaultAsync(p => p.Id == id);
    // Sin AsNoTracking() para que EF Core rastree cambios
```

### 5. Logging Estructurado

```csharp
// ✅ Correcto: Parámetros nombrados
_logger.LogInformation(
    "Producto creado: Id={ProductId}, Sku={Sku}", 
    product.Id, product.Sku);

// ❌ Evitar: Interpolación de strings
_logger.LogInformation($"Producto creado: {product.Id}");
```

---

## ⚡ Patrones de Performance

### 1. N+1 Queries Prevention

```csharp
// ❌ PROBLEMA: N+1 queries
public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct)
{
    var products = await _repository.GetAllAsync(ct);  // 1 query
    return products.Select(p => new ProductDto 
    { 
        Stocks = p.Stocks.Select(...).ToList()  // N queries! 
    }).ToList();
}

// ✅ SOLUCIÓN: Include en repository
public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
{
    return await _context.Products
        .AsNoTracking()
        .Include(p => p.Category)    // 1 query para Category
        .Include(p => p.Stocks)      // 1 query para Stocks
        .ToListAsync(ct);             // Total: 1-2 queries
}
```

### 2. Tracked vs Non-Tracked

```csharp
// Patrón recomendado en Repository
public interface IProductRepository
{
    // Para LECTURA
    Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Products
            .AsNoTracking()  // ✅ No rastrear (más rápido)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
    
    // Para ACTUALIZACIÓN/ELIMINACIÓN
    Task<Product?> GetByIdTrackedAsync(int id, CancellationToken ct)
    {
        return await _context.Products
            // SIN AsNoTracking() ✅ Rastrear cambios
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}

// Uso en Service
public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct)
{
    var product = await _repository.GetByIdTrackedAsync(id, ct);  // ✅ Tracked
    product.Name = dto.Name.SafeTrim()!;
    await _repository.SaveChangesAsync(ct);
    return true;
}
```

### 3. Validación Condicional

```csharp
// ✅ CORRECTO: Validar solo si el valor cambió
public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken ct)
{
    var category = await _repository.GetByIdTrackedAsync(id, ct);
    
    // Solo validar si el nombre CAMBIÓ
    if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name && 
        await _repository.NameExistsAsync(dto.Name, cancellationToken: ct))
    {
        throw new ConflictException("Nombre duplicado");
    }
    
    // Aplicar cambios
    if (!string.IsNullOrEmpty(dto.Name))
        category.Name = dto.Name.SafeTrim()!;
    
    await _repository.SaveChangesAsync(ct);
    return true;
}
```

---

## 🧪 Estándares de Testing

### Estructura de Tests

```csharp
public class ProductServiceTests
{
    // Setup: Crear contexto en-memoria
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }
    
    // Factory: Crear servicio con dependencias
    private static ProductService CreateService(InventoryDbContext context)
    {
        var repository = new ProductRepository(context);
        return new ProductService(
            repository, 
            new CategoryRepository(context),
            NullLogger<ProductService>.Instance);
    }
    
    // Test: Patrón Arrange-Act-Assert
    [Fact]
    public async Task CreateAsync_CreatesProduct_WhenValid()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new ProductCreateDto { Name = "Test", Sku = "SKU001", ... };
        
        // Act
        var result = await service.CreateAsync(dto);
        
        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Test", result.Name);
    }
}
```

---

## 🔐 Seguridad Implementada

### 1. Input Validation

```csharp
// DTOs con Data Annotations
public sealed record ProductCreateDto
{
    [Required, MaxLength(200)]
    public required string Name { get; init; }
    
    [Required, MaxLength(50)]
    public required string Sku { get; init; }
    
    [Range(0.01, 999999.99)]
    public required decimal UnitPrice { get; init; }
}

// Service valida antes de usar
if (await _repository.SkuExistsAsync(dto.Sku, cancellationToken))
{
    throw new ConflictException("SKU duplicado");
}
```

### 2. Parameter Binding

```csharp
// ✅ Usar parámetros SQL (previene SQL Injection)
// EF Core los maneja automáticamente
await _context.Products
    .Where(p => p.Sku == sku)  // ✅ Parametrizado
    .AnyAsync();

// ❌ NUNCA hacer string interpolation en queries
// var query = $"SELECT * WHERE Sku = '{sku}'";  // ❌ SQL Injection
```

### 3. Exception Handling

```csharp
// Excepciones personalizadas
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

// Usadas en validation
if (await _repository.SkuExistsAsync(dto.Sku, cancellationToken))
{
    throw new ConflictException($"SKU '{dto.Sku}' ya existe");
}
```

---

## 📊 Checklist de Calidad

Al agregar una nueva funcionalidad, verificar:

- [ ] **Interfaz**: Existe interfaz para cada servicio/repositorio
- [ ] **Documentación XML**: Todas las clases/métodos públicos documentados
- [ ] **DTOs**: Usando `sealed record` con data annotations
- [ ] **Validación**: Validar entrada temprano con ConflictException
- [ ] **Logging**: Logging estructurado con parámetros nombrados
- [ ] **EF Core**: Usar AsNoTracking() en lecturas, Include() correcto
- [ ] **Testing**: Tests para happy path + edge cases
- [ ] **Performance**: Evitar N+1, ToList() innecesarios
- [ ] **Seguridad**: Sin SQL Injection, input validado
- [ ] **CancellationToken**: Pasar a todas las operaciones async

---

## 🚀 Recomendaciones Futuras

### Corto Plazo (1-2 sprints)
1. Agregar índices a base de datos (ver PERFORMANCE_REVIEW.md)
2. Implementar IMemoryCache para GetAll()
3. Crear benchmark suite con BenchmarkDotNet

### Mediano Plazo (3-6 sprints)
1. Implementar Specification Pattern para queries complejas
2. Agregar auditoría (quién cambió qué y cuándo)
3. Implementar soft-delete para registros críticos

### Largo Plazo (6+ sprints)
1. Considerar CQRS si las reads/writes divergen significativamente
2. Implementar Event Sourcing para auditoría completa
3. Consideración de Read Replicas o OLAP para reports

---

## 📚 Referencias

- [EF Core Best Practices](https://learn.microsoft.com/en-us/ef/core/)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)


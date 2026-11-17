# 🚀 Guía Rápida de Inicio

## 1. Compilar la Solución

```bash
cd InventorySolution
dotnet build
```

**Resultado esperado:**
```
✅ Compilación exitosa
⚠️ 8 advertencias sobre vulnerabilidad transitiva (esperado)
❌ 0 errores
```

---

## 2. Ejecutar Pruebas Unitarias

```bash
dotnet test
```

**Resultado esperado:**
```
✅ 36/36 pruebas pasando
```

### Ejecutar pruebas específicas:

```bash
# Solo pruebas de categorías
dotnet test --filter "CategoryServiceTests"

# Solo pruebas de stocks
dotnet test --filter "StockServiceTests"

# Solo pruebas de productos
dotnet test --filter "ProductServiceTests"
```

---

## 3. Ejecutar el Backend

```bash
cd InventoryBackend
dotnet run
```

**Resultado esperado:**
```
info: Microsoft.Hosting.Lifetime
      Now listening on: https://localhost:7xxx
info: Microsoft.AspNetCore.Hosting.Diagnostics
      Request starting HTTP/2 GET https://localhost:7xxx/swagger/index.html
```

**Acceso:**
- Swagger/OpenAPI: https://localhost:7xxx/swagger
- API: https://localhost:7xxx/api/

---

## 4. Testing de API

### Opción A: Usar VS Code REST Client

1. Instalar extensión REST Client en VS Code
2. Abrir archivo `api.http`
3. Click en "Send Request" arriba de cada endpoint

### Opción B: Usar cURL

```bash
# Crear categoría
curl -X POST "https://localhost:7xxx/api/categories" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Electrónica",
    "description": "Productos electrónicos"
  }'

# Listar categorías
curl "https://localhost:7xxx/api/categories"

# Obtener categoría específica
curl "https://localhost:7xxx/api/categories/{id}"
```

### Opción C: Usar Postman

1. Importar colección desde `api.http`
2. Configurar variable `baseUrl` = `https://localhost:7xxx`
3. Ejecutar requests

---

## 5. Testing Manual de Seguridad

### Verificar Headers de Seguridad

```bash
curl -i https://localhost:7xxx/api/categories
```

**Verificar presencia de headers:**
- ✅ Strict-Transport-Security: max-age=31536000
- ✅ X-Content-Type-Options: nosniff
- ✅ X-Frame-Options: DENY
- ✅ Content-Security-Policy
- ✅ Permissions-Policy

### Verificar Rate Limiting

```bash
# Ejecutar 150 requests rápido (límite es 100/min)
for i in {1..150}; do
  curl https://localhost:7xxx/api/categories
done
```

**Resultado esperado después de ~100 requests:**
```
HTTP 429 Too Many Requests
```

### Verificar Validación de Entrada

```bash
# Intentar SKU inválido
curl -X POST "https://localhost:7xxx/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test",
    "sku": "invalid@#$%",
    "price": 10.00
  }'

# Resultado: 400 Bad Request con detalles de validación
```

---

## 6. Estructura de Carpetas

```
InventorySolution/
├── InventoryBackend/              # Backend API
│   ├── Models/                    # Modelos de datos
│   ├── DTOs/                      # Objetos de transferencia
│   ├── Services/                  # Lógica de negocio
│   ├── Repositories/              # Acceso a datos
│   ├── Controllers/               # Endpoints HTTP
│   └── Data/
│       ├── InventoryDbContext.cs
│       └── Migrations/
│
├── InventoryBackend.Tests/        # Pruebas unitarias
│   ├── CategoryServiceTests.cs
│   ├── StockServiceTests.cs
│   └── ProductServiceTests.cs
│
├── InventoryFrontEnd/             # Frontend Blazor
│   └── Models/
│       └── ProductDto.cs
│
└── Documentación/
    ├── SECURITY.md                # Guía de seguridad
    ├── README.md                  # Guía principal
    ├── IMPLEMENTATION_GUIDE.md    # Detalles técnicos
    ├── FINAL_STATUS.md            # Estado del proyecto
    ├── QUICK_START.md             # Este documento
    ├── CHANGELOG.md               # Historial
    └── api.http                   # Ejemplos de testing
```

---

## 7. Comandos Útiles de EF Core

```bash
cd InventoryBackend

# Listar migraciones
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add "NombreMigracion"

# Deshacer última migración (en memoria)
dotnet ef migrations remove

# Aplicar migraciones a BD
dotnet ef database update

# Revertir a migración anterior
dotnet ef database update "NombreMigracion"

# Ver SQL de una migración
dotnet ef migrations script "MigracionPrevía" "MigracionActual"
```

---

## 8. Variables de Entorno Típicas

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryDb;Trusted_Connection=true;"
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "Window": "00:01:00"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:3000",
      "https://localhost:7xxx"
    ]
  }
}
```

---

## 9. Solución de Problemas Comunes

### "No se puede conectar a la base de datos"
```bash
# Crear/actualizar base de datos
cd InventoryBackend
dotnet ef database update
```

### "Puerto ya está en uso"
```bash
# Cambiar puerto en launchSettings.json
# O ejecutar en puerto específico
dotnet run --urls "https://localhost:8000"
```

### "Certificado HTTPS no válido"
```bash
# En desarrollo, confiar en certificado local
dotnet dev-certs https --trust
```

### "Fallo en Entity Framework"
```bash
# Limpiar y reconstruir
dotnet ef database drop
dotnet ef database update
```

---

## 10. Verificación Final

Ejecutar este checklist antes de dar por completado:

- [ ] `dotnet build` → 0 errores
- [ ] `dotnet test` → 36/36 pasando
- [ ] `dotnet run` → Backend levanta sin errores
- [ ] Acceso a https://localhost:7xxx/swagger
- [ ] Headers de seguridad presentes en respuestas
- [ ] Rate limiting funciona (después de 100 req/min)
- [ ] Validación de DTOs funciona
- [ ] Migraciones listar correctamente
- [ ] Documentación (SECURITY.md, README.md) accesible
- [ ] api.http puede ejecutar requests exitosamente

---

## 📞 Próximos Pasos

Una vez verificado que todo funciona:

1. **Autenticación**: Implementar JWT en lugar de Anonymous
2. **Autorización**: Agregar roles y permisos
3. **Logging**: Configurar Serilog o Application Insights
4. **Frontend**: Crear componentes Blazor para UI
5. **Producción**: Seguir guía en SECURITY.md → Recomendaciones para Producción

---

**¡Listo para comenzar a desarrollar!** 🎉

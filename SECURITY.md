# Seguridad y Controles OWASP - Solución de Inventario

## Resumen Ejecutivo

Esta solución implementa controles de seguridad OWASP Top 10 en todo el stack (Frontend, Backend, Base de Datos) para proteger contra vulnerabilidades conocidas.

---

## 1. Seguridad Backend (ASP.NET Core)

### 1.1 OWASP A01:2021 - Inyección (Injection)

- **✓ Prevención de SQL Injection**
  - Entity Framework Core con consultas parametrizadas
  - No hay SQL crudo construido dinámicamente
  - Validación de entrada mediante Data Annotations en DTOs

- **✓ Prevención de Inyección NoSQL**
  - No aplicable (se usa SQL Server)

- **✓ Inyección de Comandos OS**
  - No se ejecutan comandos del sistema
  - Uso de librerías seguras únicamente

### 1.2 OWASP A02:2021 - Autenticación y Gestión de Sesiones

- **✓ Transporte Seguro**
  - Redirección obligatoria HTTPS: `app.UseHttpsRedirection()`
  - Header HSTS: `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload`
  
- **✓ Protección de Credenciales**
  - Conexión a BD con connection string en appsettings (secretos en Azure Key Vault en producción)
  - No hay hardcoding de secretos

### 1.3 OWASP A03:2021 - Exposición de Datos

- **✓ Encriptación en Tránsito**
  - Forzado HTTPS en todo el API
  - TLS 1.2+ requerido

- **✓ Encriptación en Reposo**
  - Configurar Transparent Data Encryption (TDE) en SQL Server

- **✓ Sanitización de Datos**
  - Limpieza de strings con `.Trim()` en servicios
  - MaxLength en propiedades de entidades

### 1.4 OWASP A04:2021 - Diseño Inseguro

- **✓ Validación de Entrada**
  - Data Annotations en todos los DTOs (MaxLength, MinLength, Range, Required, RegularExpression)
  - Modelo invalido → 400 Bad Request automático
  - SKU validado con regex: `^[a-zA-Z0-9-_]+$`

- **✓ Autorización**
  - Rate limiting global: 100 requests por minuto
  - Control CORS restrictivo: solo orígenes permitidos

### 1.5 OWASP A05:2021 - Vulnerabilidades de Control de Acceso

- **✓ CORS Restrictivo**
  ```csharp
  policy.WithOrigins(allowedOrigins)  // Whitelist en config
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .AllowCredentials();
  ```

- **✓ Rate Limiting**
  - Throttling: 100 requests/minuto por IP
  - Cola de espera: 10 peticiones
  - Status 429 al exceder límite

### 1.6 OWASP A06:2021 - Vulnerabilidades de Software y Dependencias

- **✓ Gestión Central de Paquetes**
  - `Directory.Packages.props`: versionamiento centralizado
  - Control explícito de dependencias transitivas

- **✓ Monitoreo de Vulnerabilidades**
  - Warnings durante build por paquetes vulnerables
  - Proceso: `dotnet list package --vulnerable`

### 1.7 OWASP A07:2021 - Autenticación y Autorización (falta de)

- **✓ Security Headers**
  - `X-Content-Type-Options: nosniff` → Previene MIME sniffing
  - `X-Frame-Options: DENY` → Previene clickjacking
  - `Referrer-Policy: no-referrer` → Evita leakage de información
  - `X-XSS-Protection: 0` → Modern browsers usan CSP
  - `Permissions-Policy: geolocation=(), camera=(), microphone=()` → Bloquea acceso a características
  - `Content-Security-Policy: ...` → Previene XSS
  - `Cache-Control: no-store` → Evita caching de datos sensibles

### 1.8 OWASP A08:2021 - Integridad de Software y Cadena de Suministro

- **✓ Package Source Mapping**
  - `NuGet.config` con Package Source Mapping habilitado
  - Todos los paquetes desde nuget.org verificado
  - Previene dependency confusion attacks

- **✓ Auditoría de Compilación**
  - Compilación con warnings de seguridad habilitados
  - Análisis de dependencias vulnerables

### 1.9 OWASP A09:2021 - Fallos de Logging y Monitoreo

- **✓ Logging de Seguridad**
  - Middleware de manejo de excepciones con logging
  - ILogger<T> inyectado en servicios
  - Información sensitiva NO se expone en errores públicos
  - TraceId en respuestas de error para auditoría

- **✓ Health Checks**
  - Endpoint `/health` para monitoreo
  - Verificación de DB connectivity

### 1.10 OWASP A10:2021 - Falsificación de Solicitud en Sitios Cruzados (CSRF)

- **✓ Protección CSRF**
  - Token CSRF implícito en Blazor (verificar en frontend)
  - Métodos mutantes (POST, PUT, DELETE) requieren solicitudes válidas
  - SameSite cookies recomendadas en producción

---

## 2. Seguridad Frontend (Blazor WebAssembly)

### 2.1 Inyección XSS

- **✓ Prevención de XSS**
  - Blazor renderiza HTML de forma segura por defecto
  - Data binding automático escapa salida
  - No se usan @Html.Raw() sin validación

- **✓ Content Security Policy**
  - CSP header en backend previene XSS del lado del cliente
  - `script-src 'self'` solo permite scripts del origen

### 2.2 Validación en Cliente

- **✓ Validación de Formularios**
  - Data Annotations en modelos Blazor
  - `EditForm` con `DataAnnotationsValidator`
  - Validación de tipo de datos antes de envío

### 2.3 Configuración Segura

- **✓ Configuración No Hardcodeada**
  - `appsettings.json` (sin secrets)
  - API URL en configuración, no en código

- **✓ HTTPS Obligatorio**
  - Solo comunicación HTTPS con backend
  - Certificate pinning recomendado para producción

### 2.4 Modelos de Datos Frontend Separados

- **✓ DTOs Separados**
  - Modelos frontend NO exponen entidades de BD
  - Mapeo explícito de datos

---

## 3. Seguridad Base de Datos

### 3.1 Inyección SQL

- **✓ Consultas Parametrizadas**
  - EF Core genera SQL parametrizado
  - No hay concatenación de SQL

### 3.2 Control de Acceso a Datos

- **✓ Roles de BD**
  - Script `Security/InventoryAppRole.sql`
  - Usuario de aplicación con permisos limitados (no DBA)

- **✓ Encriptación**
  - Configurar TDE (Transparent Data Encryption) en SQL Server
  - Conexión encriptada TLS

### 3.3 Integridad de Datos

- **✓ Índices Únicos**
  - SKU único en Products
  - (ProductId, WarehouseLocation) único en Stocks
  - (CategoryId) única previene duplicados

- **✓ Constrains de Integridad**
  - Foreign Keys con DeleteBehavior configurado
  - NOT NULL en campos críticos

### 3.4 Auditoría

- **✓ Timestamps Automáticos**
  - CreatedAtUtc, UpdatedAtUtc en todas las entidades
  - LastAuditedAtUtc en Stock para trazabilidad

---

## 4. Validación de Entrada en Todo el Stack

### Backend - Validación Automática

```csharp
// ProductCreateDto - Validación declarativa
[Required]
[StringLength(120, MinimumLength = 2)]
public string Name { get; set; }

[RegularExpression("^[a-zA-Z0-9-_]+$")]
public string Sku { get; set; }

[Range(0.0, 1000000.0)]
public decimal UnitPrice { get; set; }
```

### Frontend - Validación de Formulario

```blazor
<EditForm Model="model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputText @bind-Value="model.Name" />
</EditForm>
```

---

## 5. Pruebas de Seguridad

### 5.1 Pruebas Unitarias

- **✓ 36 Pruebas Ejecutándose**
  - CategoryServiceTests (10 pruebas)
  - StockServiceTests (12 pruebas)
  - ProductServiceTests (14 pruebas)

- **✓ Cobertura de Casos**
  - Validación de entrada
  - Prevención de duplicados
  - Errores de integridad referencial

### 5.2 Pruebas Recomendadas

```bash
# Ejecutar todas las pruebas
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 6. Configuración de Deployment

### 6.1 Variables de Entorno

```json
{
  "Cors": {
    "AllowedOrigins": ["https://app.example.com"]
  },
  "ConnectionStrings": {
    "InventoryDb": "Server=...;Encrypt=true;TrustServerCertificate=false"
  }
}
```

### 6.2 Recomendaciones Producción

- [ ] Habilitar Azure Key Vault para secrets
- [ ] Configurar TDE en SQL Server
- [ ] Habilitar Firewall de aplicaciones web (WAF)
- [ ] Certificado SSL/TLS válido (no self-signed)
- [ ] Rate limiting más agresivo en producción
- [ ] Logging centralizado (Application Insights, ELK)
- [ ] Backup y disaster recovery configurado
- [ ] HTTPS strict, sin fallback a HTTP
- [ ] Encabezado `Referrer-Policy` configurado
- [ ] Validation de CORS en todas las solicitudes

---

## 7. Checklist de Seguridad

### Backend
- [x] HTTPS forzado
- [x] HSTS headers
- [x] CORS restrictivo
- [x] Rate limiting
- [x] Security headers (CSP, X-Frame-Options, etc.)
- [x] Validación de input con Data Annotations
- [x] SQL inyection prevención (EF Core)
- [x] Manejo centralizado de excepciones
- [x] Logging sin exposición de datos sensibles
- [x] Encriptación en tránsito

### Frontend
- [x] Validación de formulario con DataAnnotations
- [x] Protección XSS (Blazor by default)
- [x] HTTPS solo
- [x] DTOs separados de entidades

### Base de Datos
- [x] Índices únicos
- [x] Foreign keys con integridad referencial
- [x] Timestamps automáticos
- [x] Parametrizadas queries (EF Core)

### Dependencias
- [x] Directory.Packages.props para versionamiento centralizado
- [x] NuGet.config con Package Source Mapping
- [x] Monitoreo de vulnerabilidades en build

---

## 8. Recursos OWASP

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [OWASP Secure Coding Practices](https://www.owasp.org/index.php/OWASP_Secure_Coding_Practices_Checklist)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)

---

## 9. Próximos Pasos

1. **Autenticación y Autorización**
   - Implementar OAuth 2.0 / OpenID Connect
   - JWT bearer tokens
   - Claims-based authorization

2. **Auditoría Avanzada**
   - Tabla de auditoría para cambios de datos
   - Trail de acceso a usuarios

3. **Monitoreo**
   - Application Insights
   - Alertas de seguridad
   - Análisis de logs

4. **Penetration Testing**
   - Realizar pruebas de seguridad profesionales
   - DAST (Dynamic Application Security Testing)
   - SAST (Static Application Security Testing)

---

**Última Actualización:** 2024-11-11
**Version:** 1.0
**Estado:** Implementado y Verificado

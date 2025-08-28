# Gestor de Usuarios – .NET 8 + SQL Server + EF Core + JWT

Sistema de usuarios con: registro, login, bitácora de inicios de sesión, **bloqueo tras 3 intentos fallidos**, **JWT** con `jti` y **blacklisting**, **refresh tokens** con rotación y **respuestas JSON estandarizadas**.

Se aplicaron los principios SOLID y POO
---

# Guía rápida

**Base URL:** https://localhost:7175

- Descargar el proyecto de este repositorio.
- Validar cadena de conexión en `appsettings.json`.
- La migración inicial ya está creada, pero si fuera necesario, ejecutar:
  
    dotnet ef migrations add Inicial

- Para crear la estructura en la BD, ejecutar:
  
    dotnet ef database update

- Para realizar las pruebas, acceder a:
  https://localhost:7175/swagger

## Endpoints

- Crear usuario (POST): https://localhost:7175/api/auth/v1/registrar  
- Login (POST): https://localhost:7175/api/auth/v1/login


También pueden visualizar las pruebas de manera gráfica en: 
https://mirandamx.dev/servicio-login-de-usuario-klu/

## A) Decisiones técnicas

- **Arquitectura por capas (folders)**  
  `Common` (utilidades y middleware), `Domain` (entidades), `DTOs` (contratos y no exponer los modelos de BD),  
  `Infrastructure` (EF Core `DbContext`), `Services` (interfaces + implementaciones), `Controllers` (HTTP).

  Nota: Se puede  realizar por proyectos individuales o librerías de clases para mayor independencia.

- **EF Core + SQL Server (Code First)**  
  Índices y restricciones: `Usuarios.Correo` único, `TokensNegros.Jti` único. FK en bitácora y refresh tokens.  Se uso EF Core por practicidad y agilidad.
- **JWT**  
  `AccessToken` con `jti` y claims (`sub`, `email`, `nombre`), expiración corta; `RefreshToken` opaco, rotado en cada refresh.  
  Validación en `JwtBearerEvents.OnTokenValidated` consultando **blacklist** (por `jti`).  
- **Seguridad**  
  Hasheo de contraseñas con `HMACSHA256` + salt (sin contraseñas en texto plano).  
  **Bloqueo** por 3 intentos fallidos × *N* minutos (`LockMinutes` en `appsettings.json`).  
- **Respuestas uniformes**  
  Clase `ApiResponse<T>`: `{ ok, mensaje, codigo, datos, error }` en **todas** las respuestas (éxito o error).  
  `ErrorHandlingMiddleware` captura excepciones y retorna formato uniforme (no hay excepciones no controladas).  
- **SOLID**  
  Controladores “delgados”; lógica de autenticación en `IAuthService/AuthService`, dominio en `IUsuarioService/UsuarioService`, tokens en servicios dedicados.

---

## B) Flujo de autenticación JWT

1. **Registro** (`POST /api/auth/registrar`)  
   - Normaliza y valida correo; verifica unicidad (normalización minúsculas); guarda usuario con `PasswordHash` + `PasswordSalt`.
2. **Login** (`POST /api/auth/login`)  
   - Valida credenciales y estado de bloqueo.  
   - Registra en **bitácora** (éxito/fracaso, IP y User-Agent).  
   - Si éxito: reinicia contador de fallos, entrega **AccessToken** (corto) + **RefreshToken** (largo, almacenado en BD).  
3. **Uso de API**  
   - Cliente envía `Authorization: Bearer <AccessToken>`.  
   - En `OnTokenValidated`: se extrae `jti`; si está en **blacklist** → `401`.  
4. **Refresh** (`POST /api/auth/refresh`)  
   - Verifica `RefreshToken` vigente en BD.  
   - **Rotación**: revoca el token usado y emite par nuevo (*access + refresh*).  
5. **Logout** (`POST /api/auth/logout`)  
   - Extrae `jti` y fecha de expiración del `AccessToken`; agrega a **blacklist** hasta su vencimiento.  
   - Revoca **todos** los `RefreshToken` del usuario.  
6. **Bloqueo por intentos fallidos**  
   - Al tercer fallo consecutivo, establece `BloqueadoHasta = now + LockMinutes` y reinicia contador.  
   - Durante el bloqueo, login retorna `401` con código `AUTH_002`.

**Configuración relevante (`appsettings.json`)**:
```json
"Jwt": {
  "Key": "Clave único para la firma del token",
  "Issuer": "UsuariosAuth",
  "Audience": "UsuariosAuthClientes",
  "AccessMinutes": 15,
  "RefreshDays": 7,
  "LockMinutes": 15 /* para gestionar el tiempo de bloqueo por logins erroneos */
}
```
Saludos =)

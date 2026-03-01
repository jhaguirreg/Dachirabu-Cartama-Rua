# Dachirabu-Cartama-Rua
### Sistema de Gestión de Bases Censales para Comunidades Indígenas

**Enfoque institucional – Ministerio del Interior**

**Dachirabu Cartama Rua** es una aplicación diseñada para la gestión integral de censos poblacionales. El sistema está alineado con los lineamientos técnicos y los campos de información exigidos por el **Ministerio del Interior**, permitiendo la recolección, validación, almacenamiento y exportación de datos demográficos y socioeconómicos de comunidades de manera estructurada y auditable.

---

## Características Principales

- **Gestión de registros maestros:** Identificación única de personas con persistencia histórica.
- **Control por vigencias anuales:** Administración de información por año censal (2025, 2026, etc.), conservando trazabilidad de cambios.
- **Estructura familiar jerarquizada:** Organización por núcleos familiares, vivienda y parentesco (Jefe de Familia, Cónyuge, Hijos).
- **Módulos de educación y salud:** Registro de niveles educativos (NI, PR, SE, UN) y control de información relacionada con discapacidad y tratamientos.
- **Arquitectura híbrida:** Soporte para base de datos local **SQLite** y sincronización con **Supabase (PostgreSQL)**.

---

## Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Lenguaje | C# (.NET) |
| Entorno de desarrollo | Visual Studio / VS Code |
| Base de datos local | SQLite (`cabildo.db`) |
| Base de datos en la nube | Supabase (PostgreSQL) |

---

## Modelo de Datos

El sistema utiliza un modelo relacional orientado a la integridad y consistencia de la información:

1. **Persona:** Datos básicos invariables (documento, nombres, fecha de nacimiento).
2. **Familia:** Información de ubicación geográfica y características de la vivienda.
3. **Censo_Anual:** Datos variables por vigencia (ocupación, parentesco, composición familiar, apoyos).
4. **Estudios:** Historial académico asociado al documento de identidad.
5. **Discapacidad:** Registro de condiciones médicas y tratamientos.

---

## Instalación y Configuración

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/jhaguirreg/Dachirabu-Cartama-Rua
2. **Configurar la base de datos**
    Verifique que el archivo cabildo.db esté ubicado en la ruta definida en la cadena de conexión del proyecto.

3. **Permisos de escritura**
    Asegúrese de que la carpeta de instalación tenga permisos de lectura y escritura para evitar errores de solo lectura en SQLite, especialmente en instalaciones realizadas con Inno Setup.
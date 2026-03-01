using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Autocenso_Cabildo
{
    public static class ConexionDB
    {
        private static string rutaDB = Path.Combine(Application.StartupPath, "database", "cabildo.db");
        private static string cadenaConexion = $"Data Source={rutaDB}";

        public static void Inicializar()
        {
            try
            {
                string carpeta = Path.GetDirectoryName(rutaDB);
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();

                    // 1. Entidad Usuario (Para el login)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Usuario (
                            ID INTEGER PRIMARY KEY,
                            username TEXT NOT NULL UNIQUE,
                            contrasena TEXT NOT NULL,
                            Rol TEXT NOT NULL,
                            contador_familias INTEGER DEFAULT 0
                        );";
                    comando.ExecuteNonQuery();

                    // 2. Entidad Persona (Datos Maestros que NO cambian)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Persona (
                            numero_documento INTEGER PRIMARY KEY,
                            tipo_documento TEXT,
                            primer_nombre TEXT NOT NULL,
                            segundo_nombre TEXT,
                            primer_apellido TEXT NOT NULL,
                            segundo_apellido TEXT,
                            dia_nacimiento INTEGER,
                            mes_nacimiento TEXT,
                            año_nacimiento INTEGER,
                            lugar_nacimiento TEXT,
                            genero TEXT,
                            tipo_sangre TEXT
                        );";
                    comando.ExecuteNonQuery();

                    // 3. Entidad Familia (Solo datos actuales)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Familia (
                            Num_familia TEXT PRIMARY KEY, -- Clave única, no se repite nunca
                            direccion TEXT,
                            telefono INTEGER,
                            correo_electronico TEXT,
                            casa TEXT,
                            tipo_documento_casa TEXT,
                            tierra TEXT,
                            tipo_documento_tierra TEXT,
                            mina TEXT,
                            tipo_documento_mina TEXT,
                            num_fallecidos_año_anterior INTEGER,
                            Sincronizado INTEGER DEFAULT 0
                        );";
                    comando.ExecuteNonQuery();

                    // 4. Entidad Censo_Anual (Datos de la Persona que cambian cada año)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Censo_Anual (
                            numero_documento INTEGER,
                            vigencia INTEGER,
                            orientacion_sexual TEXT,
                            estado_civil TEXT,
                            cedula_pareja INTEGER DEFAULT 0,
                            Parentesco TEXT,
                            ocupacion TEXT,
                            empresa TEXT,
                            labor_comunidad TEXT,
                            num_hijos_nacidos_vivos INTEGER,
                            num_hijos_sobrevivientes INTEGER,
                            dia_ultimo_hijo INTEGER,
                            mes_ultimo_hijo TEXT,
                            año_ultimo_hijo INTEGER,
                            libreta_militar TEXT,
                            direccion_actual TEXT,
                            eps TEXT,
                            regimen TEXT,
                            tipo_censo TEXT,
                            razon_ingreso TEXT,
                            familias_accion TEXT, -- Si, No
                            renta_joven TEXT,     -- Si, No
                            adulto_mayor TEXT,    -- Si, No
                            Otros_apoyos TEXT,    -- Texto libre
                            ultimo_editor INTEGER,
                            num_familia TEXT,
                            Sincronizado INTEGER DEFAULT 0,
                            PRIMARY KEY(numero_documento, vigencia),
                            FOREIGN KEY(numero_documento) REFERENCES Persona(numero_documento),
                            FOREIGN KEY(ultimo_editor) REFERENCES Usuario(ID),
                            FOREIGN KEY(num_familia) REFERENCES Familia(Num_familia)
                        );";
                    comando.ExecuteNonQuery();

                    // 5. Entidad Estudios (Historial educativo ligado al documento)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Estudios (
                            id_persona INTEGER,
                            codigo INTEGER,
                            nivel TEXT,
                            estado TEXT,
                            grado TEXT,
                            institucion TEXT,
                            titulo TEXT,
                            PRIMARY KEY(id_persona, codigo),
                            FOREIGN KEY(id_persona) REFERENCES Persona(numero_documento)
                        );";
                    comando.ExecuteNonQuery();

                    // 6. Entidad Discapacidad (Historial médico ligado al documento)
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Discapacidad (
                            id_persona INTEGER,
                            codigo INTEGER,
                            tipo TEXT,
                            tratamiento TEXT,
                            tipo_tratamiento TEXT,
                            PRIMARY KEY(id_persona, codigo),
                            FOREIGN KEY(id_persona) REFERENCES Persona(numero_documento)
                        );";
                    comando.ExecuteNonQuery();

                    // 7. Tabla de Configuración del Sistema
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Configuracion (
                            ID INTEGER PRIMARY KEY CHECK (ID = 1), 
                            vigencia_activa INTEGER,
                            ultima_sincro TEXT DEFAULT '2000-01-01 00:00:00' -- Fecha base muy antigua
                        );";
                    comando.ExecuteNonQuery();

                    // Usuario Administrador inicial por ID
                    CrearAdminPorDefecto(comando);

                    // Ajustamos el insert inicial para incluir la fecha si la tabla es nueva
                    comando.CommandText = "SELECT COUNT(*) FROM Configuracion";
                    if ((long)comando.ExecuteScalar() == 0)
                    {
                        comando.CommandText = "INSERT INTO Configuracion (ID, vigencia_activa, ultima_sincro) VALUES (1, 2026, '2000-01-01 00:00:00')";
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar la base de datos: " + ex.Message);
            }
        }

        private static void CrearAdminPorDefecto(SqliteCommand comando)
        {
            // Verificamos si existe el ID 1 (Admin)
            comando.CommandText = "SELECT 1 FROM Usuario WHERE ID = 1";
            var resultado = comando.ExecuteScalar();

            if (resultado == null)
            {
                comando.CommandText = "INSERT INTO Usuario (ID, username, contrasena, Rol) VALUES (1, 'root', 'root', 'Administrador')";
                comando.ExecuteNonQuery();
            }
        }

        public static SqliteConnection ObtenerConexion()
        {
            return new SqliteConnection(cadenaConexion);
        }

        // Agrega este método dentro de tu clase static ConexionDB
        public static DataTable EjecutarConsulta(string sql, SqliteParameter[] parametros = null)
        {
            DataTable tabla = new DataTable();
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    conexion.Open();
                    using (var comando = conexion.CreateCommand())
                    {
                        comando.CommandText = sql;
                        if (parametros != null)
                        {
                            comando.Parameters.AddRange(parametros);
                        }

                        // Usamos un DataReader para llenar el DataTable
                        using (var reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                        comando.Parameters.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar consulta: " + ex.Message);
            }
            return tabla;
        }

        public static int EjecutarOrden(string sql, SqliteParameter[] parametros = null)
        {
            int filasAfectadas = 0;
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    conexion.Open();
                    using (var comando = conexion.CreateCommand())
                    {
                        comando.CommandText = sql;
                        if (parametros != null)
                        {
                            comando.Parameters.AddRange(parametros);
                        }

                        // ExecuteNonQuery se usa para INSERT, UPDATE y DELETE
                        filasAfectadas = comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar cambio en la base de datos: " + ex.Message);
            }
            return filasAfectadas;
        }
    }
}
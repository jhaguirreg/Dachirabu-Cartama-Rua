using Microsoft.Data.Sqlite;
using System.Data;

namespace Autocenso_Cabildo
{
    public class PersonaController
    {
        // Retorna ultima vigencia o 0 en INT.
        public int ObtenerUltimaVigenciaCenso(long numeroDocumento)
        {
            // 1. SQL para buscar la vigencia más alta (la más reciente)
            string sql = @"SELECT vigencia 
                           FROM Censo_Anual 
                           WHERE numero_documento = @doc 
                           ORDER BY vigencia DESC 
                           LIMIT 1";

            SqliteParameter[] parametros = new SqliteParameter[]
            {
                new SqliteParameter("@doc", numeroDocumento)
            };

            // 2. Ejecutamos la consulta usando tu método static de ConexionDB
            DataTable dt = ConexionDB.EjecutarConsulta(sql, parametros);

            // 3. Verificamos si encontramos algún registro
            if (dt.Rows.Count > 0)
            {
                // Retornamos el valor de la columna 'vigencia' convertido a entero
                return Convert.ToInt32(dt.Rows[0]["vigencia"]);
            }

            // 4. Si nunca se ha censado, retornamos 0 o un valor que indique "sin registro"
            return 0;
        }

        public int ObtenerUltimoEditor(long numeroDocumento)
        {
            // 1. SQL para buscar la vigencia más alta (la más reciente)
            string sql = @"SELECT ultimo_editor 
                           FROM Censo_Anual 
                           WHERE numero_documento = @doc 
                           ORDER BY vigencia DESC 
                           LIMIT 1";

            SqliteParameter[] parametros = new SqliteParameter[]
            {
                new SqliteParameter("@doc", numeroDocumento)
            };

            // 2. Ejecutamos la consulta usando tu método static de ConexionDB
            DataTable dt = ConexionDB.EjecutarConsulta(sql, parametros);

            // 3. Verificamos si encontramos algún registro
            if (dt.Rows.Count > 0)
            {
                // Retornamos el valor de la columna 'vigencia' convertido a entero
                return Convert.ToInt32(dt.Rows[0]["ultimo_editor"]);
            }

            // 4. Si nunca se ha censado, retornamos 0 o un valor que indique "sin registro"
            return 0;
        }

        public DataSet ObtenerInformacionPersonaCenso(long documento, int vigencia, DataSet dataSetOriginal)
        {
            // 1. Validaciones iniciales
            if (vigencia == 0 || dataSetOriginal == null) return dataSetOriginal;

            // 2. Verificar si el documento ya existe en la tabla "Persona" del DataSet
            // Usamos PrimaryKey (debe estar definida en el DataTable) o una búsqueda por Rows
            bool existePersona = false;
            if (dataSetOriginal.Tables.Contains("Persona"))
            {
                DataRow[] filas = dataSetOriginal.Tables["Persona"].Select($"numero_documento = {documento}");
                if (filas.Length > 0) existePersona = true;
            }

            // 3. Si no existe, traemos la información de la BD y la agregamos al DataSet
            if (!existePersona)
            {
                SqliteParameter[] paramDoc = { new SqliteParameter("@doc", documento) };
                SqliteParameter[] paramCenso = {
                    new SqliteParameter("@doc", documento),
                    new SqliteParameter("@vig", vigencia) 
                };
                
                // --- Cargar Persona ---
                DataTable dtPersona = ConexionDB.EjecutarConsulta("SELECT * FROM Persona WHERE numero_documento = @doc", paramDoc);
                ImportarResultados(dtPersona, dataSetOriginal, "Persona");

                // --- Cargar Censo_Anual ---
                DataTable dtCenso = ConexionDB.EjecutarConsulta("SELECT * FROM Censo_Anual WHERE numero_documento = @doc AND vigencia = @vig", paramCenso);
                ImportarResultados(dtCenso, dataSetOriginal, "Censo_Anual");

                // --- Cargar Estudios ---
                DataTable dtEst = ConexionDB.EjecutarConsulta("SELECT * FROM Estudios WHERE id_persona = @doc", paramDoc);
                ImportarResultados(dtEst, dataSetOriginal, "Estudios");

                // --- Cargar Discapacidad ---
                DataTable dtDisc = ConexionDB.EjecutarConsulta("SELECT * FROM Discapacidad WHERE id_persona = @doc", paramDoc);
                ImportarResultados(dtDisc, dataSetOriginal, "Discapacidad");
            }
            return dataSetOriginal;
        }

        // Método auxiliar para pasar las filas de la consulta al DataSet original sin sobrescribir la estructura
        private void ImportarResultados(DataTable consulta, DataSet destino, string nombreTabla)
        {
            if (consulta != null && destino.Tables.Contains(nombreTabla))
            {
                DataTable tablaDestino = destino.Tables[nombreTabla];

                foreach (DataRow filaOrigen in consulta.Rows)
                {
                    // Creamos una nueva fila compatible con la estructura del destino
                    DataRow nuevaFila = tablaDestino.NewRow();

                    // Recorremos las columnas de la tabla destino para llenarlas
                    foreach (DataColumn colDestino in tablaDestino.Columns)
                    {
                        // Verificamos si la columna existe en el resultado de la consulta SQL
                        if (consulta.Columns.Contains(colDestino.ColumnName))
                        {
                            nuevaFila[colDestino.ColumnName] = filaOrigen[colDestino.ColumnName];
                        }
                    }
                    tablaDestino.Rows.Add(nuevaFila);
                }
            }
        }
    }
}
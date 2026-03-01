using Microsoft.Data.Sqlite;
using System.Data;

namespace Autocenso_Cabildo
{
    public class FamiliaController
    {
        // 1. Obtener el código de familia vinculado a una persona en un año específico
        public string ObtenerCodigoFamilia(long documento, int vigencia)
        {
            string sql = "SELECT num_familia FROM Censo_Anual WHERE numero_documento = @doc AND vigencia = @vig";
            SqliteParameter[] parametros = {
                new SqliteParameter("@doc", documento),
                new SqliteParameter("@vig", vigencia)
            };

            DataTable dt = ConexionDB.EjecutarConsulta(sql, parametros);
            return dt.Rows.Count > 0 ? dt.Rows[0]["num_familia"].ToString() : null;
        }

        // 2. Obtener los datos maestros de la Familia (Dirección, Casa, Tierra, etc.)
        public void ObtenerDatosVivienda(string codigoFamilia, DataSet dsCenso)
        {
            // 1. Definir la consulta y los parámetros
            string sql = "SELECT * FROM Familia WHERE Num_familia = @codFam";
            SqliteParameter[] parametros = { new SqliteParameter("@codFam", codigoFamilia) };

            // 2. Ejecutar la consulta en la base de datos
            DataTable dtResultado = ConexionDB.EjecutarConsulta(sql, parametros);

            // 3. Verificar si la tabla "Familia" ya existe en el DataSet
            if (!dsCenso.Tables.Contains("Familia"))
            {
                // --- Si no existe, creamos la estructura manual que pediste ---
                DataTable familia = new DataTable("Familia");
                familia.Columns.Add("direccion", typeof(string));
                familia.Columns.Add("telefono", typeof(long));
                familia.Columns.Add("correo_electronico", typeof(string));
                familia.Columns.Add("casa", typeof(string));
                familia.Columns.Add("tipo_documento_casa", typeof(string));
                familia.Columns.Add("tierra", typeof(string));
                familia.Columns.Add("tipo_documento_tierra", typeof(string));
                familia.Columns.Add("mina", typeof(string));
                familia.Columns.Add("tipo_documento_mina", typeof(string));
                familia.Columns.Add("num_fallecidos_año_anterior", typeof(int));

                dsCenso.Tables.Add(familia);
            }
            else
            {
                // Si ya existe, simplemente limpiamos los datos viejos
                dsCenso.Tables["Familia"].Clear();
            }

            // 4. Llenamos la tabla del DataSet con los resultados de la consulta
            // MissingSchemaAction.Ignore hace que solo se llenen las columnas que definiste arriba
            dsCenso.Tables["Familia"].Merge(dtResultado, false, MissingSchemaAction.Ignore);
        }

        public void ObtenerDatosFamilia_paraSinc(string codigoFamilia, DataSet dsCenso)
        {
            // 1. Definir la consulta
            string sql = "SELECT * FROM Familia WHERE Num_familia = @codFam";
            SqliteParameter[] parametros = { new SqliteParameter("@codFam", codigoFamilia) };

            // 2. Ejecutar la consulta en la base de datos local
            DataTable dtResultado = ConexionDB.EjecutarConsulta(sql, parametros);

            // 3. Darle nombre a la tabla (Crucial para que SubirPaqueteNubeAsync la encuentre)
            dtResultado.TableName = "Familia";

            // 4. Limpiar si ya existía y agregar la nueva tabla al DataSet
            if (dsCenso.Tables.Contains("Familia"))
            {
                dsCenso.Tables.Remove("Familia");
            }

            dsCenso.Tables.Add(dtResultado.Copy());
        }

        // 3. Obtener todos los integrantes de esa familia en esa vigencia
        public long[] ObtenerDocumentosIntegrantes(string codigoFamilia, int vigencia)
        {
            // 1. Definimos la consulta SQL
            string sql = @"
            SELECT 
            p.numero_documento
            FROM Censo_Anual c
            INNER JOIN Persona p ON c.numero_documento = p.numero_documento
            WHERE c.num_familia = @codFam AND c.vigencia = @vig";

            SqliteParameter[] parametros = {
                new SqliteParameter("@codFam", codigoFamilia),
                new SqliteParameter("@vig", vigencia)
            };

            // 2. Ejecutamos la consulta para obtener los datos
            DataTable dtIntegrantes = ConexionDB.EjecutarConsulta(sql, parametros);

            // 3. Convertimos la columna de documentos a long[] usando Convert.ToInt64
            long[] documentos = dtIntegrantes.AsEnumerable()
                                             .Select(fila => Convert.ToInt64(fila["numero_documento"]))
                                             .ToArray();

            return documentos;
        }
    }
}
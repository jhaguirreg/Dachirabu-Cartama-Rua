using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace Autocenso_Cabildo
{
    public class MainController
    {
        private PersonaController personaCtrl = new PersonaController();
        private FamiliaController familiaCtrl = new FamiliaController();

        ///// Base de datos. 
        public DataSet ObtenerCensoCompleto(long documento, int UserId, int vigencia)
        {
            DataSet dsResultado = CrearDataSetCenso();

            // 1. Obtener la última vigencia (Usando PersonaController)
            int ultimaVigencia = personaCtrl.ObtenerUltimaVigenciaCenso(documento);

            if (ultimaVigencia == 0) return null; // No hay datos de censo

            // 3. Obtener el código de la familia (Usando FamiliaController)
            string codFam = familiaCtrl.ObtenerCodigoFamilia(documento, ultimaVigencia);

            if (!string.IsNullOrEmpty(codFam))
            {
                familiaCtrl.ObtenerDatosVivienda(codFam, dsResultado);

                // 4. Obtener todos los integrantes (Usando FamiliaController)
                // 1. Llamamos a la función y guardamos el array de documentos
                long[] documentos = familiaCtrl.ObtenerDocumentosIntegrantes(codFam, ultimaVigencia);

                // 2. Recorremos con un for clásico
                for (int i = 0; i < documentos.Length; i++)
                {
                    long docActual = documentos[i];
                    personaCtrl.ObtenerInformacionPersonaCenso(docActual, ultimaVigencia, dsResultado);

                    DataRow[] filas = dsResultado.Tables["Censo_Anual"].Select($"numero_documento = {docActual}");

                    if (filas.Length > 0)
                    {
                        if (ultimaVigencia != vigencia)
                        {
                            filas[0]["razon_ingreso"] = filas[0]["tipo_censo"];
                            filas[0]["tipo_censo"] = $"Regimen_{vigencia}";

                            if (filas[0]["razon_ingreso"].ToString() == "SOLICITUD_ESTUDIO_RECHAZADA" || filas[0]["razon_ingreso"].ToString() == "SOLICITUD_ESTUDIO" || filas[0]["razon_ingreso"].ToString() == "INHABILITADO")
                            {
                                filas[0]["tipo_censo"] = filas[0]["razon_ingreso"];
                            }
                        }
                        // IMPORTANTE: Forzamos el fin de la edición para que el DataSet tome el cambio
                    }
                }
            }

            DataTable tablaConfig = dsResultado.Tables["Configuracion"];

            // 2. Borramos todos los datos previos de la tabla
            tablaConfig.Clear();

            // 3. Creamos una nueva fila
            DataRow nuevaFila = tablaConfig.NewRow();

            if (ultimaVigencia == vigencia)
            {
                // 4. Asignamos el valor de tu variable codFam
                // Nota: Asegúrate que codFam sea de tipo int como definiste en la columna
                nuevaFila["CodFam"] = int.Parse(codFam.Split('-')[1]);

                // 5. Agregamos la fila a la tabla

            } else
            {
                int valor_codigo_nuevo = ObtenerContadorFamilias(UserId);
                nuevaFila["CodFam"] = valor_codigo_nuevo;
            }
            tablaConfig.Rows.Add(nuevaFila);

            return dsResultado;
        }

        public int ObtenerContadorFamilias(int userId)
        {
            string sql = "SELECT contador_familias FROM Usuario WHERE ID = @id";
            SqliteParameter[] parametros = { new SqliteParameter("@id", userId) };

            DataTable dt = ConexionDB.EjecutarConsulta(sql, parametros);

            // Si hay filas, retornamos el valor convertido a int, si no, retornamos 0
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["contador_familias"]) : 0;
        }

        public void IncrementarContadorFamilias(int userId)
        {
            string sql = "UPDATE Usuario SET contador_familias = contador_familias + 1 WHERE ID = @id";
            SqliteParameter[] parametros = { new SqliteParameter("@id", userId) };

            // Usamos la nueva función EjecutarOrden para aplicar el cambio
            ConexionDB.EjecutarOrden(sql, parametros);
        }

        ///// Archivos familia.
        public DataSet CrearDataSetCenso_ParaSinc()
        {
            DataSet dsCenso = new DataSet("CensoPoblacional");

            // --- 1. TABLA PERSONA (Datos Maestros) ---
            DataTable persona = new DataTable("Persona");
            persona.Columns.Add("numero_documento", typeof(long));
            persona.Columns.Add("tipo_documento", typeof(string));
            persona.Columns.Add("primer_nombre", typeof(string));
            persona.Columns.Add("segundo_nombre", typeof(string));
            persona.Columns.Add("primer_apellido", typeof(string));
            persona.Columns.Add("segundo_apellido", typeof(string));
            persona.Columns.Add("dia_nacimiento", typeof(int));
            persona.Columns.Add("mes_nacimiento", typeof(string));
            persona.Columns.Add("año_nacimiento", typeof(int));
            persona.Columns.Add("lugar_nacimiento", typeof(string));
            persona.Columns.Add("genero", typeof(string));
            persona.Columns.Add("tipo_sangre", typeof(string));

            persona.PrimaryKey = new DataColumn[] { persona.Columns["numero_documento"] };
            dsCenso.Tables.Add(persona);

            // --- 2. TABLA FAMILIA ---
            DataTable familia = new DataTable("Familia");
            familia.Columns.Add("Num_familia", typeof(string));
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
            familia.Columns.Add("Sincronizado", typeof(string));
            dsCenso.Tables.Add(familia);

            // --- 3. TABLA CENSO ANUAL ---
            DataTable censoAnual = new DataTable("Censo_Anual");
            censoAnual.Columns.Add("numero_documento", typeof(long));
            censoAnual.Columns.Add("vigencia", typeof(int));
            censoAnual.Columns.Add("orientacion_sexual", typeof(string));
            censoAnual.Columns.Add("estado_civil", typeof(string));
            censoAnual.Columns.Add("cedula_pareja", typeof(long));
            censoAnual.Columns.Add("Parentesco", typeof(string));
            censoAnual.Columns.Add("ocupacion", typeof(string));
            censoAnual.Columns.Add("empresa", typeof(string));
            censoAnual.Columns.Add("labor_comunidad", typeof(string));
            censoAnual.Columns.Add("num_hijos_nacidos_vivos", typeof(int));
            censoAnual.Columns.Add("num_hijos_sobrevivientes", typeof(int));
            censoAnual.Columns.Add("dia_ultimo_hijo", typeof(int));
            censoAnual.Columns.Add("mes_ultimo_hijo", typeof(string));
            censoAnual.Columns.Add("año_ultimo_hijo", typeof(int));
            censoAnual.Columns.Add("libreta_militar", typeof(string));
            censoAnual.Columns.Add("direccion_actual", typeof(string));
            censoAnual.Columns.Add("eps", typeof(string));
            censoAnual.Columns.Add("regimen", typeof(string));
            censoAnual.Columns.Add("tipo_censo", typeof(string));
            censoAnual.Columns.Add("razon_ingreso", typeof(string));
            censoAnual.Columns.Add("familias_accion", typeof(string));
            censoAnual.Columns.Add("renta_joven", typeof(string));
            censoAnual.Columns.Add("adulto_mayor", typeof(string));
            censoAnual.Columns.Add("Otros_apoyos", typeof(string));
            censoAnual.Columns.Add("ultimo_editor", typeof(int));
            censoAnual.Columns.Add("num_familia", typeof(string));
            censoAnual.Columns.Add("Sincronizado", typeof(string));

            dsCenso.Tables.Add(censoAnual);

            // --- 4. TABLA ESTUDIOS ---
            DataTable estudios = new DataTable("Estudios");
            estudios.Columns.Add("id_persona", typeof(long));
            estudios.Columns.Add("codigo", typeof(int));
            estudios.Columns.Add("nivel", typeof(string));
            estudios.Columns.Add("estado", typeof(string));
            estudios.Columns.Add("grado", typeof(string));
            estudios.Columns.Add("institucion", typeof(string));
            estudios.Columns.Add("titulo", typeof(string));
            dsCenso.Tables.Add(estudios);

            // --- 5. TABLA DISCAPACIDAD ---
            DataTable discapacidad = new DataTable("Discapacidad");
            discapacidad.Columns.Add("id_persona", typeof(long));
            discapacidad.Columns.Add("codigo", typeof(int));
            discapacidad.Columns.Add("tipo", typeof(string));
            discapacidad.Columns.Add("tratamiento", typeof(string));
            discapacidad.Columns.Add("tipo_tratamiento", typeof(string));
            dsCenso.Tables.Add(discapacidad);

            return dsCenso;
        }

        public DataSet CrearDataSetCenso()
        {
            DataSet dsCenso = new DataSet("CensoPoblacional");

            // --- 1. TABLA PERSONA (Datos Maestros) ---
            DataTable persona = new DataTable("Persona");
            persona.Columns.Add("numero_documento", typeof(long));
            persona.Columns.Add("tipo_documento", typeof(string));
            persona.Columns.Add("primer_nombre", typeof(string));
            persona.Columns.Add("segundo_nombre", typeof(string));
            persona.Columns.Add("primer_apellido", typeof(string));
            persona.Columns.Add("segundo_apellido", typeof(string));
            persona.Columns.Add("dia_nacimiento", typeof(int));
            persona.Columns.Add("mes_nacimiento", typeof(string));
            persona.Columns.Add("año_nacimiento", typeof(int));
            persona.Columns.Add("lugar_nacimiento", typeof(string));
            persona.Columns.Add("genero", typeof(string));
            persona.Columns.Add("tipo_sangre", typeof(string));

            persona.PrimaryKey = new DataColumn[] { persona.Columns["numero_documento"] };
            dsCenso.Tables.Add(persona);

            // --- 2. TABLA FAMILIA ---
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

            // --- 3. TABLA CENSO ANUAL ---
            DataTable censoAnual = new DataTable("Censo_Anual");
            censoAnual.Columns.Add("numero_documento", typeof(long));
            censoAnual.Columns.Add("orientacion_sexual", typeof(string));
            censoAnual.Columns.Add("estado_civil", typeof(string));
            censoAnual.Columns.Add("cedula_pareja", typeof(long));
            censoAnual.Columns.Add("Parentesco", typeof(string));
            censoAnual.Columns.Add("ocupacion", typeof(string));
            censoAnual.Columns.Add("empresa", typeof(string));
            censoAnual.Columns.Add("labor_comunidad", typeof(string));
            censoAnual.Columns.Add("num_hijos_nacidos_vivos", typeof(int));
            censoAnual.Columns.Add("num_hijos_sobrevivientes", typeof(int));
            censoAnual.Columns.Add("dia_ultimo_hijo", typeof(int));
            censoAnual.Columns.Add("mes_ultimo_hijo", typeof(string));
            censoAnual.Columns.Add("año_ultimo_hijo", typeof(int));
            censoAnual.Columns.Add("libreta_militar", typeof(string));
            censoAnual.Columns.Add("direccion_actual", typeof(string));
            censoAnual.Columns.Add("eps", typeof(string));
            censoAnual.Columns.Add("regimen", typeof(string));
            censoAnual.Columns.Add("tipo_censo", typeof(string));
            censoAnual.Columns.Add("razon_ingreso", typeof(string));
            censoAnual.Columns.Add("familias_accion", typeof(string));
            censoAnual.Columns.Add("renta_joven", typeof(string));
            censoAnual.Columns.Add("adulto_mayor", typeof(string));
            censoAnual.Columns.Add("Otros_apoyos", typeof(string));
            dsCenso.Tables.Add(censoAnual);

            // --- 4. TABLA ESTUDIOS ---
            DataTable estudios = new DataTable("Estudios");
            estudios.Columns.Add("id_persona", typeof(long));
            estudios.Columns.Add("codigo", typeof(int));
            estudios.Columns.Add("nivel", typeof(string));
            estudios.Columns.Add("estado", typeof(string));
            estudios.Columns.Add("grado", typeof(string));
            estudios.Columns.Add("institucion", typeof(string));
            estudios.Columns.Add("titulo", typeof(string));
            dsCenso.Tables.Add(estudios);

            // --- 5. TABLA DISCAPACIDAD ---
            DataTable discapacidad = new DataTable("Discapacidad");
            discapacidad.Columns.Add("id_persona", typeof(long));
            discapacidad.Columns.Add("codigo", typeof(int));
            discapacidad.Columns.Add("tipo", typeof(string));
            discapacidad.Columns.Add("tratamiento", typeof(string));
            discapacidad.Columns.Add("tipo_tratamiento", typeof(string));
            dsCenso.Tables.Add(discapacidad);

            // --- 5. TABLA OTROS DATOS ---
            DataTable Configuracion = new DataTable("Configuracion");
            Configuracion.Columns.Add("CodFam", typeof(int));
            dsCenso.Tables.Add(Configuracion);

            return dsCenso;
        }

    }
}
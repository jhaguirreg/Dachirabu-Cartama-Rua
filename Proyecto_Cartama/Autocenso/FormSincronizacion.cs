using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.ApplicationServices;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Autocenso_Cabildo
{
    public partial class FormSincronizacion : Form
    {
        private FamiliaController familiaCtrl = new FamiliaController();
        private MainController mainCtrl = new MainController();
        private PersonaController personaCtrl = new PersonaController();
        int contador;
        private bool sincronizando = false;
        int usuario_actual;
        public FormSincronizacion(int userId)
        {
            InitializeComponent();
            this.usuario_actual = userId;
        }

        private void FormSincronizacionClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private async Task<bool> ProcesoPush()
        {   
            await SincronizarContadorUsuarioNubeAsync().ConfigureAwait(false);

            // 1. Buscamos todas las familias marcadas como NO sincronizadas
            DataTable dtFamiliasPendientes = ConexionDB.EjecutarConsulta("SELECT Num_familia FROM Familia WHERE Sincronizado = 'NO'");

            int totalFamilias = dtFamiliasPendientes.Rows.Count;
            double avancePorFamilia = 0;
            double porcentaje = pbProgreso.Value;
            if (totalFamilias != 0)
            {
                avancePorFamilia = 40.0 / totalFamilias;
            }

            foreach (DataRow fila in dtFamiliasPendientes.Rows)
            {

                string codFam = fila["Num_familia"].ToString();

                // 2. Usamos tu lógica (o similar) para obtener TODO el paquete de la familia
                // Aquí extraemos los documentos de los integrantes para esta familia
                SqliteParameter[] pFam = { new SqliteParameter("@cod", codFam) };
                DataTable dtIntegrantes = ConexionDB.EjecutarConsulta("SELECT numero_documento FROM Censo_Anual WHERE num_familia = @cod", pFam);

                DataSet dsPaquete = mainCtrl.CrearDataSetCenso_ParaSinc(); // Tu método que estructura el DataSet

                // Llenamos el DataSet con la info local
                familiaCtrl.ObtenerDatosFamilia_paraSinc(codFam, dsPaquete);

                foreach (DataRow p in dtIntegrantes.Rows)
                {
                    long doc = Convert.ToInt64(p["numero_documento"]);
                    int vigencia = personaCtrl.ObtenerUltimaVigenciaCenso(doc);
                    dsPaquete = personaCtrl.ObtenerInformacionPersonaCenso(doc, vigencia, dsPaquete);
                }
                try { 
                    // 3. Enviamos el paquete completo a la Nube
                    bool exito = await SubirPaqueteNubeAsync(dsPaquete, codFam).ConfigureAwait(false);

                    if (exito)
                    {
                        // 4. Si la nube confirmó, marcamos como sincronizado localmente
                        SqliteParameter[] pUpd = { new SqliteParameter("@cod", codFam) };
                        ConexionDB.EjecutarOrden("UPDATE Familia SET Sincronizado = 'SI' WHERE Num_familia = @cod", pUpd);
                        this.contador++;
                        porcentaje += avancePorFamilia;
                        if ((int)porcentaje < 100)
                        {
                            pbProgreso.Value = (int)porcentaje;
                        }
                    }

                } catch (Exception ex)
                {
                    // Si una familia falla, registramos y continuamos con la siguiente en lugar de romper todo el proceso
                    MessageBox.Show($"Error subiendo familia {codFam}: {ex.Message}");
                }
            }
            return true;
        }

        private async Task<bool> SincronizarContadorUsuarioNubeAsync()
        {
            // 1. Instanciamos y abrimos la conexión internamente
            using (var conn = new ConexionNube().GetConexion())
            {
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);

                    // 2. Obtener el contador de la base de datos local (SQLite)
                    int contadorLocal = 0;
                    // IMPORTANTE: Pasar el ID numérico, no el objeto completo
                    SqliteParameter[] pAddUsu = { new SqliteParameter("@id", this.usuario_actual) };
                    DataTable dt = ConexionDB.EjecutarConsulta("SELECT contador_familias FROM Usuario WHERE ID = @id", pAddUsu);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        contadorLocal = Convert.ToInt32(dt.Rows[0]["contador_familias"]);
                    }

                    // 3. Actualizar en la nube (PostgreSQL)
                    string sqlNube = @"UPDATE ""Usuario"" SET 
                                contador_familias = @cont 
                              WHERE ""ID"" = @id";

                    using (var cmdNube = new NpgsqlCommand(sqlNube, conn))
                    {
                        cmdNube.Parameters.AddWithValue("@cont", contadorLocal);
                        cmdNube.Parameters.AddWithValue("@id", this.usuario_actual);

                        await cmdNube.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sincronizando contador de usuario:\n" + ex.Message);
                    return false;
                }
                // El bloque 'using' cerrará la conexión automáticamente aquí
            }
        }

        private async Task<bool> SubirPaqueteNubeAsync(DataSet ds, string codFam)
        {
            using (var conn = new ConexionNube().GetConexion())
            {
                await conn.OpenAsync().ConfigureAwait(false);

                    try
                    {
                        // --- 1. UPSERT FAMILIA (Maestro) ---
                        // Usamos ON CONFLICT para actualizar si la familia ya existe
                        DataTable dtFamilia = ds.Tables["Familia"];
                        foreach (DataRow row in dtFamilia.Rows)
                        {
                            string sqlFam = @"INSERT INTO ""Familia"" (
                                ""Num_familia"", direccion, telefono, correo_electronico, casa, 
                                tipo_documento_casa, tierra, tipo_documento_tierra, mina, 
                                tipo_documento_mina, num_fallecidos_año_anterior, ""Sincronizado"", fecha_sincronizacion
                            ) VALUES (
                                @num, @dir, @tel, @mail, @casa, @tdc, @tier, @tdt, @mina, @tdm, @fall, 'SI', @fecha
                            ) ON CONFLICT (""Num_familia"") DO UPDATE SET 
                                direccion = excluded.direccion, 
                                telefono = excluded.telefono, 
                                correo_electronico = excluded.correo_electronico, 
                                casa = excluded.casa, 
                                tipo_documento_casa = excluded.tipo_documento_casa,
                                tierra = excluded.tierra, 
                                tipo_documento_tierra = excluded.tipo_documento_tierra,
                                mina = excluded.mina, 
                                tipo_documento_mina = excluded.tipo_documento_mina,
                                num_fallecidos_año_anterior = excluded.num_fallecidos_año_anterior,
                                ""Sincronizado"" = 'SI',
                                fecha_sincronizacion = EXCLUDED.fecha_sincronizacion;";

                            using(var cmd = new NpgsqlCommand(sqlFam, conn))
                            {
                                cmd.Parameters.AddWithValue("@num", codFam);
                                cmd.Parameters.AddWithValue("@dir", row["direccion"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tel", row["telefono"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@mail", row["correo_electronico"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@casa", row["casa"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tdc", row["tipo_documento_casa"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tier", row["tierra"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tdt", row["tipo_documento_tierra"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@mina", row["mina"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tdm", row["tipo_documento_mina"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@fall", row["num_fallecidos_año_anterior"] ?? 0);
                                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }

                    // --- 2. BORRADO DE DATOS VOLÁTILES (Censo, Estudios, Discapacidad) ---
                    // Borramos para evitar duplicados y manejar eliminaciones de integrantes
                        string sqlLimpiar = @"
                                DELETE FROM ""Censo_Anual"" WHERE num_familia = @fam;";

                        using (var cmdDel = new NpgsqlCommand(sqlLimpiar, conn))
                        {
                            cmdDel.Parameters.AddWithValue("@fam", codFam);
                            await cmdDel.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        // --- 3. UPSERT PERSONAS (Maestro) ---
                        foreach (DataRow row in ds.Tables["Persona"].Rows)
                        {
                            if (row["numero_documento"] == DBNull.Value) continue;

                            string sqlPers = @"INSERT INTO ""Persona"" (
                                numero_documento, tipo_documento, primer_nombre, segundo_nombre, 
                                primer_apellido, segundo_apellido, dia_nacimiento, mes_nacimiento, 
                                año_nacimiento, lugar_nacimiento, genero, tipo_sangre
                            ) VALUES (
                                @doc, @td, @pn, @sn, @pa, @sa, @dn, @mn, @an, @ln, @gen, @ts
                            ) ON CONFLICT (numero_documento) DO UPDATE SET 
                                tipo_documento = excluded.tipo_documento,
                                primer_nombre = excluded.primer_nombre, 
                                segundo_nombre = excluded.segundo_nombre, 
                                primer_apellido = excluded.primer_apellido, 
                                segundo_apellido = excluded.segundo_apellido,
                                dia_nacimiento = excluded.dia_nacimiento,
                                mes_nacimiento = excluded.mes_nacimiento,
                                año_nacimiento = excluded.año_nacimiento,
                                lugar_nacimiento = excluded.lugar_nacimiento,
                                genero = excluded.genero,
                                tipo_sangre = excluded.tipo_sangre;";

                            using (var cmdP = new NpgsqlCommand(sqlPers, conn))
                            {
                                cmdP.Parameters.AddWithValue("@doc", row["numero_documento"]);
                                cmdP.Parameters.AddWithValue("@td", row["tipo_documento"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@pn", row["primer_nombre"] ?? "");
                                cmdP.Parameters.AddWithValue("@sn", row["segundo_nombre"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@pa", row["primer_apellido"] ?? "");
                                cmdP.Parameters.AddWithValue("@sa", row["segundo_apellido"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@dn", row["dia_nacimiento"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@mn", row["mes_nacimiento"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@an", row["año_nacimiento"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@ln", row["lugar_nacimiento"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@gen", row["genero"] ?? DBNull.Value);
                                cmdP.Parameters.AddWithValue("@ts", row["tipo_sangre"] ?? DBNull.Value);
                                await cmdP.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }

                    // --- BORRADO MASIVO DE ESTUDIOS Y DISCAPACIDAD ---

                    // 1. Creamos una lista con los IDs únicos de las personas que vienen en el Censo
                    var listaIds = ds.Tables["Censo_Anual"].AsEnumerable()
                                   .Select(r => r.Field<object>("numero_documento").ToString())
                                   .Distinct()
                                   .ToList();

                    if (listaIds.Count > 0)
                    {
                        // Unimos los IDs en un string separado por comas: "123, 456, 789"
                        string idsParaSql = string.Join(",", listaIds);

                        // 2. Borramos todos los estudios de esas personas de un solo golpe
                        string sqlDelEstudios = $@"DELETE FROM ""Estudios"" WHERE id_persona IN ({idsParaSql})";
                        using (var cmdEst = new NpgsqlCommand(sqlDelEstudios, conn))
                        {
                            await cmdEst.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        // 3. Borramos todas las discapacidades de esas personas de un solo golpe
                        string sqlDelDisc = $@"DELETE FROM ""Discapacidad"" WHERE id_persona IN ({idsParaSql})";
                        using (var cmdDisc = new NpgsqlCommand(sqlDelDisc, conn))
                        {
                            await cmdDisc.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    // --- 4. INSERTAR CENSO ANUAL ---
                    foreach (DataRow row in ds.Tables["Censo_Anual"].Rows)
                        {
                            long doc = Convert.ToInt64(row["numero_documento"]);
                            int vigencia = Convert.ToInt32(row["vigencia"]);

                            if (row["numero_documento"] == DBNull.Value) continue;

                            string sqlInsCenso = @"INSERT INTO ""Censo_Anual"" (
                                numero_documento, vigencia, orientacion_sexual, estado_civil, cedula_pareja, 
                                ""Parentesco"", ocupacion, empresa, labor_comunidad, num_hijos_nacidos_vivos, 
                                num_hijos_sobrevivientes, dia_ultimo_hijo, mes_ultimo_hijo, año_ultimo_hijo, 
                                libreta_militar, direccion_actual, eps, regimen, tipo_censo, razon_ingreso, 
                                familias_accion, renta_joven, adulto_mayor, ""Otros_apoyos"", ultimo_editor, num_familia, ""Sincronizado""
                            ) VALUES (
                                @doc, @vig, @ori, @est, @pare, @par, @ocu, @emp, @lab, @nhv, 
                                @nsob, @duh, @muh, @auh, @lib, @dir, @eps, @reg, @tce, @raz, 
                                @fac, @rjo, @ama, @oap, @edit, @fam, 'SI'
                            )";

                            using (var cmd = new NpgsqlCommand(sqlInsCenso, conn))
                            {
                                cmd.Parameters.AddWithValue("@doc", row["numero_documento"]);
                                cmd.Parameters.AddWithValue("@vig", row["vigencia"]);
                                cmd.Parameters.AddWithValue("@fam", row["num_familia"]);
                                cmd.Parameters.AddWithValue("@edit", row["ultimo_editor"]);
                                cmd.Parameters.AddWithValue("@ori", row["orientacion_sexual"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@est", row["estado_civil"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pare", row["cedula_pareja"] ?? 0);
                                cmd.Parameters.AddWithValue("@par", row["Parentesco"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ocu", row["ocupacion"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@emp", row["empresa"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@lab", row["labor_comunidad"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@nhv", row["num_hijos_nacidos_vivos"] ?? 0);
                                cmd.Parameters.AddWithValue("@nsob", row["num_hijos_sobrevivientes"] ?? 0);
                                cmd.Parameters.AddWithValue("@duh", row["dia_ultimo_hijo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@muh", row["mes_ultimo_hijo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@auh", row["año_ultimo_hijo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@lib", row["libreta_militar"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@dir", row["direccion_actual"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@eps", row["eps"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@reg", row["regimen"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tce", row["tipo_censo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@raz", row["razon_ingreso"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@fac", row["familias_accion"] ?? "No");
                                cmd.Parameters.AddWithValue("@rjo", row["renta_joven"] ?? "No");
                                cmd.Parameters.AddWithValue("@ama", row["adulto_mayor"] ?? "No");
                                cmd.Parameters.AddWithValue("@oap", row["Otros_apoyos"] ?? DBNull.Value);
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }

                            ConexionDB.EjecutarOrden($"UPDATE Censo_Anual SET Sincronizado = 'SI' WHERE numero_documento = '{doc}' AND vigencia = '{vigencia}'");

                        }

                    // --- 5. REINSERTAR ESTUDIOS ---
                    // --- BORRADO PREVIO DE ESTUDIOS EN LA NUBE ---
                    foreach (DataRow row in ds.Tables["Estudios"].Rows)
                    {
                        // Solo procedemos si hay un ID válido
                        if (row["id_persona"] == DBNull.Value) continue;

                        string sqlDelEst = @"DELETE FROM ""Estudios"" WHERE id_persona = @id";

                        using (var cmdDel = new NpgsqlCommand(sqlDelEst, conn))
                        {
                            cmdDel.Parameters.AddWithValue("@id", row["id_persona"]);

                            // Ejecutamos el borrado antes de que el siguiente proceso inserte los nuevos datos
                            await cmdDel.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    // --- BORRADO PREVIO DE DISCAPACIDAD EN LA NUBE ---
                    foreach (DataRow row in ds.Tables["Discapacidad"].Rows)
                    {
                        // Solo procedemos si hay un ID de persona válido
                        if (row["id_persona"] == DBNull.Value) continue;

                        // Borramos todos los registros de discapacidad para este documento
                        string sqlDelDisc = @"DELETE FROM ""Discapacidad"" WHERE id_persona = @id";

                        using (var cmdDel = new NpgsqlCommand(sqlDelDisc, conn))
                        {
                            cmdDel.Parameters.AddWithValue("@id", row["id_persona"]);

                            // Ejecutamos el borrado
                            await cmdDel.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    foreach (DataRow row in ds.Tables["Estudios"].Rows)
                        {
                            string sqlEst = @"INSERT INTO ""Estudios"" (id_persona, codigo, nivel, estado, grado, institucion, titulo) 
                                            VALUES (@id,@cod,@niv, @est, @gra, @ins, @tit)";
                            using (var cmdE = new NpgsqlCommand(sqlEst, conn))
                            {
                                cmdE.Parameters.AddWithValue("@id", row["id_persona"]);
                                cmdE.Parameters.AddWithValue("@cod", row["codigo"]);
                                cmdE.Parameters.AddWithValue("@niv", row["nivel"] ?? DBNull.Value);
                                cmdE.Parameters.AddWithValue("@est", row["estado"] ?? DBNull.Value);
                                cmdE.Parameters.AddWithValue("@gra", row["grado"] ?? DBNull.Value);
                                cmdE.Parameters.AddWithValue("@ins", row["institucion"] ?? DBNull.Value);
                                cmdE.Parameters.AddWithValue("@tit", row["titulo"] ?? DBNull.Value);
                                await cmdE.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }

                        // --- 6. REINSERTAR DISCAPACIDAD ---
                        foreach (DataRow row in ds.Tables["Discapacidad"].Rows)
                        {
                            string sqlDisc = @"INSERT INTO ""Discapacidad"" (id_persona, codigo, tipo, tratamiento, tipo_tratamiento) 
                                             VALUES (@id, @cod, @tip, @tra, @ttra)";
                            using (var cmdD = new NpgsqlCommand(sqlDisc, conn))
                            {
                                cmdD.Parameters.AddWithValue("@id", row["id_persona"]);
                                cmdD.Parameters.AddWithValue("@cod", row["codigo"]);
                                cmdD.Parameters.AddWithValue("@tip", row["tipo"] ?? DBNull.Value);
                                cmdD.Parameters.AddWithValue("@tra", row["tratamiento"] ?? DBNull.Value);
                                cmdD.Parameters.AddWithValue("@ttra", row["tipo_tratamiento"] ?? DBNull.Value);
                                await cmdD.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }

                        return true;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        MessageBox.Show(
                            "Error: ObjectDisposed:\n" +
                            ex.ObjectName + "\n\n" +
                            ex.StackTrace);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Error: Exception:\n" +
                            ex.GetType().FullName + "\n\n" +
                            ex.Message + "\n\n" +
                            ex.StackTrace);
                        throw;
                    }
            }
        }





        private async Task<bool> ProcesoPull()
        {
            try
            {
                // 1. Obtener la última fecha de sincronización y vigencia desde la configuración local
                // Asumo que tienes una tabla 'Configuracion' con estos campos
                DataTable dtConfig = ConexionDB.EjecutarConsulta("SELECT ultima_sincro FROM Configuracion LIMIT 1");
                DateTime ultimaSincroLocal = Convert.ToDateTime(dtConfig.Rows[0]["ultima_sincro"]);

                using (var conn = new ConexionNube().GetConexion())
                {
                    await conn.OpenAsync().ConfigureAwait(false);

                    // 2. RENOVAR TABLA USUARIOS (Eliminar y traer todo de la nube)
                    await RenovacionUsuariosAsync(conn).ConfigureAwait(false);

                    // 3. BUSCAR FAMILIAS NUEVAS O MODIFICADAS EN LA NUBE
                    string sqlBusca = "SELECT * FROM \"Familia\" WHERE fecha_sincronizacion > @ultima";
                    DataTable dtFamiliasNube = new DataTable();

                    using (var cmd = new NpgsqlCommand(sqlBusca, conn))
                    {
                        cmd.Parameters.AddWithValue("@ultima", ultimaSincroLocal);
                        using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            dtFamiliasNube.Load(reader);
                        }
                    }

                    int totalFamilias = dtFamiliasNube.Rows.Count;
                    double avancePorFamilia = 0;
                    double porcentaje = pbProgreso.Value;
                    if (totalFamilias != 0)
                    {
                        avancePorFamilia = 50.0 / totalFamilias;
                    }

                    // 4. PROCESAR CADA FAMILIA ENCONTRADA
                    foreach (DataRow filaNube in dtFamiliasNube.Rows)
                    {
                        string codFam = filaNube["Num_familia"].ToString();

                        // 5. DESCARGAR PAQUETE COMPLETO DE LA NUBE (Integrantes, Estudios, etc.)
                        DataSet dsPaqueteNube = await DescargarPaqueteNubeAsync(conn, codFam).ConfigureAwait(false);

                        // 1. Clonamos la estructura de la tabla original (esto copia las columnas, no los datos)
                        DataTable dtFilaFamilia = dtFamiliasNube.Clone();
                        dtFilaFamilia.TableName = "Familia";

                        // 2. Importamos la fila. ImportRow copia los datos y el estado de la fila 
                        // sin causar el error de pertenencia a otra tabla.
                        dtFilaFamilia.ImportRow(filaNube);

                        // 3. Ahora que la tabla tiene su propia copia de la fila, la añadimos al DataSet
                        dsPaqueteNube.Tables.Add(dtFilaFamilia);


                        // 6. ACTUALIZAR BASE DE DATOS LOCAL
                        await ActualizarLocalConPaqueteAsync(dsPaqueteNube).ConfigureAwait(false);
                        this.contador++;
                        lblDetalle.Text = $"Familia {contador} de {totalFamilias} descargada";
                        porcentaje += avancePorFamilia;
                        pbProgreso.Value =(int)(porcentaje);
                    }

                    lblDetalle.Text = "Solo faltan algunas configuraciones.";

                    // 7. ACTUALIZAR CONFIGURACIÓN FINAL (Nueva fecha y Vigencia)
                    await FinalizarConfiguracionPullAsync(conn).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico en el Pull: {ex.Message}");
                return false;
            }
        }

        private async Task<DataSet> DescargarPaqueteNubeAsync(NpgsqlConnection conn, string codFam)
        {
            DataSet ds = new DataSet();

            // Traer Censo_Anual
            string sqlCenso = "SELECT * FROM \"Censo_Anual\" WHERE num_familia = @cod";
            using (var cmd = new NpgsqlCommand(sqlCenso, conn))
            {
                cmd.Parameters.AddWithValue("@cod", codFam);
                DataTable dtCenso = new DataTable("Censo_Anual");
                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) { dtCenso.Load(reader); }
                ds.Tables.Add(dtCenso);
            }

            // Traer Estudios y Discapacidades basados en los integrantes encontrados
            if (ds.Tables["Censo_Anual"].Rows.Count > 0)
            {
                string sqlEst = "SELECT * FROM \"Estudios\" WHERE id_persona IN (SELECT numero_documento FROM \"Censo_Anual\" WHERE num_familia = @cod)";
                using (var cmd = new NpgsqlCommand(sqlEst, conn))
                {
                    cmd.Parameters.AddWithValue("@cod", codFam);
                    DataTable dtEst = new DataTable("Estudios");
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) { dtEst.Load(reader); }
                    ds.Tables.Add(dtEst);
                }

                string sqlDisc = "SELECT * FROM \"Discapacidad\" WHERE id_persona IN (SELECT numero_documento FROM \"Censo_Anual\" WHERE num_familia = @cod)";
                using (var cmd = new NpgsqlCommand(sqlDisc, conn))
                {
                    cmd.Parameters.AddWithValue("@cod", codFam);
                    DataTable dtDisc = new DataTable("Discapacidad");
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) { dtDisc.Load(reader); }
                    ds.Tables.Add(dtDisc);
                }

                string sqlPer = "SELECT * FROM \"Persona\" WHERE numero_documento IN (SELECT numero_documento FROM \"Censo_Anual\" WHERE num_familia = @cod)";
                using (var cmd = new NpgsqlCommand(sqlPer, conn))
                {
                    cmd.Parameters.AddWithValue("@cod", codFam);
                    DataTable dtPer = new DataTable("Persona");
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) { dtPer.Load(reader); }
                    ds.Tables.Add(dtPer);
                }
            }
            return ds;
        }

        private async Task ActualizarLocalConPaqueteAsync(DataSet dsNube)
        {
            // Extraemos la tabla Familia del DataSet (que agregaste manualmente)
            DataTable dtFam = dsNube.Tables["Familia"];
            if (dtFam == null || dtFam.Rows.Count == 0) return;

            DataRow rowFam = dtFam.Rows[0];
            string codFam = rowFam["Num_familia"].ToString();

            // 1. UPSERT DE FAMILIA LOCAL (SQLite)
            // Usamos INSERT OR REPLACE para simplificar la lógica de "Si existe actualiza"
            string sqlFam = @"
                            INSERT OR REPLACE INTO Familia (
                                Num_familia, direccion, telefono, correo_electronico, casa, 
                                tipo_documento_casa, tierra, tipo_documento_tierra, mina, 
                                tipo_documento_mina, num_fallecidos_año_anterior, Sincronizado
                            ) VALUES (
                                @num, @dir, @tel, @mail, @casa, @tdc, @tier, @tdt, @mina, @tdm, @fall, 'SI'
                            );";

            SqliteParameter[] pFam = {
                                        new SqliteParameter("@num", codFam),
                                        new SqliteParameter("@dir", rowFam["direccion"] ?? DBNull.Value),
                                        new SqliteParameter("@tel", rowFam["telefono"] ?? DBNull.Value),
                                        new SqliteParameter("@mail", rowFam["correo_electronico"] ?? DBNull.Value),
                                        new SqliteParameter("@casa", rowFam["casa"] ?? DBNull.Value),
                                        new SqliteParameter("@tdc", rowFam["tipo_documento_casa"] ?? DBNull.Value),
                                        new SqliteParameter("@tier", rowFam["tierra"] ?? DBNull.Value),
                                        new SqliteParameter("@tdt", rowFam["tipo_documento_tierra"] ?? DBNull.Value),
                                        new SqliteParameter("@mina", rowFam["mina"] ?? DBNull.Value),
                                        new SqliteParameter("@tdm", rowFam["tipo_documento_mina"] ?? DBNull.Value),
                                        new SqliteParameter("@fall", rowFam["num_fallecidos_año_anterior"] ?? 0)
                                    };
            ConexionDB.EjecutarOrden(sqlFam, pFam);

            // 2. LIMPIEZA DE DATOS VOLÁTILES LOCALES
            // Borramos en cascada manual para esta familia
            
            ConexionDB.EjecutarOrden($"DELETE FROM Censo_Anual WHERE num_familia = '{codFam}'");

            // 3. REINSERTAR PERSONAS (Datos Maestros)
            if (dsNube.Tables.Contains("Persona"))
            {
                foreach (DataRow p in dsNube.Tables["Persona"].Rows)
                {
                    string sqlPers = @"
                                    INSERT OR REPLACE INTO Persona (
                                        numero_documento, tipo_documento, primer_nombre, segundo_nombre, 
                                        primer_apellido, segundo_apellido, dia_nacimiento, mes_nacimiento, 
                                        año_nacimiento, lugar_nacimiento, genero, tipo_sangre
                                    ) VALUES (@doc, @td, @pn, @sn, @pa, @sa, @dn, @mn, @an, @ln, @gen, @ts)";

                    SqliteParameter[] pPers = {
                                                new SqliteParameter("@doc", p["numero_documento"]),
                                                new SqliteParameter("@td", p["tipo_documento"] ?? DBNull.Value),
                                                new SqliteParameter("@pn", p["primer_nombre"]?? DBNull.Value),
                                                new SqliteParameter("@sn", p["segundo_nombre"] ?? DBNull.Value),
                                                new SqliteParameter("@pa", p["primer_apellido"]?? DBNull.Value),
                                                new SqliteParameter("@sa", p["segundo_apellido"] ?? DBNull.Value),
                                                new SqliteParameter("@dn", p["dia_nacimiento"] ?? DBNull.Value),
                                                new SqliteParameter("@mn", p["mes_nacimiento"] ?? DBNull.Value),
                                                new SqliteParameter("@an", p["año_nacimiento"] ?? DBNull.Value),
                                                new SqliteParameter("@ln", p["lugar_nacimiento"] ?? DBNull.Value),
                                                new SqliteParameter("@gen", p["genero"] ?? DBNull.Value),
                                                new SqliteParameter("@ts", p["tipo_sangre"] ?? DBNull.Value)
                                            };
                    ConexionDB.EjecutarOrden(sqlPers, pPers);
                }
            }

            // 4. REINSERTAR CENSO_ANUAL
            if (dsNube.Tables.Contains("Censo_Anual"))
            {
                foreach (DataRow c in dsNube.Tables["Censo_Anual"].Rows)
                {
                    string sqlCenso = @"
                                    INSERT INTO Censo_Anual (
                                        numero_documento, vigencia, orientacion_sexual, estado_civil, cedula_pareja, 
                                        Parentesco, ocupacion, empresa, labor_comunidad, num_hijos_nacidos_vivos, 
                                        num_hijos_sobrevivientes, dia_ultimo_hijo, mes_ultimo_hijo, año_ultimo_hijo, 
                                        libreta_militar, direccion_actual, eps, regimen, tipo_censo, razon_ingreso, 
                                        familias_accion, renta_joven, adulto_mayor, Otros_apoyos, ultimo_editor, 
                                        num_familia, Sincronizado
                                    ) VALUES (
                                        @doc, @vig, @ori, @est, @pare, @par, @ocu, @emp, @lab, @nhv, 
                                        @nsob, @duh, @muh, @auh, @lib, @dir, @eps, @reg, @tce, @raz, 
                                        @fac, @rjo, @ama, @oap, @edit, @fam, 'SI'
                                    )";

                    SqliteParameter[] pCenso = {
                                                    new SqliteParameter("@doc", c["numero_documento"]),
                                                    new SqliteParameter("@vig", c["vigencia"]),
                                                    new SqliteParameter("@ori", c["orientacion_sexual"] ?? DBNull.Value),
                                                    new SqliteParameter("@est", c["estado_civil"] ?? DBNull.Value),
                                                    new SqliteParameter("@pare", c["cedula_pareja"] ?? 0),
                                                    new SqliteParameter("@par", c["Parentesco"] ?? DBNull.Value),
                                                    new SqliteParameter("@ocu", c["ocupacion"] ?? DBNull.Value),
                                                    new SqliteParameter("@emp", c["empresa"] ?? DBNull.Value),
                                                    new SqliteParameter("@lab", c["labor_comunidad"] ?? DBNull.Value),
                                                    new SqliteParameter("@nhv", c["num_hijos_nacidos_vivos"] ?? 0),
                                                    new SqliteParameter("@nsob", c["num_hijos_sobrevivientes"] ?? 0),
                                                    new SqliteParameter("@duh", c["dia_ultimo_hijo"] ?? DBNull.Value),
                                                    new SqliteParameter("@muh", c["mes_ultimo_hijo"] ?? DBNull.Value),
                                                    new SqliteParameter("@auh", c["año_ultimo_hijo"] ?? DBNull.Value),
                                                    new SqliteParameter("@lib", c["libreta_militar"] ?? DBNull.Value),
                                                    new SqliteParameter("@dir", c["direccion_actual"] ?? DBNull.Value),
                                                    new SqliteParameter("@eps", c["eps"] ?? DBNull.Value),
                                                    new SqliteParameter("@reg", c["regimen"] ?? DBNull.Value),
                                                    new SqliteParameter("@tce", c["tipo_censo"] ?? DBNull.Value),
                                                    new SqliteParameter("@raz", c["razon_ingreso"] ?? DBNull.Value),
                                                    new SqliteParameter("@fac", c["familias_accion"] ?? "No"),
                                                    new SqliteParameter("@rjo", c["renta_joven"] ?? "No"),
                                                    new SqliteParameter("@ama", c["adulto_mayor"] ?? "No"),
                                                    new SqliteParameter("@oap", c["Otros_apoyos"] ?? DBNull.Value),
                                                    new SqliteParameter("@edit", c["ultimo_editor"] ?? 1),
                                                    new SqliteParameter("@fam", codFam)
                                                };
                    ConexionDB.EjecutarOrden(sqlCenso, pCenso);
                }
            }
            // 5. REINSERTAR ESTUDIOS
            if (dsNube.Tables.Contains("Estudios"))
            {
                foreach (DataRow eb in dsNube.Tables["Estudios"].Rows)
                {
                    ConexionDB.EjecutarOrden($"DELETE FROM Estudios WHERE id_persona = '{eb["id_persona"]}'");
                }
                foreach (DataRow e in dsNube.Tables["Estudios"].Rows)
                {
                    string sqlEst = "INSERT INTO Estudios (id_persona, codigo, nivel, estado, grado, institucion, titulo) VALUES (@id, @cod, @niv, @est, @gra, @ins, @tit)";
                    SqliteParameter[] pEst = {
                                                new SqliteParameter("@id", e["id_persona"]),
                                                new SqliteParameter("@cod", e["codigo"]),
                                                new SqliteParameter("@niv", e["nivel"] ?? DBNull.Value),
                                                new SqliteParameter("@est", e["estado"] ?? DBNull.Value),
                                                new SqliteParameter("@gra", e["grado"] ?? DBNull.Value),
                                                new SqliteParameter("@ins", e["institucion"] ?? DBNull.Value),
                                                new SqliteParameter("@tit", e["titulo"] ?? DBNull.Value)
                                            };
                    ConexionDB.EjecutarOrden(sqlEst, pEst);
                }
            }

            // 6. REINSERTAR DISCAPACIDAD
            if (dsNube.Tables.Contains("Discapacidad"))
            {
                foreach (DataRow db in dsNube.Tables["Discapacidad"].Rows)
                {
                    ConexionDB.EjecutarOrden($"DELETE FROM Discapacidad WHERE id_persona = '{db["id_persona"]}'");
                }
                foreach (DataRow d in dsNube.Tables["Discapacidad"].Rows)
                {
                    string sqlDisc = "INSERT INTO Discapacidad (id_persona, codigo, tipo, tratamiento, tipo_tratamiento) VALUES (@id, @cod, @tip, @tra, @ttra)";
                    SqliteParameter[] pDisc = {
                new SqliteParameter("@id", d["id_persona"]),
                new SqliteParameter("@cod", d["codigo"]),
                new SqliteParameter("@tip", d["tipo"] ?? DBNull.Value),
                new SqliteParameter("@tra", d["tratamiento"] ?? DBNull.Value),
                new SqliteParameter("@ttra", d["tipo_tratamiento"] ?? DBNull.Value)
            };
                    ConexionDB.EjecutarOrden(sqlDisc, pDisc);
                }
            }
        }

        private async Task RenovacionUsuariosAsync(NpgsqlConnection conn)
        {
            // 1. Desvincular censos del editor actual para evitar el error de Foreign Key
            // Los asignamos al ID 1 (root) que siempre existe por tu Inicializar()
            ConexionDB.EjecutarOrden("UPDATE Censo_Anual SET ultimo_editor = 1");

            // 2. Ahora sí podemos borrar todos los usuarios excepto el root (ID 1)
            // Esto asegura que nunca te quedes bloqueado fuera del sistema
            ConexionDB.EjecutarOrden("DELETE FROM Usuario WHERE ID > 1");

            using (var cmd = new NpgsqlCommand("SELECT * FROM \"Usuario\"", conn))
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    // Omitimos insertar el root si viene de la nube para no duplicar ID 1
                    if (reader["username"].ToString().ToLower() == "root") continue;

                    SqliteParameter[] pUsua = {
                        new SqliteParameter("@id", reader["ID"]),
                        new SqliteParameter("@u", reader["username"]),
                        new SqliteParameter("@p", reader["contrasena"]),
                        new SqliteParameter("@r", reader["Rol"] ?? "Censista"),
                        new SqliteParameter("@c", reader["contador_familias"] ?? 0)
            };

                    // Usamos las columnas exactas de tu tabla local
                    string sqlIns = "INSERT INTO Usuario (ID, username, contrasena, Rol, contador_familias) VALUES (@id,@u, @p, @r, @c)";
                    ConexionDB.EjecutarOrden(sqlIns, pUsua);
                }
            }
        }
        private async Task FinalizarConfiguracionPullAsync(NpgsqlConnection conn)
        {
            // Obtenemos la vigencia y la hora actual del servidor de Supabase
            using (var cmd = new NpgsqlCommand("SELECT vigencia_activa, NOW() as hora FROM \"Configuracion\" LIMIT 1", conn))
            using (var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
            {
                if (await rdr.ReadAsync().ConfigureAwait(false))
                {
                    string vig = rdr["vigencia_activa"].ToString();
                    DateTime hora = Convert.ToDateTime(rdr["hora"]);
                    SqliteParameter[] pInCon = {
                        new SqliteParameter("@v", vig),
                        new SqliteParameter("@h", hora) };
                    // Actualizamos la tabla de configuración local para que el próximo pull sea desde este momento
                    ConexionDB.EjecutarOrden("UPDATE Configuracion SET vigencia_activa = @v, ultima_sincro = @h", pInCon);
                }
            }
        }

        // --- LÓGICA DEL BOTÓN INICIAR ---

        private async void button1_Click(object sender, EventArgs e)
        {
            ConexionNube clsnube = new ConexionNube();
            btnIniciar.Enabled = false; // Bloqueamos para evitar doble clic
            btn_cerrar_sesion.Enabled = false;
            this.contador = 0;

            if (sincronizando) return;
            sincronizando = true;

            try
            {
                // 1. ENCABEZADO: Verificar Conexión
                lblEstadoConexion.Text = "Verificando conexión..."; // Asumiendo que tienes un Label

                using (var conn = clsnube.GetConexion())
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    // Si llega aquí, está vivo
                    lblEstadoConexion.Text = "Conexión Activa";
                    lblEstadoConexion.ForeColor = Color.Green;
                }

                // 2. CONFIRMACIÓN PUSH
                DialogResult respPush = MessageBox.Show("¿Desea comenzar a montar los datos a la Nube?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (respPush == DialogResult.Yes)
                {
                    lblDetalle.Text = "Montando datos a la nube...";
                    pbProgreso.Value = 10;

                    bool exitoPush = await ProcesoPush().ConfigureAwait(false);

                    if (exitoPush)
                    {
                        pbProgreso.Value = 50;
                        lblDetalle.Text = "Carga a la nube finalizada.";
                    }
                }

                // 3. CONFIRMACIÓN PULL
                DialogResult respPull = MessageBox.Show($"Se subieron {this.contador} familias. ¿Desea comenzar a actualizar sus datos con la nube?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (respPull == DialogResult.Yes)
                {
                    lblDetalle.Text = "Descargando actualizaciones...";
                    this.contador = 0;

                    bool exitoPull = await ProcesoPull().ConfigureAwait(false);

                    if (exitoPull)
                    {
                        pbProgreso.Value = 100;
                        lblDetalle.Text = "¡Todo listo!";
                        MessageBox.Show($"Sincronización finalizada. {this.contador} familias procesadas correctamente.", "Éxito");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el proceso: " + ex.Message);
                lblEstadoConexion.Text = "Error de conexión";
                lblEstadoConexion.ForeColor = Color.Red;
            }
            finally
            {
                btnIniciar.Enabled = true;
                btn_cerrar_sesion.Enabled = true;
                sincronizando = false;
            }
        }

        private void btn_cerrar_sesion_Click(object sender, EventArgs e)
        {
            // Creamos el cuadro de diálogo
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar Sesión - Autocenso",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (resultado == DialogResult.Yes)
            {
                // Buscamos el formulario de Login por su nombre de clase
                Form login = Application.OpenForms["FormLogin"];

                if (login != null)
                {
                    login.Show();    // Lo volvemos a mostrar
                    this.Dispose();  // Destruimos el formulario principal para liberar memoria
                }
                else
                {
                    // Caso de emergencia: Si por alguna razón no lo encuentra, lo crea de nuevo
                    FormLogin nuevoLogin = new FormLogin();
                    nuevoLogin.Show();
                    this.Dispose();
                }
            }
        }
    }
}
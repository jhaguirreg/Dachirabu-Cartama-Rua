using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Autocenso_Cabildo
{
    public partial class FormListaCensados : Form
    {
        private int userId;
        private string usuarioActual;
        private string rolActual;
        private int vigenciaActual;

        // FORM:

        public FormListaCensados(int userId, string usuario, string rol, int vigencia)
        {
            InitializeComponent();

            cbb_comunidad.DropDownStyle = ComboBoxStyle.DropDownList;
            visor_personas.AllowUserToAddRows = false;
            visor_personas.AllowUserToDeleteRows = false;
            visor_personas.EditMode = DataGridViewEditMode.EditProgrammatically;


            this.userId = userId;
            this.usuarioActual = usuario;
            this.rolActual = rol;
            this.vigenciaActual = vigencia;

            // Opcional: Mostrar la información en el título de la ventana o en etiquetas
            this.Text = $"Autocenso Cabildo - Usuario: {usuario} | Vigencia: {vigencia}";
            this.lblrol.Text = this.rolActual;
            this.lblusuario.Text = this.usuarioActual;
            this.lblvigencia.Text = $"{this.vigenciaActual}";
        }

        private void FormListaCensados_Closed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void FormListaCensados_Load(object sender, EventArgs e)
        {
            CargarDireccionesVigenciaAnterior();
            visor_personas.RowPrePaint += visor_personas_RowPrePaint;

        }

        private void visor_personas_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = visor_personas;

            if (e.RowIndex >= grid.Rows.Count - 1) return;

            var famActual = grid.Rows[e.RowIndex].Cells["Familia"].Value;
            var famSiguiente = grid.Rows[e.RowIndex + 1].Cells["Familia"].Value;

            if (!Equals(famActual, famSiguiente))
            {
                grid.Rows[e.RowIndex].DividerHeight = 8; // separa este grupo del siguiente
            }
            else
            {
                grid.Rows[e.RowIndex].DividerHeight = 0;
            }
        }




        // BOTONES:

        private void btn_salir_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
        "¿Desea cerrar el sistema de Autocenso?\n\n",
        "Confirmar Salida",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Error);

            if (resultado == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void btn_cerrar_sesion_Click(object sender, EventArgs e)
        {
            // Creamos el cuadro de diálogo
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?\n\n",
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

        private void btn_volver_Click(object sender, EventArgs e)
        {

            // Buscamos el formulario de Login por su nombre de clase
            FormPrincipal mainform = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual, null, null);

            if (mainform != null)
            {
                mainform.Show();    // Lo volvemos a mostrar
                this.Dispose();  // Destruimos el formulario principal para liberar memoria
            }
        }

        private void btn_buscar_Click(object sender, EventArgs e)
        {
            // 1. Validar que haya una selección en el ComboBox
            if (cbb_comunidad.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione una comunidad/dirección primero.");
                return;
            }

            // 2. Obtener la dirección seleccionada (manejando si es un DataRow o Texto)
            string direccionSeleccionada = cbb_comunidad.Text;

            // 3. Preparar la consulta SQL
            // Traemos datos de Censo_Anual unidos con Persona y Familia
            // Calculamos la vigencia anterior
            int vigenciaAnterior = this.vigenciaActual - 1;
            string regimenAnterior = $"Regimen_{vigenciaAnterior}";

            string sql = @"
                        SELECT 
                            c.num_familia AS [Familia],
                            p.numero_documento AS [Documento],
                            p.primer_nombre || ' ' || IFNULL(p.segundo_nombre, '') || ' ' || 
                            p.primer_apellido || ' ' || IFNULL(p.segundo_apellido, '') AS [Nombre Completo],
                            c.Parentesco AS [Parentesco],
                            c.tipo_censo AS [Tipo Censo]
                        FROM Censo_Anual c
                        INNER JOIN Persona p ON c.numero_documento = p.numero_documento
                        INNER JOIN Familia f ON c.num_familia = f.Num_familia
                        WHERE f.direccion = @dir 
                            AND c.vigencia = @vigAnt
                            AND (c.tipo_censo = @regimen OR c.tipo_censo IN ('LISTA_ESPERA', 'SOLICITUD_ESTUDIO_ACEPTADA'))
                        ORDER BY c.num_familia ASC";

            SqliteParameter[] parametros = {
                                                new SqliteParameter("@dir", direccionSeleccionada),
                                                new SqliteParameter("@vigAnt", vigenciaAnterior),
                                                new SqliteParameter("@regimen", regimenAnterior)
                                            };

            // 4. Ejecutar y vincular
            DataTable dtResultados = ConexionDB.EjecutarConsulta(sql, parametros);

            if (dtResultados != null)
            {
                visor_personas.DataSource = dtResultados;
                // Ajuste opcional para que las columnas se vean bien
                visor_personas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Aquí puedes llamar a tu lógica de colores (Verde, Naranja, Rojo) que definimos antes
                AplicarColoresVisor();
               
            }
        }

        private void btn_censar_click(object sender, EventArgs e)
        {
            long documento_seleccionado = ObtenerDocumentoParaCenso();

            if (documento_seleccionado != 0)
            {

                // Buscamos el formulario de Login por su nombre de clase
                FormPrincipal mainform = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual, documento_seleccionado, null);

                if (mainform != null)
                {
                    mainform.Show();    // Lo volvemos a mostrar
                    this.Dispose();  // Destruimos el formulario principal para liberar memoria
                }
            }
        }

        private void btn_espera_click(object sender, EventArgs e)
        {
            long documento_seleccionado = ObtenerDocumentoParaCenso();

            if (documento_seleccionado != 0)
            {

                // Buscamos el formulario de Login por su nombre de clase
                FormPrincipal mainform = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual, documento_seleccionado, "LISTA_ESPERA");

                if (mainform != null)
                {
                    mainform.Show();    // Lo volvemos a mostrar
                    this.Dispose();  // Destruimos el formulario principal para liberar memoria
                }
            }
        }

        private void btn_retiro_click(object sender, EventArgs e)
        {
            long documento_seleccionado = ObtenerDocumentoParaCenso();

            if (documento_seleccionado != 0)
            {

                // Buscamos el formulario de Login por su nombre de clase
                FormPrincipal mainform = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual, documento_seleccionado, "INHABILITADO");

                if (mainform != null)
                {
                    mainform.Show();    // Lo volvemos a mostrar
                    this.Dispose();  // Destruimos el formulario principal para liberar memoria
                }
            }
        }

        // FUNCIONES:


        private void AplicarColoresVisor()
        {
            // 1. Obtener la vigencia actual (puedes traerla de tu tabla Configuracion)
            int vigenciaAct = this.vigenciaActual;

            // 2. Crear una consulta rápida para traer todos los documentos ya censados este año
            string sqlCheck = "SELECT numero_documento, Sincronizado FROM Censo_Anual WHERE vigencia = @vig";
            SqliteParameter[] p = { new SqliteParameter("@vig", vigenciaAct) };

            // Traemos esto a un DataTable temporal para comparar en memoria
            DataTable dtCensadosActual = ConexionDB.EjecutarConsulta(sqlCheck, p);

            // 3. Recorrer el visor y aplicar colores
            foreach (DataGridViewRow row in visor_personas.Rows)
            {
                if (row.Cells["Documento"].Value == null) continue;

                string docVisor = row.Cells["Documento"].Value.ToString();

                // Buscamos si el documento de la fila existe en nuestro DataTable de "ya censados"
                DataRow[] filaEncontrada = dtCensadosActual.Select($"numero_documento = {docVisor}");

                if (filaEncontrada.Length == 0)
                {
                    // CASO 1: No aparece en la vigencia actual
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 180, 180); // Rojo pastel.
                }
                else
                {
                    // Si existe, revisamos su estado de sincronización en la vigencia actual
                    string estadoSincro = filaEncontrada[0]["Sincronizado"].ToString().ToUpper();

                    if (estadoSincro == "SI" || estadoSincro == "1")
                    {
                        // CASO 2: Existe y está Sincronizado
                        row.DefaultCellStyle.BackColor = Color.LightGreen; // Verde
                    }
                    else
                    {
                        // CASO 3: Existe pero NO está sincronizado
                        row.DefaultCellStyle.BackColor = Color.Khaki; // Amarillo
                    }
                }
            }
        }

        private void CargarDireccionesVigenciaAnterior()
        {
            try
            {
                // Calculamos la vigencia anterior (si la actual es 2026, buscamos 2025)
                int vigenciaAnterior = this.vigenciaActual - 1;

                // SQL: Buscamos direcciones de familias que tienen registros en la vigencia pasada
                string sql = @"
                            SELECT DISTINCT f.direccion 
                            FROM Familia f
                            INNER JOIN Censo_Anual c ON f.Num_familia = c.num_familia
                            WHERE c.vigencia = @vigAnt
                            ORDER BY f.direccion ASC";

                    SqliteParameter[] p = {
                                            new SqliteParameter("@vigAnt", vigenciaAnterior)
                                          };

                // Ejecutamos usando tu método de la clase ConexionDB
                DataTable dt = ConexionDB.EjecutarConsulta(sql, p);

                // Limpiamos y asignamos al ComboBox
                cbb_comunidad.DataSource = null;
                if (dt != null && dt.Rows.Count > 0)
                {
                    cbb_comunidad.DataSource = dt;
                    cbb_comunidad.DisplayMember = "direccion";
                    cbb_comunidad.ValueMember = "direccion";

                    // Opcional: Dejar el combo sin selección inicial
                    cbb_comunidad.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las comunidades de la vigencia anterior: " + ex.Message);
            }
        }

        private long ObtenerDocumentoParaCenso()
        {
            // 1. Validar que haya una fila seleccionada
            if (visor_personas.SelectedRows.Count != 1)
            {
                MessageBox.Show("Seleccione una persona para poder realizar la acción.", "Aviso");
                return 0;
            }

            try
            {
                var fila = visor_personas.SelectedRows[0];

                // 2. Definir el color rojo de validación
                Color colorPendiente = Color.FromArgb(255, 180, 180);
                Color colorFila = fila.DefaultCellStyle.BackColor;

                // 3. Comprobar color (R, G, B)
                bool esRojo = colorFila.R == colorPendiente.R &&
                              colorFila.G == colorPendiente.G &&
                              colorFila.B == colorPendiente.B;

                if (esRojo)
                {
                    // Retornamos el valor de la columna "Documento"
                    return Convert.ToInt64(fila.Cells["Documento"].Value);
                }
                else
                {
                    MessageBox.Show("Solo se pueden seleccionar personas en estado sin censar (Rojo).", "Validación");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el documento: " + ex.Message);
                return 0;
            }
        }

    }
}

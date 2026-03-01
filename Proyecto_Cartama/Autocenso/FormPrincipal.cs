using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic; // Debes agregar la referencia al proyecto primero
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices; // Necesitas este using arriba
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Autocenso_Cabildo
{
    public partial class FormPrincipal : Form
    {
        // Variables para almacenar la información de la sesión
        private int userId;
        private string usuarioActual;
        private string rolActual;
        private int vigenciaActual;
        private string tipo_censo_persona;
        private string razon_ingreso_persona;
        private long documento_editando;
        private DataSet datos;

        // Dentro de tu clase Form
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        // Modificamos el constructor para recibir los 3 datos
        public FormPrincipal(int userId, string usuario, string rol, int vigencia, long? censar_este, string tipo_censo_para_este)
        {
            InitializeComponent();
            Grid_Vista_Integrantes.ReadOnly = false;
            InicializarGridIntegrantes();
            InicializarGridEstudios();
            InicializarGridDiscapacidad();

            // Asignamos los valores recibidos a nuestras variables locales
            this.userId = userId;
            this.usuarioActual = usuario;
            this.rolActual = rol;
            this.vigenciaActual = vigencia;

            // Razones de ingreso:
            this.razon_ingreso_persona = null;
            this.tipo_censo_persona = null;
            lblResultado.Visible = false;
            cbbTierra.Visible = false;
            cbbCasa.Visible = false;
            cbbMina.Visible = false;
            cbb2_Casa.Visible = false;
            cbb2_Mina.Visible = false;
            cbb2_Tierra.Visible = false;
            documento_editando = 0;

            // Opcional: Mostrar la información en el título de la ventana o en etiquetas
            this.Text = $"Autocenso Cabildo - Usuario: {usuario} | Vigencia: {vigencia}";
            this.lblrol.Text = this.rolActual;
            this.lblusuario.Text = this.usuarioActual;
            this.lblvigencia.Text = $"{this.vigenciaActual}";

            MostrarPasoIzq(Lyt_Paso_Inicial);
            MostrarPasoDer(lytImg_fondo);

            if (censar_este.HasValue)
            {
                if (!string.IsNullOrEmpty(tipo_censo_para_este))
                {
                    if (tipo_censo_para_este == "LISTA_ESPERA")
                    {
                        // Lógica específica para Lista de Espera
                        txtBusqueda.Text = $"{censar_este}";
                        btnProbar_Click(btnProbar, EventArgs.Empty);
                        ActualizarTipoYRazonCenso(this.datos, "LISTA_ESPERA",null);
                        CargarIntegrantesDesdeDS(this.datos);

                    }
                    else if (tipo_censo_para_este == "INHABILITADO")
                    {
                        // Lógica específica para Inhabilitados
                        txtBusqueda.Text = $"{censar_este}";
                        btnProbar_Click(btnProbar, EventArgs.Empty);
                        ActualizarTipoYRazonCenso(this.datos, "INHABILITADO","RETIRO INFORMADO");
                        CargarIntegrantesDesdeDS(this.datos);
                    }
                }
                else
                {
                    // Lógica cuando el tipo de censo es nulo o vacío
                    txtBusqueda.Text = $"{censar_este}";
                    btnProbar_Click(btnProbar, EventArgs.Empty);
                }
            }
        }

        private void principal_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void btn_cerrar_sesion_Click(object sender, EventArgs e)
        {
            // Creamos el cuadro de diálogo
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?\n\nCualquier dato que no haya sido guardado se perderá definitivamente.",
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

        public void ActualizarTipoYRazonCenso(DataSet ds, string nuevoTipo, string nuevaRazon)
        {
            if (ds != null && ds.Tables.Contains("Censo_Anual"))
            {
                DataTable tablaCenso = ds.Tables["Censo_Anual"];

                foreach (DataRow fila in tablaCenso.Rows)
                {
                    // 1. Siempre actualizamos el tipo (o podrías aplicar la misma lógica de null aquí)
                    if (nuevoTipo != null)
                    {
                        fila["tipo_censo"] = nuevoTipo;
                    }

                    // 2. Solo cambiamos la razón si el parámetro recibido NO es null
                    if (nuevaRazon != null)
                    {
                        fila["razon_ingreso"] = nuevaRazon;
                    }
                }
            }
        }

        private void btn_salir_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
        "¿Desea cerrar el sistema de Autocenso?\n\nSe perderán los datos que no hayan sido procesados.",
        "Confirmar Salida",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Error);

            if (resultado == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void principal_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si el cierre fue iniciado por el usuario (botón X o Alt+F4)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("¿Salir sin guardar cambios?", "Atención",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true; // Cancela el evento de cierre y mantiene el programa abierto
                }
            }
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            // 0x1501 es el mensaje para "Banner de sugerencia"
            // Para lado izquierdo:
            SendMessage(txtBusqueda.Handle, 0x1501, 0, "Ingrese el número de documento para buscar una familia.");
            SendMessage(Dirección.Handle, 0x1501, 0, "Ingrese la dirección de la familia.");
            SendMessage(Teléfono.Handle, 0x1501, 0, "Ingrese el teléfono de la familia.");

            SendMessage(Correo_electronico.Handle, 0x1501, 0, "Ingrese un correo electrónico de la familia.");
            SendMessage(txt2Direccion.Handle, 0x1501, 0, "Ingrese la dirección de la familia.");
            SendMessage(txt2Telefono.Handle, 0x1501, 0, "Ingrese el teléfono de la familia.");
            SendMessage(txt2CorreoElectronico.Handle, 0x1501, 0, "Ingrese un correo electrónico de la familia.");

            // Para lado derecho:
            SendMessage(text_num_doc.Handle, 0x1501, 0, "Ingrese el número del documento de identidad.");
            SendMessage(text_primer_nombre.Handle, 0x1501, 0, "Ingrese solamente el primer nombre.");
            SendMessage(text_segundo_nombre.Handle, 0x1501, 0, "Ingrese solamente el segundo nombre.");
            SendMessage(text_primer_apellido.Handle, 0x1501, 0, "Ingrese solamente el primer appelido.");
            SendMessage(text_segundo_apellido.Handle, 0x1501, 0, "Ingrese solamente el segundo apellido.");
            SendMessage(text_lugar_nacimiento.Handle, 0x1501, 0, "Ingrese el lugar de nacimiento.");
            SendMessage(text_direccion_persona.Handle, 0x1501, 0, "Ingrese la dirección actual de la persona.");
            SendMessage(text_cc_pareja.Handle, 0x1501, 0, "Ingrese el documento de identidad de su pareja en caso de tenerla y ser comunera.");

            SendMessage(text_ocupacion.Handle, 0x1501, 0, "Ingrese la ocupación u oficio principal.");
            SendMessage(text_empresa.Handle, 0x1501, 0, "Ingrese la empresa que lo contrata o donde labora.");
            SendMessage(text_labor_comunidad.Handle, 0x1501, 0, "Ingrese su labor en comunidad si aplica.");

            SendMessage(text_otros_apoyos.Handle, 0x1501, 0, "Ingrese otros apoyos o ayudas económicas en caso de aplicar.");

            cbbCasa.DropDownStyle = ComboBoxStyle.DropDownList;
            cbbMina.DropDownStyle = ComboBoxStyle.DropDownList;
            cbbTierra.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb2_Casa.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb2_Mina.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb2_Tierra.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_tip_doc.Items.AddRange(new object[]
{"CC", "TI", "RC", "NUIP" });
            cbb_tip_doc.DropDownStyle = ComboBoxStyle.DropDownList;

            for (int i = 1; i <= 31; i++)
            {
                cbb_dia_nacimiento.Items.Add(i.ToString());
                cbb_dia_hijo.Items.Add(i.ToString());
            }
            cbb_dia_nacimiento.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_dia_hijo.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_mes_nacimiento.Items.AddRange(new object[]
                {"Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" });
            cbb_mes_nacimiento.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_mes_hijo.Items.AddRange(new object[]
                {"Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" });
            cbb_mes_hijo.DropDownStyle = ComboBoxStyle.DropDownList;

            for (int i = this.vigenciaActual; i >= 1900; i--)
            {
                cbb_año_nacimiento.Items.Add(i.ToString());
                cbb_año_hijo.Items.Add(i.ToString());
            }
            cbb_año_nacimiento.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_año_hijo.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_tipo_sangre.Items.AddRange(new object[]
{"A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });
            cbb_tipo_sangre.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_estado_civil.Items.AddRange(new object[]
{"S", "C" });
            cbb_estado_civil.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_estado_civil.SelectedIndex = 0;

            cbb_genero.Items.AddRange(new object[]
{"F", "M" });
            cbb_genero.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_orientacion_sexual.Items.AddRange(new object[]
{"Heterosexual", "Homosexual", "Bisexual", "Pansexual", "Asexual", "Prefiero no decirlo" });
            cbb_orientacion_sexual.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_orientacion_sexual.SelectedIndex = 0;

            cbb_eps.Items.AddRange(new object[]
                {"MALLAMAS", "SALUD TOTAL S.A.", "SALUDVIDA S.A. E.P.S", "SAVIA SALUD", "ALIANSALUD ENTIDAD PROMOTORA DE SALUD S.A.", "ASOCIACIÓN INDÍGENA DEL CAUCA", "ASOCIACION MUTUAL SER EMPRESA SOLIDARIA DE SALUD EPS", "CAPITAL SALUD",
                "COMFENALCO VALLE E.P.S.", "COMPENSAR", "E.P.S. FAMISANAR LTDA.", "E.P.S. SANITAS S.A.", "EPS SERVICIO OCCIDENTAL DE SALUD S.A.", "NUEVA EPS S.A.", "PIJAOS SALUD EPSI", "OTRO" });
            cbb_eps.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_eps.SelectedIndex = 0;

            cbb_regimen.Items.AddRange(new object[]
                {"Subsidiado", "Contributivo" });
            cbb_regimen.DropDownStyle = ComboBoxStyle.DropDownList;

            // Para Estudio:
            cbb_nivel_estudio.Items.AddRange(new object[]
                {"PR", "SE", "UN", "NI" });
            cbb_nivel_estudio.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_estado_estudio.Items.AddRange(new object[]
                {"Finalizado", "En Curso", "Suspendido" });
            cbb_estado_estudio.DropDownStyle = ComboBoxStyle.DropDownList;

            SendMessage(textBox1ext_institucion_estudios.Handle, 0x1501, 0, "Ingrese el nombre de la institución donde cursó el estudio a registrar.");
            SendMessage(text_titulo_estudio.Handle, 0x1501, 0, "Ingrese el nombre específico del estudio.");
            SendMessage(text_grado_estudio.Handle, 0x1501, 0, "Ingrese el grado a cursar este año del estudio o semestre/periodo en caso de aplicar.");

            // Para discapacidad:
            cbb_tipo_discapacidad.Items.AddRange(new object[]
                {"Discapacidad Física", "Discapacidad Mental", "Discapacidad Intelectual", "Discapacidad Auditiva", "Discapacidad Visual" });
            cbb_tipo_discapacidad.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_tratamiento.Items.AddRange(new object[]
                {"Si", "No" });
            cbb_tratamiento.DropDownStyle = ComboBoxStyle.DropDownList;
            SendMessage(text_tipo_tratamiento.Handle, 0x1501, 0, "Ingrese quien financia el tratamiento o especificaciones sobre este.");

        }

        private void cmbEstadoCivil_SelectedIndexChanged(object sender, EventArgs e)
        {
            text_cc_pareja.Enabled = (cbb_estado_civil.Text == "C");
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            try
            {
                lblResultado.Visible = true;

                // 1. Validamos la entrada
                if (string.IsNullOrWhiteSpace(txtBusqueda.Text))
                {
                    lblResultado.Text = "Por favor, ingrese un número de documento.";
                    return;
                }

                long documento = long.Parse(txtBusqueda.Text);
                MainController mainCtrl = new MainController();

                mainCtrl.IncrementarContadorFamilias(userId);
                // 2. Llamamos al controlador principal
                DataSet ds = mainCtrl.ObtenerCensoCompleto(documento, this.userId, this.vigenciaActual);

                if (ds != null)
                {
                    // Ocultar etiqueta para que no aparezca una vez terminado el proceso.
                    lblResultado.Visible = false;

                    // Pasar los datos de vivienda.
                    DataTable dtFamilia = ds.Tables["Familia"];
                    LlenarCamposDesdeDataTable(dtFamilia);
                    CargarIntegrantesDesdeDS(ds);

                    this.datos = ds.Copy();

                    MostrarPasoIzq(Lyt_Datos_Familiares);
                    MostrarPasoDer(Lyt_lista_integrantes);
                }
                else
                {
                    lblResultado.Text = "No se encontró historial para este documento.";
                    lblResultado.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error: " + ex.Message);
            }
        }

        private void Casa_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cbCasa.Checked)
            {
                cbbCasa.Visible = true;
            }
            else
            {
                cbbCasa.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbbCasa.SelectedIndex = -1;
            }
        }

        private void Casa2_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cb2Casa.Checked)
            {
                cbb2_Casa.Visible = true;
            }
            else
            {
                cbb2_Casa.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbb2_Casa.SelectedIndex = -1;
            }
        }

        private void Mina_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cbMina.Checked)
            {
                cbbMina.Visible = true;
            }
            else
            {
                cbbMina.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbbMina.SelectedIndex = -1;
            }
        }

        private void Mina2_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cb2Mina.Checked)
            {
                cbb2_Mina.Visible = true;
            }
            else
            {
                cbb2_Mina.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbb2_Mina.SelectedIndex = -1;
            }
        }

        private void Tierra_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cbTierra.Checked)
            {
                cbbTierra.Visible = true;
            }
            else
            {
                cbbTierra.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbbTierra.SelectedIndex = -1;
            }
        }

        private void Tierra2_CheckedChanged(object sender, EventArgs e)
        {
            // La propiedad .Checked devuelve true si está marcado y false si no
            if (cb2Tierra.Checked)
            {
                cbb2_Tierra.Visible = true;
            }
            else
            {
                cbb2_Tierra.Visible = false;
                // Opcional: Limpiar la selección si se oculta
                cbb2_Tierra.SelectedIndex = -1;
            }
        }

        private void MostrarPasoIzq(TableLayoutPanel pasoActivo)
        {
            // Ocultas los TableLayoutPanel
            Lyt_Paso_Inicial.Visible = false;
            Lyt_Datos_Familiares.Visible = false;

            // Muestras el que necesitas
            pasoActivo.Visible = true;
            pasoActivo.BringToFront();
        }

        private void MostrarPasoDer(TableLayoutPanel pasoActivo)
        {
            int indiceFila = 0;
            if (pasoActivo == Lyt_lista_integrantes)
            {
                indiceFila = 1;
            }

            if (pasoActivo == lyt_datos_personales)
            {
                indiceFila = 2;
            }

            if (pasoActivo == lyt_estudios)
            {
                indiceFila = 3;
            }

            if (pasoActivo == lyt_discapacidad)
            {
                indiceFila = 4;
            }

            Lado_Derecho.SuspendLayout(); // Detiene el parpadeo

            for (int i = 0; i < Lado_Derecho.RowStyles.Count; i++)
            {
                Control control = Lado_Derecho.GetControlFromPosition(0, i);


                Lado_Derecho.RowStyles[i].SizeType = SizeType.Percent;
                Lado_Derecho.RowStyles[i].Height = 0;
                if (control != null) control.Visible = false;
            }

            // Fila activa: Le damos todo el espacio
            Control control1 = Lado_Derecho.GetControlFromPosition(0, indiceFila);

            Lado_Derecho.RowStyles[indiceFila].SizeType = SizeType.Percent;
            Lado_Derecho.RowStyles[indiceFila].Height = 100;
            if (control1 != null) control1.Visible = true;

            Lado_Derecho.ResumeLayout();
            Lado_Derecho.PerformLayout(); // Fuerza el redibujado total
        }

        private void AgregarNuevaFamilia_Click(object sender, EventArgs e)
        {
            MainController mainCtrl = new MainController();

            DataSet ds = mainCtrl.CrearDataSetCenso();

            // Asignaciones invertidas (del control principal al control de edición)
            txt2Direccion.Text = Dirección.Text;
            txt2Telefono.Text = Teléfono.Text;
            txt2CorreoElectronico.Text = Correo_electronico.Text;

            cb2Casa.Checked = cbCasa.Checked;
            cb2Tierra.Checked = cbTierra.Checked;
            cb2Mina.Checked = cbMina.Checked;

            cbb2_Casa.Text = cbbCasa.Text;
            cbb2_Mina.Text = cbbMina.Text;
            cbb2_Tierra.Text = cbbTierra.Text;

            numericUpDown1.Value = NumFallecidos.Value;

            DataTable familia = ds.Tables["Familia"];
            GuardarInterfazADataTable(familia);

            DataTable tablaConfig = ds.Tables["Configuracion"];

            // 2. Borramos todos los datos previos de la tabla
            tablaConfig.Clear();

            // 3. Creamos una nueva fila
            DataRow nuevaFila = tablaConfig.NewRow();

            mainCtrl.IncrementarContadorFamilias(this.userId);

            int valor_codigo_nuevo = mainCtrl.ObtenerContadorFamilias(this.userId);
            nuevaFila["CodFam"] = valor_codigo_nuevo;

            tablaConfig.Rows.Add(nuevaFila);

            this.datos = ds;

            DataTable familia_lleno = ds.Tables["Familia"];
            LlenarCamposDesdeDataTable(familia_lleno);

            MostrarPasoIzq(Lyt_Datos_Familiares);
            MostrarPasoDer(Lyt_lista_integrantes);
        }

        private void btn_cerrar_familia(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
        "¿Desea cerrar la familia?\n\nSe perderán los datos que no hayan sido guardados.",
        "Confirmar Salida",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Warning);

            if (resultado == DialogResult.OK)
            {
                // Creamos una nueva instancia del mismo formulario
                Form nombreDeTuForm = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual, null,null);
                nombreDeTuForm.Show();

                if (this.datos != null)
                {
                    this.datos.Clear();
                    this.datos.Dispose(); // Libera recursos no administrados
                }

                // Cerramos el actual
                this.Dispose();
                this.Close();
            }
        }

        private void btn_Agregar_persona_click(object sender, EventArgs e)
        {
            // Esto hace que el menú aparezca justo debajo del botón
            Opciones_ingreso.Show(btn_añadir_persona, 0, btn_añadir_persona.Height);
        }

        private void btnEliminarDatos_Click(object sender, EventArgs e)
        {
            DataSet dsCenso = this.datos.Copy();

            // 1. Obtener los valores clave desde el DataSet (Tabla Configuracion)
            if (dsCenso.Tables["Configuracion"].Rows.Count == 0) return;

            // Obtenemos los componentes del DataSet y de la clase
            int userId = this.userId; // Asumiendo que esta variable existe en tu clase
            string codFam = dsCenso.Tables["Configuracion"].Rows[0]["CodFam"].ToString();
            int vigencia = this.vigenciaActual;

            string codFamAEliminar = $"{userId}-{codFam}-{vigencia}";
            int vigenciaActiva = this.vigenciaActual;

            // Confirmación del usuario
            DialogResult result = MessageBox.Show($"¿Está seguro de eliminar los datos de la familia {codFamAEliminar} y el censo de la vigencia {vigenciaActiva}?",
                "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // --- PARTE 1: BASE DE DATOS (FÍSICO) ---

                    // A. Borrar registros en Censo_Anual para esa familia y vigencia
                    string sqlCenso = "DELETE FROM Censo_Anual WHERE num_familia = @codFam AND vigencia = @vig";
                    SqliteParameter[] paramCenso = {
                        new SqliteParameter("@codFam", codFamAEliminar),
                        new SqliteParameter("@vig", vigenciaActiva)
                    };
                    ConexionDB.EjecutarOrden(sqlCenso, paramCenso);

                    // B. Borrar la Familia
                    string sqlFamilia = "DELETE FROM Familia WHERE Num_familia = @codFam";
                    SqliteParameter[] paramFam = {
                        new SqliteParameter("@codFam", codFamAEliminar)
                    };
                    ConexionDB.EjecutarOrden(sqlFamilia, paramFam);

                    MessageBox.Show("Datos eliminados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Creamos una nueva instancia del mismo formulario
                    Form nombreDeTuForm = new FormPrincipal(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual,null,null);
                    nombreDeTuForm.Show();

                    if (this.datos != null)
                    {
                        this.datos.Clear();
                        this.datos.Dispose(); // Libera recursos no administrados
                    }

                    // Cerramos el actual
                    this.Dispose();
                    this.Close();


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message);
                }
            }
        }

        private void btnEliminarSeleccionados_Click(object sender, EventArgs e)
        {
            if (Grid_Vista_Integrantes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione una o más filas.");
                return;
            }

            DialogResult confirm = MessageBox.Show("¿Está seguro de eliminar las personas seleccionadas del Grid?",
                "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                DataSet dsCenso = this.datos;
                try
                {
                    List<long> documentosAEliminar = new List<long>();
                    foreach (DataGridViewRow fila in Grid_Vista_Integrantes.SelectedRows)
                    {
                        if (fila.Cells["numero_documento"].Value != null)
                        {
                            documentosAEliminar.Add(Convert.ToInt64(fila.Cells["numero_documento"].Value));
                        }
                    }

                    foreach (long documento in documentosAEliminar)
                    {
                        // Para la tabla Persona (usando la llave primaria)
                        DataRow filaPersona = dsCenso.Tables["Persona"].Rows.Find(documento);
                        if (filaPersona != null)
                        {
                            // Remove() requiere que pases la fila como argumento
                            dsCenso.Tables["Persona"].Rows.Remove(filaPersona);
                        }

                        // Para las tablas relacionales usando tu función auxiliar corregida
                        RemoverDeTablaDataset(dsCenso.Tables["Censo_Anual"], "numero_documento", documento);
                        RemoverDeTablaDataset(dsCenso.Tables["Estudios"], "id_persona", documento);
                        RemoverDeTablaDataset(dsCenso.Tables["Discapacidad"], "id_persona", documento);
                    }

                    // Ya no es estrictamente necesario AcceptChanges() con Remove, 
                    // pero es buena práctica para limpiar estados internos.
                    dsCenso.AcceptChanges();

                    // Refrescamos la vista
                    CargarIntegrantesDesdeDS(this.datos);

                    MessageBox.Show("Registros removidos del sistema local.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error durante la eliminación: " + ex.Message);
                }
            }
        }

        // Función auxiliar corregida para usar Remove
        private void RemoverDeTablaDataset(DataTable tabla, string nombreColumna, long id)
        {
            if (tabla == null) return;

            for (int i = tabla.Rows.Count - 1; i >= 0; i--)
            {
                if (Convert.ToInt64(tabla.Rows[i][nombreColumna]) == id)
                {
                    // El método correcto es tabla.Rows.Remove(DataRow)
                    // o tabla.Rows.RemoveAt(índice)
                    tabla.Rows.RemoveAt(i);
                }
            }
        }

        public void Descartar_integrante(object sender, EventArgs e)
        {
            MostrarPasoDer(Lyt_lista_integrantes);
            CargarIntegrantesDesdeDS(this.datos);
            LimpiarFormulario(lyt_datos_personales);
        }

        public void GuardarOActualizarIntegrante(object sender, EventArgs e)
        {
            DataSet dsCenso = this.datos;
            try
            {
                // 1. Validación básica de documento
                if (string.IsNullOrWhiteSpace(text_num_doc.Text))
                {
                    MessageBox.Show("El número de documento es obligatorio.");
                    return;
                }

                long numDoc = Convert.ToInt64(text_num_doc.Text);
                // --- SECCIÓN A: TABLA PERSONA ---
                DataTable tablaPersona = dsCenso.Tables["Persona"];
                DataRow filaPersona = tablaPersona.Rows.Find(numDoc);
                bool esNuevaPersona = (filaPersona == null);

                if (esNuevaPersona) filaPersona = tablaPersona.NewRow();

                // Mapeo de lyt_nombres y lyt_doc_identidad
                filaPersona["numero_documento"] = numDoc;
                filaPersona["tipo_documento"] = cbb_tip_doc.Text;
                filaPersona["primer_nombre"] = text_primer_nombre.Text;
                filaPersona["segundo_nombre"] = text_segundo_nombre.Text;
                filaPersona["primer_apellido"] = text_primer_apellido.Text;
                filaPersona["segundo_apellido"] = text_segundo_apellido.Text;

                // Mapeo de lyt_fecha_nacimiento y lyt_otros_datos
                filaPersona["dia_nacimiento"] = string.IsNullOrEmpty(cbb_dia_nacimiento.Text) ? 0 : Convert.ToInt32(cbb_dia_nacimiento.Text);
                filaPersona["mes_nacimiento"] = cbb_mes_nacimiento.Text;
                filaPersona["año_nacimiento"] = string.IsNullOrEmpty(cbb_año_nacimiento.Text) ? 0 : Convert.ToInt32(cbb_año_nacimiento.Text);
                filaPersona["lugar_nacimiento"] = text_lugar_nacimiento.Text;
                filaPersona["genero"] = cbb_genero.Text;
                filaPersona["tipo_sangre"] = cbb_tipo_sangre.Text;

                if (esNuevaPersona) tablaPersona.Rows.Add(filaPersona);

                // --- SECCIÓN B: TABLA CENSO_ANUAL ---
                DataTable tablaCenso = dsCenso.Tables["Censo_Anual"];
                // Buscamos si ya tiene censo este documento
                DataRow[] filasCenso = tablaCenso.Select($"numero_documento = {numDoc}");
                DataRow filaCenso = (filasCenso.Length > 0) ? filasCenso[0] : tablaCenso.NewRow();
                bool esNuevoCenso = (filasCenso.Length == 0);

                // Datos básicos y de salud (lyt_eps, lyt_otros_datos_2)
                filaCenso["numero_documento"] = numDoc;
                filaCenso["orientacion_sexual"] = cbb_orientacion_sexual.Text;
                filaCenso["estado_civil"] = cbb_estado_civil.Text;
                filaCenso["cedula_pareja"] = (cbb_estado_civil.Text == "C" && !string.IsNullOrEmpty(text_cc_pareja.Text))
                                              ? Convert.ToInt64(text_cc_pareja.Text) : 0;
                filaCenso["direccion_actual"] = text_direccion_persona.Text;

                filaCenso["eps"] = cbb_eps.Text;
                filaCenso["regimen"] = cbb_regimen.Text;

                // Datos laborales (lyt_datos_laborales)
                filaCenso["ocupacion"] = text_ocupacion.Text;
                filaCenso["empresa"] = text_empresa.Text;
                filaCenso["labor_comunidad"] = text_labor_comunidad.Text;

                // Datos de hijos (lyt_nacimiento_ultimo_hijo)
                filaCenso["num_hijos_nacidos_vivos"] = (int)Cont_hijos_nacidos_vivos.Value;
                filaCenso["num_hijos_sobrevivientes"] = (int)Cont_hijos_sobrevivientes.Value;
                filaCenso["dia_ultimo_hijo"] = string.IsNullOrEmpty(cbb_dia_hijo.Text) ? 0 : Convert.ToInt32(cbb_dia_hijo.Text);
                filaCenso["mes_ultimo_hijo"] = cbb_mes_hijo.Text;
                filaCenso["año_ultimo_hijo"] = string.IsNullOrEmpty(cbb_año_hijo.Text) ? 0 : Convert.ToInt32(cbb_año_hijo.Text);

                // Apoyos y otros (lyt_apoyos_economicos)
                filaCenso["adulto_mayor"] = cb_adulto_mayor.Checked ? "Si" : "No";
                filaCenso["renta_joven"] = cb_renta_joven.Checked ? "Si" : "No";
                filaCenso["familias_accion"] = cb_familias_accion.Checked ? "Si" : "No";
                filaCenso["libreta_militar"] = cb_libreta_militar.Checked ? "Si" : "No";
                filaCenso["Otros_apoyos"] = text_otros_apoyos.Text;
                if (esNuevoCenso)
                {
                    filaCenso["Parentesco"] = null;
                    filaCenso["tipo_censo"] = this.tipo_censo_persona;
                    filaCenso["razon_ingreso"] = this.razon_ingreso_persona;
                }

                // CODIGO PARA MARCAR FALLECIDOS.
                // Asumimos que filaCenso es tu DataRow y chkFallecido es tu CheckBox
                bool esFallecido = fallecido.Checked;
                string tipoCenso = filaCenso["tipo_censo"]?.ToString() ?? "";
                string razonActual = filaCenso["razon_ingreso"]?.ToString() ?? "";

                if (esFallecido)
                {
                    // CASO 1: Marcado como fallecido pero el tipo de censo aún no lo refleja
                    if (tipoCenso != "Fallecido")
                    {
                        // Guardamos la razón anterior combinada con el tipo de censo previo
                        filaCenso["razon_ingreso"] = $"{razonActual}/{tipoCenso}";
                        filaCenso["tipo_censo"] = "Fallecido";
                    }
                }
                else
                {
                    // CASO 2: No está marcado como fallecido pero el tipo de censo dice "Fallecido"
                    // Debemos restaurar los valores desde el string razon_ingreso (antes y después del /)
                    if (tipoCenso == "Fallecido" && razonActual.Contains("/"))
                    {
                        string[] partes = razonActual.Split('/');

                        // El primer elemento (antes del /) vuelve a ser la razón de ingreso
                        filaCenso["razon_ingreso"] = partes[0];

                        // El segundo elemento (después del /) vuelve a ser el tipo de censo
                        filaCenso["tipo_censo"] = partes[1];
                    }
                }

                if (esNuevoCenso) tablaCenso.Rows.Add(filaCenso);

                MessageBox.Show("Información guardada/actualizada con éxito.");
                MostrarPasoDer(lyt_estudios);
                this.documento_editando = numDoc;
                CargarEstudiosPorPersona();
                LimpiarFormulario(lyt_datos_personales);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar los datos: " + ex.Message);
            }
        }

        public void CargarDatosAControles(long documento)
        {
            // 1. Buscar las filas en las tablas correspondientes
            DataRow filaPersona = this.datos.Tables["Persona"].Rows.Find(documento);
            DataRow[] filasCenso = this.datos.Tables["Censo_Anual"].Select($"numero_documento = {documento}");

            if (filaPersona != null)
            {
                // Datos de Persona
                text_num_doc.Text = filaPersona["numero_documento"].ToString();
                cbb_tip_doc.Text = filaPersona["tipo_documento"].ToString();
                text_primer_nombre.Text = filaPersona["primer_nombre"].ToString();
                text_segundo_nombre.Text = filaPersona["segundo_nombre"].ToString();
                text_primer_apellido.Text = filaPersona["primer_apellido"].ToString();
                text_segundo_apellido.Text = filaPersona["segundo_apellido"].ToString();

                cbb_dia_nacimiento.Text = filaPersona["dia_nacimiento"].ToString();
                cbb_mes_nacimiento.Text = filaPersona["mes_nacimiento"].ToString();
                cbb_año_nacimiento.Text = filaPersona["año_nacimiento"].ToString();
                text_lugar_nacimiento.Text = filaPersona["lugar_nacimiento"].ToString();

                cbb_genero.Text = filaPersona["genero"].ToString();
                cbb_tipo_sangre.Text = filaPersona["tipo_sangre"].ToString();
            }

            if (filasCenso.Length > 0)
            {
                DataRow fC = filasCenso[0];
                // Datos de Censo
                cbb_orientacion_sexual.Text = fC["orientacion_sexual"].ToString();
                cbb_estado_civil.Text = fC["estado_civil"].ToString();
                text_cc_pareja.Text = fC["cedula_pareja"].ToString() == "0" ? "" : fC["cedula_pareja"].ToString();

                text_ocupacion.Text = fC["ocupacion"].ToString();
                text_empresa.Text = fC["empresa"].ToString();
                text_labor_comunidad.Text = fC["labor_comunidad"].ToString();

                if (fC["num_hijos_nacidos_vivos"] != DBNull.Value)
                {
                    Cont_hijos_nacidos_vivos.Value = Convert.ToDecimal(fC["num_hijos_nacidos_vivos"]);
                }
                else
                {
                    Cont_hijos_nacidos_vivos.Value = 0;
                }

                if (fC["num_hijos_sobrevivientes"] != DBNull.Value)
                {
                    Cont_hijos_sobrevivientes.Value = Convert.ToDecimal(fC["num_hijos_sobrevivientes"]);
                }
                else
                {
                    Cont_hijos_sobrevivientes.Value = 0;
                }

                // fC es tu DataRow (filaCenso)
                if (fC["tipo_censo"] != DBNull.Value)
                {
                    // Si el valor es "Fallecido", el checkbox se marca (True), de lo contrario se desmarca (False)
                    fallecido.Checked = (fC["tipo_censo"].ToString() == "Fallecido");
                }
                else
                {
                    fallecido.Checked = false;
                }

                cbb_dia_hijo.Text = fC["dia_ultimo_hijo"].ToString();
                cbb_mes_hijo.Text = fC["mes_ultimo_hijo"].ToString();
                cbb_año_hijo.Text = fC["año_ultimo_hijo"].ToString();

                text_direccion_persona.Text = fC["direccion_actual"].ToString();
                cbb_eps.Text = fC["eps"].ToString();
                cbb_regimen.Text = fC["regimen"].ToString();

                // Checkboxes (Asumiendo que guardas "S" para seleccionado)
                cb_adulto_mayor.Checked = fC["adulto_mayor"].ToString() == "Si";
                cb_renta_joven.Checked = fC["renta_joven"].ToString() == "Si";
                cb_familias_accion.Checked = fC["familias_accion"].ToString() == "Si";
                cb_libreta_militar.Checked = fC["libreta_militar"].ToString() == "Si";

                text_otros_apoyos.Text = fC["Otros_apoyos"].ToString();
            }
        }

        private void btnEditarIntegrante_Click(object sender, EventArgs e)
        {
            // 1. Validar que haya exactamente una fila seleccionada
            if (Grid_Vista_Integrantes.SelectedRows.Count != 1)
            {
                MessageBox.Show("Por favor, seleccione una (1) sola fila para editar.",
                                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 2. Extraer el documento de la fila seleccionada
                // Asumiendo que el nombre de la columna en el Grid es "numero_documento"
                var filaSeleccionada = Grid_Vista_Integrantes.SelectedRows[0];
                long documento = Convert.ToInt64(filaSeleccionada.Cells["numero_documento"].Value);

                // 3. Llamar a la función para cargar los datos en los controles
                CargarDatosAControles(documento);

                MostrarPasoDer(lyt_datos_personales);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar editar: " + ex.Message);
            }
        }

        public void LlenarCamposDesdeDataTable(DataTable dt)
        {
            // Verificamos que el DataTable tenga al menos una fila
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow fila = dt.Rows[0]; // Tomamos la primera familia encontrada

                // 1. Campos de Texto Simples
                txt2Direccion.Text = fila["direccion"]?.ToString();
                txt2Telefono.Text = fila["telefono"]?.ToString();
                txt2CorreoElectronico.Text = fila["correo_electronico"]?.ToString();

                // 2. Manejo de CheckBox (Casa, Tierra, Mina)
                // Si el valor es "Si", True o 1, se marca. Si es Null o "No", queda desmarcado.
                cb2Casa.Checked = ConvertirABooleano(fila["casa"]);
                cb2Tierra.Checked = ConvertirABooleano(fila["tierra"]);
                cb2Mina.Checked = ConvertirABooleano(fila["mina"]);

                // 3. ComboBoxes de Documentos (solo si el valor no es nulo)
                cbb2_Casa.Text = fila["tipo_documento_casa"]?.ToString();
                cbb2_Tierra.Text = fila["tipo_documento_tierra"]?.ToString();
                cbb2_Mina.Text = fila["tipo_documento_mina"]?.ToString();

                // 4. NumericUpDown para Fallecidos
                if (fila["num_fallecidos_año_anterior"] != DBNull.Value)
                {
                    numericUpDown1.Value = Convert.ToDecimal(fila["num_fallecidos_año_anterior"]);
                }
                else
                {
                    numericUpDown1.Value = 0;
                }
            }
        }

        // Función auxiliar para manejar la lógica de "Si/No" y Nulls
        private bool ConvertirABooleano(object valor)
        {
            if (valor == DBNull.Value || valor == null) return false;

            string texto = valor.ToString().ToUpper();
            return texto == "SI";
        }

        public void GuardarInterfazADataTable(DataTable dt)
        {
            dt.Clear();

            // Creamos una nueva fila con la estructura de la tabla 'Familia'
            DataRow nuevaFila = dt.NewRow();

            // 2. Campos de Texto (Si están vacíos, se guarda DBNull)
            nuevaFila["direccion"] = string.IsNullOrWhiteSpace(txt2Direccion.Text) ? DBNull.Value : (object)txt2Direccion.Text;
            nuevaFila["telefono"] = string.IsNullOrWhiteSpace(txt2Telefono.Text) ? DBNull.Value : (object)long.Parse(txt2Telefono.Text);
            nuevaFila["correo_electronico"] = string.IsNullOrWhiteSpace(txt2CorreoElectronico.Text) ? DBNull.Value : (object)txt2CorreoElectronico.Text;

            // 3. Lógica de CheckBoxes (Si es True -> "Si", si es False -> "No")
            nuevaFila["casa"] = cb2Casa.Checked ? "Si" : "No";
            nuevaFila["tierra"] = cb2Tierra.Checked ? "Si" : "No";
            nuevaFila["mina"] = cb2Mina.Checked ? "Si" : "No";

            // 4. ComboBoxes de Documentos
            nuevaFila["tipo_documento_casa"] = string.IsNullOrWhiteSpace(cbb2_Casa.Text) ? DBNull.Value : (object)cbb2_Casa.Text;
            nuevaFila["tipo_documento_tierra"] = string.IsNullOrWhiteSpace(cbb2_Tierra.Text) ? DBNull.Value : (object)cbb2_Tierra.Text;
            nuevaFila["tipo_documento_mina"] = string.IsNullOrWhiteSpace(cbb2_Mina.Text) ? DBNull.Value : (object)cbb2_Mina.Text;

            // 5. NumericUpDown y Sincronización
            nuevaFila["num_fallecidos_año_anterior"] = (int)numericUpDown1.Value;

            // Agregamos la fila terminada al DataTable
            dt.Rows.Add(nuevaFila);
        }

        public void InicializarGridIntegrantes()
        {
            Grid_Vista_Integrantes.Columns.Clear();
            Grid_Vista_Integrantes.AutoGenerateColumns = false;

            // Columnas normales (Solo lectura)
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "tipo_documento", HeaderText = "Tipo Doc.", Name = "tipo_documento", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "numero_documento", HeaderText = "Número Documento", Name = "numero_documento", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "primer_nombre", HeaderText = "Primer Nombre", Name = "primer_nombre", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "segundo_nombre", HeaderText = "Segundo Nombre", Name = "segundo_nombre", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "primer_apellido", HeaderText = "Primer Apellido", Name = "primer_apellido", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "segundo_apellido", HeaderText = "Segundo Apellido", Name = "segundo_apellido", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "tipo_censo", HeaderText = "Tipo Censo", Name = "tipo_censo", ReadOnly = true });
            Grid_Vista_Integrantes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "razon_ingreso", HeaderText = "Razón Ingreso", Name = "razon_ingreso", ReadOnly = true });

            // --- Columna de Parentesco como Selector (Editable) ---
            DataGridViewComboBoxColumn colParentesco = new DataGridViewComboBoxColumn();
            colParentesco.Name = "Parentesco";
            colParentesco.HeaderText = "Parentesco";
            colParentesco.DataPropertyName = "Parentesco"; // Vincula con el dato del DataSet

            // Agregamos las opciones del selector
            colParentesco.Items.AddRange("PA", "MA", "HE", "CF", "ES", "HI", "YR", "NU", "SU", "SO", "CU", "TI", "AB");

            colParentesco.ReadOnly = false; // Permite que el usuario cambie la opción
            colParentesco.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox; // Hace que siempre parezca un selector

            Grid_Vista_Integrantes.Columns.Add(colParentesco);
            Grid_Vista_Integrantes.Columns["Parentesco"].ReadOnly = false;
        }

        public class IntegranteFila
        {
            public string tipo_documento { get; set; }
            public long numero_documento { get; set; }
            public string primer_nombre { get; set; }
            public string segundo_nombre { get; set; }
            public string primer_apellido { get; set; }
            public string segundo_apellido { get; set; }
            public string tipo_censo { get; set; }
            public string razon_ingreso { get; set; }
            public string Parentesco { get; set; } // El ComboBox necesita este 'set'
        }

        public void CargarIntegrantesDesdeDS(DataSet ds)
        {
            if (ds.Tables.Contains("Persona") && ds.Tables.Contains("Censo_Anual"))
            {
                var consulta = from p in ds.Tables["Persona"].AsEnumerable()
                               join c in ds.Tables["Censo_Anual"].AsEnumerable()
                               on p.Field<long>("numero_documento") equals c.Field<long>("numero_documento")
                               where p.RowState != DataRowState.Deleted && c.RowState != DataRowState.Deleted
                               select new IntegranteFila // <--- CAMBIO AQUÍ
                               {
                                   tipo_documento = p.Field<string>("tipo_documento"),
                                   numero_documento = p.Field<long>("numero_documento"),
                                   primer_nombre = p.Field<string>("primer_nombre"),
                                   segundo_nombre = p.Field<string>("segundo_nombre"),
                                   primer_apellido = p.Field<string>("primer_apellido"),
                                   segundo_apellido = p.Field<string>("segundo_apellido"),
                                   tipo_censo = c.Field<string>("tipo_censo"),
                                   razon_ingreso = c.Field<string>("razon_ingreso"),
                                   Parentesco = c.Field<string>("Parentesco")
                               };

                // Al convertir a List de una clase con SET, el Grid habilita la edición
                Grid_Vista_Integrantes.DataSource = consulta.ToList();
            }
        }

        private void Grid_Vista_Integrantes_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Si la celda que seleccionó es la de Parentesco
            if (Grid_Vista_Integrantes.Columns[e.ColumnIndex].Name == "Parentesco")
            {
                // Forzamos la apertura del selector inmediatamente
                Grid_Vista_Integrantes.BeginEdit(true);
                if (Grid_Vista_Integrantes.EditingControl is System.Windows.Forms.ComboBox combo)
                {
                    combo.DroppedDown = true;
                }
            }
        }

        private void Grid_Vista_Integrantes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Validar que el cambio sea en la columna Parentesco y no sea el encabezado
            if (e.RowIndex >= 0 && Grid_Vista_Integrantes.Columns[e.ColumnIndex].Name == "Parentesco")
            {
                // 2. Obtener el valor seleccionado en el Grid
                string nuevoParentesco = Grid_Vista_Integrantes.Rows[e.RowIndex].Cells["Parentesco"].Value.ToString();
                long documento = (long)Grid_Vista_Integrantes.Rows[e.RowIndex].Cells["numero_documento"].Value;

                // 3. Buscar la fila real en el DataSet original para actualizarla
                DataRow[] filas = this.datos.Tables["Censo_Anual"].Select($"numero_documento = {documento}");

                if (filas.Length > 0)
                {
                    filas[0]["Parentesco"] = nuevoParentesco;
                    // Aquí el DataSet ya está actualizado en RAM
                }
            }
        }

        private void Grid_Vista_Integrantes_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (Grid_Vista_Integrantes.IsCurrentCellDirty)
            {
                // Esto fuerza a que CellValueChanged se dispare inmediatamente al elegir en el combo
                Grid_Vista_Integrantes.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void nacimientoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tipo_censo_persona = $"Regimen_{this.vigenciaActual}";
            this.razon_ingreso_persona = "NACIMIENTO";
            MostrarPasoDer(lyt_datos_personales);
        }

        private void trasladoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tipo_censo_persona = $"Regimen_{this.vigenciaActual}";
            this.razon_ingreso_persona = "TRASLADO";
            MostrarPasoDer(lyt_datos_personales);
        }

        private void solicitudDeIngresoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tipo_censo_persona = "SOLICITUD_ESTUDIO";
            this.razon_ingreso_persona = "SOLICITUD_ESTUDIO";
            MostrarPasoDer(lyt_datos_personales);
        }

        private void casoEspecialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string respuesta = Interaction.InputBox("Especifique la razón del ingreso:", "Ingreso caso especial", "");

            if (!string.IsNullOrEmpty(respuesta))
            {
                this.tipo_censo_persona = $"Regimen_{this.vigenciaActual}";
                this.razon_ingreso_persona = respuesta;
                MostrarPasoDer(lyt_datos_personales);
            }
        }

        private void cambioDeFamiliaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. Pedimos el dato con InputBox (requiere referencia a Microsoft.VisualBasic)
            string respuesta = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el documento de identidad a buscar:", "Busqueda de persona", "");

            // 2. Verificamos que no sea vacío o se haya cancelado
            if (!string.IsNullOrEmpty(respuesta))
            {
                long documento;

                // 3. Validamos que la respuesta sea un número largo (para documentos)
                if (long.TryParse(respuesta, out documento))
                {
                    PersonaController personaCtrl = new PersonaController();
                    FamiliaController familiaCtrl = new FamiliaController();

                    int vigencia_agregar = personaCtrl.ObtenerUltimaVigenciaCenso(documento);

                    string codfam_agregar = familiaCtrl.ObtenerCodigoFamilia(documento, vigencia_agregar);

                    if (!string.IsNullOrEmpty(codfam_agregar))
                    {
                        long[] documentos = familiaCtrl.ObtenerDocumentosIntegrantes(codfam_agregar, vigencia_agregar);

                        // 2. Recorremos con un for clásico
                        for (int i = 0; i < documentos.Length; i++)
                        {
                            long docActual = documentos[i];
                            personaCtrl.ObtenerInformacionPersonaCenso(docActual, vigencia_agregar, this.datos);
                            // 1. Buscamos la fila específica en la tabla Censo_Anual

                            DataRow[] filas = this.datos.Tables["Censo_Anual"].Select($"numero_documento = {docActual}");

                            if (filas.Length > 0)
                            {
                                // IMPORTANTE: Forzamos el fin de la edición para que el DataSet tome el cambio
                                if (vigencia_agregar != vigenciaActual)
                                {
                                    filas[0]["razon_ingreso"] = filas[0]["tipo_censo"];
                                    filas[0]["tipo_censo"] = $"Regimen_{vigenciaActual}";

                                    if (filas[0]["razon_ingreso"].ToString() == "SOLICITUD_ESTUDIO_RECHAZADA" || filas[0]["razon_ingreso"].ToString() == "SOLICITUD_ESTUDIO" || filas[0]["razon_ingreso"].ToString() == "INHABILITADO")
                                    {
                                        filas[0]["tipo_censo"] = filas[0]["razon_ingreso"];
                                    }
                                }
                            }
                        }

                        CargarIntegrantesDesdeDS(this.datos);
                    }
                    else
                    {
                        // --- CASO ERROR: Documento inexistente ---
                        MessageBox.Show($"El documento {respuesta} no existe en la base de datos.",
                                        "Documento inexistente",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Si el usuario ingresó letras
                    MessageBox.Show("Por favor, ingrese un número de documento válido.",
                                "Formato incorrecto",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                }
            }
        }

        public void LimpiarFormulario(Control contenedor)
        {
            foreach (Control c in contenedor.Controls)
            {
                if (c is System.Windows.Forms.TextBox)
                {
                    ((System.Windows.Forms.TextBox)c).Clear();
                }
                else if (c is System.Windows.Forms.ComboBox)
                {
                    ((System.Windows.Forms.ComboBox)c).SelectedIndex = -1; // Deja el combo vacío
                                                                           // O si prefieres que vuelva al primer item: ((ComboBox)c).SelectedIndex = 0;
                }
                else if (c is System.Windows.Forms.CheckBox)
                {
                    ((System.Windows.Forms.CheckBox)c).Checked = false;
                }
                else if (c is System.Windows.Forms.NumericUpDown)
                {
                    ((System.Windows.Forms.NumericUpDown)c).Value = 0;
                }

                // Si el control tiene hijos (como tus TableLayoutPanel), llamamos a la función de nuevo
                if (c.HasChildren)
                {
                    LimpiarFormulario(c);
                }
            }
        }

        public void InicializarGridEstudios()
        {
            Grid_estudios.AutoGenerateColumns = false;
            Grid_estudios.Columns.Clear();

            // 1. Columna Documento (NUEVA - No modificable)
            Grid_estudios.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "id_persona",
                Name = "id_persona",
                HeaderText = "Documento",
                ReadOnly = true,
                Width = 100
            });

            // 2. Columna Código (No modificable)
            Grid_estudios.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "codigo",
                Name = "codigo",
                HeaderText = "Cód",
                ReadOnly = true,
                Width = 40
            });

            // 3. Columna Nivel (ComboBox)
            DataGridViewComboBoxColumn colNivel = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "nivel",
                Name = "nivel",
                HeaderText = "Nivel",
                DataSource = new string[] { "PR", "SE", "UN", "NI" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            Grid_estudios.Columns.Add(colNivel);

            // 4. Columna Estado (ComboBox)
            DataGridViewComboBoxColumn colEstado = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "estado",
                Name = "estado",
                HeaderText = "Estado",
                DataSource = new string[] { "Finalizado", "En Curso", "Suspendido" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            Grid_estudios.Columns.Add(colEstado);

            // 5. Columnas de Texto
            Grid_estudios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "grado", Name = "grado", HeaderText = "Grado" });
            Grid_estudios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "institucion", Name = "institucion", HeaderText = "Institución" });
            Grid_estudios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "titulo", Name = "titulo", HeaderText = "Título" });

            Grid_estudios.AllowUserToAddRows = false;

            // Opcional: También puedes bloquear que borren filas con la tecla Suprimir
            Grid_estudios.AllowUserToDeleteRows = false;
        }

        public void CargarEstudiosPorPersona()
        {
            // Usamos el DataSet de la clase
            DataTable dtEstudios = this.datos.Tables["Estudios"];

            // Crear una vista filtrada para el Grid
            DataView vista = new DataView(dtEstudios);
            vista.RowFilter = $"id_persona = {this.documento_editando}";

            // Vinculamos al Grid
            Grid_estudios.DataSource = vista;

            // NOTA: Hemos eliminado las líneas de TableNewRow que daban error
        }

        // FUNCIÓN NUEVA: La que hace que los códigos siempre sean 1, 2, 3... sin huecos
        private void ReorganizarCodigosEstudio(long idPersona)
        {
            DataTable dtEstudios = this.datos.Tables["Estudios"];

            // Filtramos usando Field<long?> para que soporte nulos sin romperse
            var filasPersona = dtEstudios.AsEnumerable()
                .Where(r => r.RowState != DataRowState.Deleted &&
                            r.Field<long?>("id_persona") == idPersona)
                .OrderBy(r => r.Field<int?>("codigo") ?? 0)
                .ToList();

            for (int i = 0; i < filasPersona.Count; i++)
            {
                filasPersona[i]["codigo"] = i + 1;
            }
        }

        // BOTÓN NUEVO: Para poder borrar y que todo se re-organice
        private void btn_eliminar_estudio_Click(object sender, EventArgs e)
        {
            if (Grid_estudios.SelectedRows.Count != 1)
            {
                MessageBox.Show("Seleccione una fila completa para eliminar.");
                return;
            }

            long docActual = this.documento_editando;

            // Obtenemos la fila vinculada (DataRowView porque usamos DataView)
            DataRowView filaVista = (DataRowView)Grid_estudios.SelectedRows[0].DataBoundItem;
            filaVista.Row.Delete(); // Marcamos para borrar

            // REORGANIZAMOS: Al borrar, los números se ajustan
            ReorganizarCodigosEstudio(docActual);
        }

        private void btn_agregar_estudio_Click(object sender, EventArgs e)
        {
            try
            {
                long docActual = this.documento_editando;
                DataTable dtEstudios = this.datos.Tables["Estudios"];

                DataRow nuevaFila = dtEstudios.NewRow();
                nuevaFila["id_persona"] = docActual;
                nuevaFila["codigo"] = 0; // Valor temporal, se corregirá en la siguiente línea
                nuevaFila["nivel"] = cbb_nivel_estudio.Text;
                nuevaFila["estado"] = cbb_estado_estudio.Text;
                nuevaFila["grado"] = text_grado_estudio.Text;
                nuevaFila["institucion"] = textBox1ext_institucion_estudios.Text;
                nuevaFila["titulo"] = text_titulo_estudio.Text;

                dtEstudios.Rows.Add(nuevaFila);

                // LLAMADA CLAVE: Re-enumera todo (incluyendo el nuevo) del 1 al N
                ReorganizarCodigosEstudio(docActual);

                LimpiarFormulario(lyt_estudios);
                MessageBox.Show("Estudio agregado y lista numerada.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void InicializarGridDiscapacidad()
        {
            Grid_discapacidad.AutoGenerateColumns = false;
            Grid_discapacidad.Columns.Clear();

            // 1. Columnas de solo lectura
            Grid_discapacidad.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "id_persona",
                Name = "id_persona",
                HeaderText = "Documento",
                ReadOnly = true,
                Width = 100
            });
            Grid_discapacidad.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "codigo",
                Name = "codigo",
                HeaderText = "Cód",
                ReadOnly = true,
                Width = 40
            });

            // 2. Columna Tipo de Discapacidad (ComboBox)
            DataGridViewComboBoxColumn colTipo = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "tipo",
                Name = "tipo",
                HeaderText = "Tipo Discapacidad",
                DataSource = new string[] { "Discapacidad Física", "Discapacidad Mental", "Discapacidad Intelectual", "Discapacidad Auditiva", "Discapacidad Visual" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            Grid_discapacidad.Columns.Add(colTipo);

            // 3. Columna Tratamiento (ComboBox)
            DataGridViewComboBoxColumn colTratamiento = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "tratamiento",
                Name = "tratamiento",
                HeaderText = "¿Tratamiento?",
                DataSource = new string[] { "Si", "No" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            Grid_discapacidad.Columns.Add(colTratamiento);

            // 4. Columna Descripción (Texto libre)
            Grid_discapacidad.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "tipo_tratamiento",
                Name = "tipo_tratamiento",
                HeaderText = "Descripción Tratamiento",
                Width = 150
            });

            // Configuración de seguridad
            Grid_discapacidad.AllowUserToAddRows = false;
            Grid_discapacidad.AllowUserToDeleteRows = false;
        }

        public void CargarDiscapacidadPorPersona()
        {
            DataTable dt = this.datos.Tables["Discapacidad"];
            DataView vista = new DataView(dt);
            vista.RowFilter = $"id_persona = {this.documento_editando}";
            Grid_discapacidad.DataSource = vista;
        }

        private void ReorganizarCodigosDiscapacidad(long idPersona)
        {
            DataTable dt = this.datos.Tables["Discapacidad"];

            var filasPersona = dt.AsEnumerable()
                .Where(r => r.RowState != DataRowState.Deleted &&
                            r.Field<long?>("id_persona") == idPersona)
                .OrderBy(r => r.Field<int?>("codigo") ?? 0)
                .ToList();

            for (int i = 0; i < filasPersona.Count; i++)
            {
                filasPersona[i]["codigo"] = i + 1;
            }
        }

        private void btn_agregar_discapacidad_Click(object sender, EventArgs e)
        {
            try
            {
                long docActual = this.documento_editando;
                DataTable dt = this.datos.Tables["Discapacidad"];

                DataRow nuevaFila = dt.NewRow();
                nuevaFila["id_persona"] = docActual;
                nuevaFila["codigo"] = 0; // Temporal
                nuevaFila["tipo"] = cbb_tipo_discapacidad.Text;
                nuevaFila["tratamiento"] = cbb_tratamiento.Text;
                nuevaFila["tipo_tratamiento"] = text_tipo_tratamiento.Text;

                dt.Rows.Add(nuevaFila);

                ReorganizarCodigosDiscapacidad(docActual);

                // Limpiar solo los campos de este layout
                LimpiarFormulario(lyt_discapacidad);
                MessageBox.Show("Discapacidad agregada.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btn_eliminar_discapacidad_Click(object sender, EventArgs e)
        {
            if (Grid_discapacidad.SelectedRows.Count != 1) return;

            long docActual = this.documento_editando;
            DataRowView filaVista = (DataRowView)Grid_discapacidad.SelectedRows[0].DataBoundItem;

            filaVista.Row.Delete();
            ReorganizarCodigosDiscapacidad(docActual);
        }

        private void btn_pasar_discapacidad(object sender, EventArgs e)
        {
            Grid_estudios.DataSource = null;
            MostrarPasoDer(lyt_discapacidad);
            CargarDiscapacidadPorPersona();
        }

        private void btn_terminar_edicion(object sender, EventArgs e)
        {
            this.documento_editando = 0;
            CargarIntegrantesDesdeDS(this.datos);
            Grid_discapacidad.DataSource = null;
            MostrarPasoDer(Lyt_lista_integrantes);
        }

        public void GuardarCambiosCenso(object sender, EventArgs e)
        {
            DataSet ds = this.datos;
            using (var conexion = ConexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // --- 1. CONFIGURACIÓN DE VARIABLES ---
                        string numFamInterno = ds.Tables["Configuracion"].Rows[0]["CodFam"].ToString();
                        string codFam = $"{this.userId}-{numFamInterno}-{this.vigenciaActual}";

                        // --- 2. GUARDAR / ACTUALIZAR FAMILIA ---
                        DataTable dtFamilia = ds.Tables["Familia"];
                        GuardarInterfazADataTable(dtFamilia);
                        foreach (DataRow row in dtFamilia.Rows)
                        {
                            string sqlFam = @"INSERT INTO Familia (
                                Num_familia, direccion, telefono, correo_electronico, casa, 
                                tipo_documento_casa, tierra, tipo_documento_tierra, mina, 
                                tipo_documento_mina, num_fallecidos_año_anterior, Sincronizado
                            ) VALUES (
                                @num, @dir, @tel, @mail, @casa, 
                                @tdc, @tier, @tdt, @mina, 
                                @tdm, @fall, 'NO'
                            ) ON CONFLICT(Num_familia) DO UPDATE SET 
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
                                Sincronizado = 'NO';";

                            using (var cmd = new SqliteCommand(sqlFam, conexion, transaccion))
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
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // --- 3. REQUISITO: BORRAR PERSONAS DEL CENSO ANUAL ASOCIADAS A ESTA FAMILIA ---
                        // Esto "limpia" la familia para volver a llenarla con los integrantes del Dataset
                        string sqlLimpiarCenso = "DELETE FROM Censo_Anual WHERE num_familia = @codFam AND vigencia = @vig";
                        using (var cmdLimpiar = new SqliteCommand(sqlLimpiarCenso, conexion, transaccion))
                        {
                            cmdLimpiar.Parameters.AddWithValue("@codFam", codFam);
                            cmdLimpiar.Parameters.AddWithValue("@vig", this.vigenciaActual);
                            cmdLimpiar.ExecuteNonQuery();
                        }

                        // --- 4. GUARDAR PERSONAS (Maestro) Y CENSO ANUAL (Detalle) ---
                        DataTable dtPersonas = ds.Tables["Persona"];
                        foreach (DataRow row in dtPersonas.Rows)
                        {
                            // Validación de nulos para el documento (Evita el error de System.Int64)
                            if (row["numero_documento"] == DBNull.Value) continue;
                            long doc = Convert.ToInt64(row["numero_documento"]);

                            // A. Upsert en Persona (Datos maestros)
                            string sqlPers = @"INSERT INTO Persona (
                                            numero_documento, 
                                            tipo_documento, 
                                            primer_nombre, 
                                            segundo_nombre, 
                                            primer_apellido, 
                                            segundo_apellido, 
                                            dia_nacimiento, 
                                            mes_nacimiento, 
                                            año_nacimiento, 
                                            lugar_nacimiento, 
                                            genero, 
                                            tipo_sangre
                                        ) VALUES (
                                            @doc, @td, @pn, @sn, @pa, @sa, @dn, @mn, @an, @ln, @gen, @ts
                                        ) ON CONFLICT(numero_documento) DO UPDATE SET 
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

                            using (var cmd = new SqliteCommand(sqlPers, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@doc", doc);
                                cmd.Parameters.AddWithValue("@td", row["tipo_documento"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pn", row["primer_nombre"] ?? "");
                                cmd.Parameters.AddWithValue("@sn", row["segundo_nombre"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pa", row["primer_apellido"] ?? "");
                                cmd.Parameters.AddWithValue("@sa", row["segundo_apellido"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@dn", row["dia_nacimiento"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@mn", row["mes_nacimiento"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@an", row["año_nacimiento"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ln", row["lugar_nacimiento"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@gen", row["genero"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ts", row["tipo_sangre"] ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            // B. Limpieza de estudios/discapacidad para este documento específico
                            new SqliteCommand($"DELETE FROM Estudios WHERE id_persona = {doc}", conexion, transaccion).ExecuteNonQuery();
                            new SqliteCommand($"DELETE FROM Discapacidad WHERE id_persona = {doc}", conexion, transaccion).ExecuteNonQuery();
                        }

                        // --- 5. INSERTAR CENSO ANUAL (Vínculo Persona-Familia) ---
                        // 5. INSERTAR CENSO ANUAL (Vínculo Persona-Familia y datos socio-económicos)
                        DataTable dtCenso = ds.Tables["Censo_Anual"];
                        foreach (DataRow row in dtCenso.Rows)
                        {
                            if (row["numero_documento"] == DBNull.Value) continue;

                            string sqlInsCenso = @"INSERT OR REPLACE INTO Censo_Anual (
                                        numero_documento, vigencia, orientacion_sexual, estado_civil, cedula_pareja, 
                                        Parentesco, ocupacion, empresa, labor_comunidad, num_hijos_nacidos_vivos, 
                                        num_hijos_sobrevivientes, dia_ultimo_hijo, mes_ultimo_hijo, año_ultimo_hijo, 
                                        libreta_militar, direccion_actual, eps, regimen, tipo_censo, razon_ingreso, 
                                        familias_accion, renta_joven, adulto_mayor, Otros_apoyos, ultimo_editor, num_familia, Sincronizado
                                    ) VALUES (
                                        @doc, @vig, @ori, @est, @pare, @par, @ocu, @emp, @lab, @nhv, 
                                        @nsob, @duh, @muh, @auh, @lib, @dir, @eps, @reg, @tce, @raz, 
                                        @fac, @rjo, @ama, @oap, @edit, @fam, 'NO'
                                    )";

                            using (var cmd = new SqliteCommand(sqlInsCenso, conexion, transaccion))
                            {
                                // Parámetros Clave
                                cmd.Parameters.AddWithValue("@doc", row["numero_documento"]);
                                cmd.Parameters.AddWithValue("@vig", this.vigenciaActual);
                                cmd.Parameters.AddWithValue("@fam", codFam);
                                cmd.Parameters.AddWithValue("@edit", this.userId);

                                // Datos Personales/Sociales (con validación de nulos)
                                cmd.Parameters.AddWithValue("@ori", row["orientacion_sexual"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@est", row["estado_civil"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pare", row["cedula_pareja"] ?? 0);
                                cmd.Parameters.AddWithValue("@par", row["Parentesco"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ocu", row["ocupacion"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@emp", row["empresa"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@lab", row["labor_comunidad"] ?? DBNull.Value);

                                // Datos de Hijos
                                cmd.Parameters.AddWithValue("@nhv", row["num_hijos_nacidos_vivos"] ?? 0);
                                cmd.Parameters.AddWithValue("@nsob", row["num_hijos_sobrevivientes"] ?? 0);
                                cmd.Parameters.AddWithValue("@duh", row["dia_ultimo_hijo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@muh", row["mes_ultimo_hijo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@auh", row["año_ultimo_hijo"] ?? DBNull.Value);

                                // Salud y Ubicación
                                cmd.Parameters.AddWithValue("@lib", row["libreta_militar"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@dir", row["direccion_actual"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@eps", row["eps"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@reg", row["regimen"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@tce", row["tipo_censo"] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@raz", row["razon_ingreso"] ?? DBNull.Value);

                                // Programas de Apoyo
                                cmd.Parameters.AddWithValue("@fac", row["familias_accion"] ?? "No");
                                cmd.Parameters.AddWithValue("@rjo", row["renta_joven"] ?? "No");
                                cmd.Parameters.AddWithValue("@ama", row["adulto_mayor"] ?? "No");
                                cmd.Parameters.AddWithValue("@oap", row["Otros_apoyos"] ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 4. REINSERTAR ESTUDIOS
                        foreach (DataRow row in ds.Tables["Estudios"].Rows)
                        {
                            string sqlEst = "INSERT INTO Estudios (id_persona, codigo, nivel, estado, grado, institucion, titulo) VALUES (@id, @cod, @niv, @est, @gra, @ins, @tit)";
                            using (var cmd = new SqliteCommand(sqlEst, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@id", row["id_persona"]);
                                cmd.Parameters.AddWithValue("@cod", row["codigo"]);
                                cmd.Parameters.AddWithValue("@niv", row["nivel"]);
                                cmd.Parameters.AddWithValue("@est", row["estado"]);
                                cmd.Parameters.AddWithValue("@gra", row["grado"]);
                                cmd.Parameters.AddWithValue("@ins", row["institucion"]);
                                cmd.Parameters.AddWithValue("@tit", row["titulo"]);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 5. REINSERTAR DISCAPACIDAD
                        foreach (DataRow row in ds.Tables["Discapacidad"].Rows)
                        {
                            string sqlDisc = "INSERT INTO Discapacidad (id_persona, codigo, tipo, tratamiento, tipo_tratamiento) VALUES (@id, @cod, @tip, @tra, @ttra)";
                            using (var cmd = new SqliteCommand(sqlDisc, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@id", row["id_persona"]);
                                cmd.Parameters.AddWithValue("@cod", row["codigo"]);
                                cmd.Parameters.AddWithValue("@tip", row["tipo"]);
                                cmd.Parameters.AddWithValue("@tra", row["tratamiento"]);
                                cmd.Parameters.AddWithValue("@ttra", row["tipo_tratamiento"]);
                                cmd.ExecuteNonQuery();
                            }
                        }


                        transaccion.Commit();
                        MessageBox.Show("¡Datos sincronizados exitosamente!");
                    }
                    catch (Exception ex)
                    {
                        transaccion.Rollback();
                        MessageBox.Show("Error al guardar: " + ex.Message);
                    }
                }
            }
        }

        private void btn_ver_censados_Click(object sender, EventArgs e)
        {
            // Creamos el cuadro de diálogo
            DialogResult resultado = MessageBox.Show(
                "ADVERTENCIA: Cualquier dato que no haya sido guardado se perderá definitivamente.",
                "Ver Censados - Autocenso",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (resultado == DialogResult.Yes)
            {
                // Buscamos el formulario de Login por su nombre de clase
                FormListaCensados ver_censados = new FormListaCensados(this.userId, this.usuarioActual, this.rolActual, this.vigenciaActual);

                if (ver_censados != null)
                {
                    ver_censados.Show();    // Lo volvemos a mostrar
                    this.Dispose();  // Destruimos el formulario principal para liberar memoria
                }
            }
        }

    }
}

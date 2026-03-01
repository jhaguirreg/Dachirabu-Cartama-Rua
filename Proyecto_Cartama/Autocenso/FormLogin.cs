using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Autocenso_Cabildo
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            Rol.Items.Add("Censista");
            Rol.Items.Add("Carga y actualización");
            // 2. RESTRICCIÓN: Impedir que escriban (Solo lectura)
            Rol.DropDownStyle = ComboBoxStyle.DropDownList;
            // 3. Opcional: Seleccionar el primer elemento por defecto
            Rol.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Cierra absolutamente todo el programa y libera la memoria
            Environment.Exit(0);
            Application.Exit();
        }

        private void FormLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        public int ObtenerVigenciaDefecto()
        {
            using (var conexion = ConexionDB.ObtenerConexion())
            {
                conexion.Open();
                var cmd = new SqliteCommand("SELECT vigencia_activa FROM Configuracion WHERE ID = 1", conexion);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void inicio_sesion_Click(object sender, EventArgs e)
        {
            string usuarioIngresado = Usuario.Text.Trim();
            string claveIngresada = Contraseña.Text.Trim();
            Contraseña.Clear();
            Contraseña.Focus();

            if (string.IsNullOrEmpty(usuarioIngresado) || string.IsNullOrEmpty(claveIngresada))
            {
                MessageBox.Show("Por favor, ingrese usuario y contraseña.");
                return;
            }

            try
            {
                // Usamos el método que creamos en tu clase ConexionDB
                using (var conexion = ConexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string query = "SELECT ID, Rol FROM Usuario WHERE username = @user AND contrasena = @pass";

                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(query, conexion))
                    {
                        // Usamos parámetros para evitar ataques de Inyección SQL
                        comando.Parameters.AddWithValue("@user", usuarioIngresado);
                        comando.Parameters.AddWithValue("@pass", claveIngresada);

                        using (var lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                // ¡Login Exitoso! 
                                int userId = lector.GetInt32(0);
                                string rol = lector.GetString(1);

                                MessageBox.Show($"Bienvenido {usuarioIngresado}. Rol: {rol}");
                                if (Rol.SelectedIndex == 0)
                                {
                                    int vigencia = ObtenerVigenciaDefecto();

                                    // Aquí abres tu formulario principal (por ejemplo, FormMenu)
                                    FormPrincipal menu = new FormPrincipal(userId, usuarioIngresado, rol, vigencia,null,null);
                                    menu.Show();
                                    this.Hide();
                                }
                                else 
                                { 
                                    FormSincronizacion nube = new FormSincronizacion(userId);
                                    nube.Show();
                                    this.Hide();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Usuario o contraseña incorrectos.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos: " + ex.Message);
            }
        }
    }
}

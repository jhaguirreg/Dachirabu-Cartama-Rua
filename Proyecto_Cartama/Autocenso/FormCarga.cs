using System;
using System.Windows.Forms;
//using System.Media; // Para el sonido

namespace Autocenso_Cabildo
{
    public partial class pantalla_carga : Form
    {

        // Instanciamos el reproductor de sonido
        //SoundPlayer musicaInicio = new SoundPlayer(Properties.Resources.Tonoinicio);

        public pantalla_carga()
        {
            InitializeComponent();
            //ReproducirMusica();
            try
            {
                ConexionDB.Inicializar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al preparar el sistema de datos: " + ex.Message);
            }
        }

        //private void ReproducirMusica()
        //{
        //    try { musicaInicio.Play(); }
        //    catch { /* Evita que el programa se cierre si no hay audio */ }
        //}

        private void progressBar1_Click(object sender, EventArgs e)
        {
            //musicaInicio.Stop();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (barra_carga.Value < 98)
            {
                barra_carga.Value += 5; // Sube la barra
            }
            else
            {
                barra_carga.Value = 100;
            }

            if (barra_carga.Value >= 100)
            {
                //musicaInicio.Stop();
                Timer.Stop();

                // Abrir el siguiente paso (Login)
                // Esto ahora funcionará porque FrmLogin ya existe en tu proyecto
                FormLogin login = new FormLogin();
                login.Show();
                this.Hide();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //musicaInicio.Stop();
        }
    }
}

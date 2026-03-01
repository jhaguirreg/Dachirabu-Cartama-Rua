using System;
using System.Collections.Generic;
using System.Text;
using Npgsql; // Asegúrate de haber instalado el paquete NuGet Npgsql
using System.Windows.Forms; // Necesario para mostrar mensajes de error

namespace Autocenso_Cabildo
{
    internal class ConexionNube
    {
        // Esta es tu cadena de conexión optimizada para Colombia
        // Usamos el Host del Pooler, el Puerto 6543 y activamos SSL
        private string cadena = "Host=aws-1-us-east-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.iyqpoygxnndobtruknwq;Password=****;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Timeout=30;Command Timeout=30;No Reset On Close=true";

        public NpgsqlConnection GetConexion()
        {
            // Retorna el objeto de conexión configurado
            return new NpgsqlConnection(cadena);
        }

        // Método opcional para verificar rápidamente si hay internet y conexión a la nube
        public bool ProbarConexion()
        {
            try
            {
                using (var conn = GetConexion())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión a la nube: " + ex.Message);
                return false;
            }
        }
    }
}
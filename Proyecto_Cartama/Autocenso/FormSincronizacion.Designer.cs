namespace Autocenso_Cabildo
{
    partial class FormSincronizacion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSincronizacion));
            btnIniciar = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblEstadoConexion = new Label();
            lblDetalle = new Label();
            pbProgreso = new ProgressBar();
            btn_cerrar_sesion = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnIniciar
            // 
            btnIniciar.Dock = DockStyle.Fill;
            btnIniciar.Location = new Point(3, 55);
            btnIniciar.Name = "btnIniciar";
            btnIniciar.Size = new Size(278, 46);
            btnIniciar.TabIndex = 0;
            btnIniciar.Text = "Comenzar proceso de sincronización";
            btnIniciar.UseVisualStyleBackColor = true;
            btnIniciar.Click += button1_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.DarkSeaGreen;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(btn_cerrar_sesion, 0, 4);
            tableLayoutPanel1.Controls.Add(lblEstadoConexion, 0, 0);
            tableLayoutPanel1.Controls.Add(btnIniciar, 0, 1);
            tableLayoutPanel1.Controls.Add(lblDetalle, 0, 3);
            tableLayoutPanel1.Controls.Add(pbProgreso, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(284, 261);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // lblEstadoConexion
            // 
            lblEstadoConexion.Dock = DockStyle.Fill;
            lblEstadoConexion.Font = new Font("Times New Roman", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblEstadoConexion.ForeColor = Color.White;
            lblEstadoConexion.Location = new Point(3, 0);
            lblEstadoConexion.Name = "lblEstadoConexion";
            lblEstadoConexion.Size = new Size(278, 52);
            lblEstadoConexion.TabIndex = 1;
            lblEstadoConexion.Text = "Verificando...";
            lblEstadoConexion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDetalle
            // 
            lblDetalle.AutoSize = true;
            lblDetalle.Dock = DockStyle.Fill;
            lblDetalle.Font = new Font("Times New Roman", 15.75F, FontStyle.Bold);
            lblDetalle.ForeColor = Color.White;
            lblDetalle.Location = new Point(3, 156);
            lblDetalle.Name = "lblDetalle";
            lblDetalle.Size = new Size(278, 52);
            lblDetalle.TabIndex = 3;
            lblDetalle.Text = "Esperando instrucciones...";
            lblDetalle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pbProgreso
            // 
            pbProgreso.Dock = DockStyle.Fill;
            pbProgreso.Location = new Point(3, 107);
            pbProgreso.Name = "pbProgreso";
            pbProgreso.Size = new Size(278, 46);
            pbProgreso.TabIndex = 2;
            // 
            // btn_cerrar_sesion
            // 
            btn_cerrar_sesion.BackColor = Color.Orange;
            btn_cerrar_sesion.Dock = DockStyle.Fill;
            btn_cerrar_sesion.ForeColor = SystemColors.Control;
            btn_cerrar_sesion.Location = new Point(3, 211);
            btn_cerrar_sesion.Name = "btn_cerrar_sesion";
            btn_cerrar_sesion.Size = new Size(278, 47);
            btn_cerrar_sesion.TabIndex = 4;
            btn_cerrar_sesion.Text = "Cerrar Sesión";
            btn_cerrar_sesion.UseVisualStyleBackColor = false;
            btn_cerrar_sesion.Click += btn_cerrar_sesion_Click;
            // 
            // FormSincronizacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 261);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormSincronizacion";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pestaña de Sincronización";
            FormClosed += FormSincronizacionClosed;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnIniciar;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblEstadoConexion;
        private ProgressBar pbProgreso;
        private Label lblDetalle;
        private Button btn_cerrar_sesion;
    }
}
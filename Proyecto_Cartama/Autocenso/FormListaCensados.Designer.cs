namespace Autocenso_Cabildo
{
    partial class FormListaCensados
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormListaCensados));
            Layot_principal = new TableLayoutPanel();
            lyt_cabecera = new TableLayoutPanel();
            btn_salir = new Button();
            btn_cerrar_sesion = new Button();
            label1 = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            lblvigencia = new Label();
            lblusuario = new Label();
            lblrol = new Label();
            btn_ver_censados = new Button();
            lyt_buscar_comunidades = new TableLayoutPanel();
            label2 = new Label();
            cbb_comunidad = new ComboBox();
            btn_buscar = new Button();
            lyt_botones = new TableLayoutPanel();
            btn_censar = new Button();
            btn_marcar_retirado = new Button();
            btn_dejar_espera = new Button();
            visor_personas = new DataGridView();
            Layot_principal.SuspendLayout();
            lyt_cabecera.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            lyt_buscar_comunidades.SuspendLayout();
            lyt_botones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)visor_personas).BeginInit();
            SuspendLayout();
            // 
            // Layot_principal
            // 
            Layot_principal.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            Layot_principal.ColumnCount = 1;
            Layot_principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Layot_principal.Controls.Add(lyt_cabecera, 0, 0);
            Layot_principal.Controls.Add(lyt_buscar_comunidades, 0, 1);
            Layot_principal.Controls.Add(lyt_botones, 0, 3);
            Layot_principal.Controls.Add(visor_personas, 0, 2);
            Layot_principal.Dock = DockStyle.Fill;
            Layot_principal.Location = new Point(0, 0);
            Layot_principal.Name = "Layot_principal";
            Layot_principal.RowCount = 4;
            Layot_principal.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            Layot_principal.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            Layot_principal.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            Layot_principal.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            Layot_principal.Size = new Size(800, 450);
            Layot_principal.TabIndex = 1;
            // 
            // lyt_cabecera
            // 
            lyt_cabecera.BackColor = Color.SeaGreen;
            lyt_cabecera.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            lyt_cabecera.ColumnCount = 5;
            lyt_cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            lyt_cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            lyt_cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            lyt_cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            lyt_cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            lyt_cabecera.Controls.Add(btn_salir, 4, 0);
            lyt_cabecera.Controls.Add(btn_cerrar_sesion, 3, 0);
            lyt_cabecera.Controls.Add(label1, 0, 0);
            lyt_cabecera.Controls.Add(tableLayoutPanel3, 1, 0);
            lyt_cabecera.Controls.Add(btn_ver_censados, 2, 0);
            lyt_cabecera.Dock = DockStyle.Fill;
            lyt_cabecera.Location = new Point(5, 5);
            lyt_cabecera.Name = "lyt_cabecera";
            lyt_cabecera.RowCount = 1;
            lyt_cabecera.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lyt_cabecera.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            lyt_cabecera.Size = new Size(790, 82);
            lyt_cabecera.TabIndex = 0;
            // 
            // btn_salir
            // 
            btn_salir.BackColor = Color.Red;
            btn_salir.Dock = DockStyle.Fill;
            btn_salir.ForeColor = SystemColors.Control;
            btn_salir.Location = new Point(727, 5);
            btn_salir.Name = "btn_salir";
            btn_salir.Size = new Size(58, 72);
            btn_salir.TabIndex = 4;
            btn_salir.Text = "Salir";
            btn_salir.UseVisualStyleBackColor = false;
            btn_salir.Click += btn_salir_Click;
            // 
            // btn_cerrar_sesion
            // 
            btn_cerrar_sesion.BackColor = Color.Orange;
            btn_cerrar_sesion.Dock = DockStyle.Fill;
            btn_cerrar_sesion.ForeColor = SystemColors.Control;
            btn_cerrar_sesion.Location = new Point(663, 5);
            btn_cerrar_sesion.Name = "btn_cerrar_sesion";
            btn_cerrar_sesion.Size = new Size(56, 72);
            btn_cerrar_sesion.TabIndex = 3;
            btn_cerrar_sesion.Text = "Cerrar Sesión";
            btn_cerrar_sesion.UseVisualStyleBackColor = false;
            btn_cerrar_sesion.Click += btn_cerrar_sesion_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(5, 2);
            label1.Name = "label1";
            label1.Size = new Size(460, 78);
            label1.TabIndex = 0;
            label1.Text = "LISTA DE CENSADOS";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(lblvigencia, 0, 2);
            tableLayoutPanel3.Controls.Add(lblusuario, 0, 1);
            tableLayoutPanel3.Controls.Add(lblrol, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(473, 5);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel3.Size = new Size(118, 72);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // lblvigencia
            // 
            lblvigencia.AutoSize = true;
            lblvigencia.Dock = DockStyle.Fill;
            lblvigencia.Font = new Font("Times New Roman", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblvigencia.ForeColor = Color.White;
            lblvigencia.Location = new Point(3, 48);
            lblvigencia.Name = "lblvigencia";
            lblvigencia.Size = new Size(112, 24);
            lblvigencia.TabIndex = 2;
            lblvigencia.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblusuario
            // 
            lblusuario.AutoSize = true;
            lblusuario.Dock = DockStyle.Fill;
            lblusuario.Font = new Font("Times New Roman", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblusuario.ForeColor = Color.White;
            lblusuario.Location = new Point(3, 24);
            lblusuario.Name = "lblusuario";
            lblusuario.Size = new Size(112, 24);
            lblusuario.TabIndex = 1;
            lblusuario.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblrol
            // 
            lblrol.AutoSize = true;
            lblrol.Dock = DockStyle.Fill;
            lblrol.Font = new Font("Times New Roman", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblrol.ForeColor = Color.White;
            lblrol.Location = new Point(3, 0);
            lblrol.Name = "lblrol";
            lblrol.Size = new Size(112, 24);
            lblrol.TabIndex = 0;
            lblrol.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btn_ver_censados
            // 
            btn_ver_censados.BackColor = Color.DarkGreen;
            btn_ver_censados.Dock = DockStyle.Fill;
            btn_ver_censados.ForeColor = SystemColors.Control;
            btn_ver_censados.Location = new Point(599, 5);
            btn_ver_censados.Name = "btn_ver_censados";
            btn_ver_censados.Size = new Size(56, 72);
            btn_ver_censados.TabIndex = 2;
            btn_ver_censados.Text = "Volver";
            btn_ver_censados.UseVisualStyleBackColor = false;
            btn_ver_censados.Click += btn_volver_Click;
            // 
            // lyt_buscar_comunidades
            // 
            lyt_buscar_comunidades.BackColor = Color.DarkSeaGreen;
            lyt_buscar_comunidades.ColumnCount = 3;
            lyt_buscar_comunidades.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            lyt_buscar_comunidades.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            lyt_buscar_comunidades.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            lyt_buscar_comunidades.Controls.Add(label2, 0, 0);
            lyt_buscar_comunidades.Controls.Add(cbb_comunidad, 1, 0);
            lyt_buscar_comunidades.Controls.Add(btn_buscar, 2, 0);
            lyt_buscar_comunidades.Dock = DockStyle.Fill;
            lyt_buscar_comunidades.Font = new Font("Times New Roman", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lyt_buscar_comunidades.ForeColor = Color.White;
            lyt_buscar_comunidades.Location = new Point(5, 95);
            lyt_buscar_comunidades.Name = "lyt_buscar_comunidades";
            lyt_buscar_comunidades.RowCount = 1;
            lyt_buscar_comunidades.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lyt_buscar_comunidades.Size = new Size(790, 16);
            lyt_buscar_comunidades.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.ControlLightLight;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(254, 16);
            label2.TabIndex = 0;
            label2.Text = "Comunidad a buscar";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cbb_comunidad
            // 
            cbb_comunidad.Dock = DockStyle.Fill;
            cbb_comunidad.FormattingEnabled = true;
            cbb_comunidad.Location = new Point(263, 3);
            cbb_comunidad.Name = "cbb_comunidad";
            cbb_comunidad.Size = new Size(262, 25);
            cbb_comunidad.TabIndex = 1;
            // 
            // btn_buscar
            // 
            btn_buscar.BackColor = Color.SeaGreen;
            btn_buscar.Dock = DockStyle.Fill;
            btn_buscar.Location = new Point(531, 3);
            btn_buscar.Name = "btn_buscar";
            btn_buscar.Size = new Size(256, 10);
            btn_buscar.TabIndex = 2;
            btn_buscar.Text = "Buscar";
            btn_buscar.UseVisualStyleBackColor = false;
            btn_buscar.Click += btn_buscar_Click;
            // 
            // lyt_botones
            // 
            lyt_botones.BackColor = Color.DarkSeaGreen;
            lyt_botones.ColumnCount = 3;
            lyt_botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            lyt_botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            lyt_botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            lyt_botones.Controls.Add(btn_censar, 0, 0);
            lyt_botones.Controls.Add(btn_marcar_retirado, 2, 0);
            lyt_botones.Controls.Add(btn_dejar_espera, 1, 0);
            lyt_botones.Dock = DockStyle.Fill;
            lyt_botones.Location = new Point(5, 429);
            lyt_botones.Name = "lyt_botones";
            lyt_botones.RowCount = 1;
            lyt_botones.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lyt_botones.Size = new Size(790, 16);
            lyt_botones.TabIndex = 2;
            // 
            // btn_censar
            // 
            btn_censar.BackColor = Color.SeaGreen;
            btn_censar.Dock = DockStyle.Fill;
            btn_censar.Font = new Font("Times New Roman", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_censar.ForeColor = Color.White;
            btn_censar.Location = new Point(3, 3);
            btn_censar.Name = "btn_censar";
            btn_censar.Size = new Size(254, 10);
            btn_censar.TabIndex = 5;
            btn_censar.Text = "Censar";
            btn_censar.UseVisualStyleBackColor = false;
            btn_censar.Click += btn_censar_click;
            // 
            // btn_marcar_retirado
            // 
            btn_marcar_retirado.BackColor = Color.SeaGreen;
            btn_marcar_retirado.Dock = DockStyle.Fill;
            btn_marcar_retirado.Font = new Font("Times New Roman", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_marcar_retirado.ForeColor = Color.White;
            btn_marcar_retirado.Location = new Point(531, 3);
            btn_marcar_retirado.Name = "btn_marcar_retirado";
            btn_marcar_retirado.Size = new Size(256, 10);
            btn_marcar_retirado.TabIndex = 4;
            btn_marcar_retirado.Text = "Marcar como retirados";
            btn_marcar_retirado.UseVisualStyleBackColor = false;
            btn_marcar_retirado.Click += btn_retiro_click;
            // 
            // btn_dejar_espera
            // 
            btn_dejar_espera.BackColor = Color.SeaGreen;
            btn_dejar_espera.Dock = DockStyle.Fill;
            btn_dejar_espera.Font = new Font("Times New Roman", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_dejar_espera.ForeColor = Color.White;
            btn_dejar_espera.Location = new Point(263, 3);
            btn_dejar_espera.Name = "btn_dejar_espera";
            btn_dejar_espera.Size = new Size(262, 10);
            btn_dejar_espera.TabIndex = 3;
            btn_dejar_espera.Text = "Dejar en espera";
            btn_dejar_espera.UseVisualStyleBackColor = false;
            btn_dejar_espera.Click += btn_espera_click;
            // 
            // visor_personas
            // 
            visor_personas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            visor_personas.Dock = DockStyle.Fill;
            visor_personas.Location = new Point(5, 119);
            visor_personas.Name = "visor_personas";
            visor_personas.Size = new Size(790, 302);
            visor_personas.TabIndex = 3;
            // 
            // FormListaCensados
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Layot_principal);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormListaCensados";
            Text = "Listado de comuneros censados";
            WindowState = FormWindowState.Maximized;
            FormClosed += FormListaCensados_Closed;
            Load += FormListaCensados_Load;
            Layot_principal.ResumeLayout(false);
            lyt_cabecera.ResumeLayout(false);
            lyt_cabecera.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            lyt_buscar_comunidades.ResumeLayout(false);
            lyt_buscar_comunidades.PerformLayout();
            lyt_botones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)visor_personas).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel Layot_principal;
        private TableLayoutPanel lyt_cabecera;
        private Button btn_salir;
        private Button btn_cerrar_sesion;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel3;
        private Label lblvigencia;
        private Label lblusuario;
        private Label lblrol;
        private Button btn_ver_censados;
        private TableLayoutPanel lyt_buscar_comunidades;
        private Label label2;
        private ComboBox cbb_comunidad;
        private Button btn_buscar;
        private TableLayoutPanel lyt_botones;
        private Button btn_marcar_retirado;
        private Button btn_dejar_espera;
        private DataGridView visor_personas;
        private Button btn_censar;
    }
}
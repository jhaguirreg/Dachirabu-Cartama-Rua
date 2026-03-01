namespace Autocenso_Cabildo
{
    partial class FormLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            Salir = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            pictureBox1 = new PictureBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label6 = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            label8 = new Label();
            Contraseña = new TextBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            label7 = new Label();
            Usuario = new TextBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            label5 = new Label();
            Rol = new ComboBox();
            inicio_sesion = new Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // Salir
            // 
            Salir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Salir.AutoSize = true;
            Salir.BackColor = Color.Red;
            Salir.Cursor = Cursors.Hand;
            Salir.FlatStyle = FlatStyle.Popup;
            Salir.ForeColor = SystemColors.ActiveCaptionText;
            Salir.ImageAlign = ContentAlignment.TopCenter;
            Salir.Location = new Point(866, 12);
            Salir.Name = "Salir";
            Salir.Size = new Size(68, 30);
            Salir.TabIndex = 0;
            Salir.Text = "Salir";
            Salir.UseVisualStyleBackColor = false;
            Salir.Click += button1_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.5714283F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.42857F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
            tableLayoutPanel1.Location = new Point(12, 48);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 51F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 49F));
            tableLayoutPanel1.Size = new Size(922, 163);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(5, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(255, 153);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Controls.Add(label2, 0, 1);
            tableLayoutPanel2.Controls.Add(label3, 0, 2);
            tableLayoutPanel2.Controls.Add(label4, 0, 3);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(268, 5);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 66.31579F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 33.68421F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel2.Size = new Size(649, 153);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.AliceBlue;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(643, 61);
            label1.TabIndex = 0;
            label1.Text = "COMUNIDAD INDÍGENA CARTAMA";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Times New Roman", 14.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label2.Location = new Point(3, 61);
            label2.Name = "label2";
            label2.Size = new Size(643, 30);
            label2.TabIndex = 1;
            label2.Text = "Resolución 0046 del 03 de Mayo del 2012";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Times New Roman", 14.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label3.Location = new Point(3, 91);
            label3.Name = "label3";
            label3.Size = new Size(643, 31);
            label3.TabIndex = 2;
            label3.Text = "Ministerio del Interior";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("Times New Roman", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(3, 122);
            label4.Name = "label4";
            label4.Size = new Size(643, 31);
            label4.TabIndex = 3;
            label4.Text = "Autocenso Cartama";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.AliceBlue;
            label6.Location = new Point(3, 0);
            label6.Name = "label6";
            label6.Size = new Size(940, 58);
            label6.TabIndex = 3;
            label6.Text = "Bienvenido";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            label6.Click += label6_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(tableLayoutPanel6, 0, 3);
            tableLayoutPanel3.Controls.Add(tableLayoutPanel5, 0, 2);
            tableLayoutPanel3.Controls.Add(label6, 0, 0);
            tableLayoutPanel3.Controls.Add(tableLayoutPanel4, 0, 1);
            tableLayoutPanel3.Controls.Add(inicio_sesion, 0, 4);
            tableLayoutPanel3.Location = new Point(0, 214);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RightToLeft = RightToLeft.No;
            tableLayoutPanel3.RowCount = 5;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(946, 292);
            tableLayoutPanel3.TabIndex = 4;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Controls.Add(label8, 0, 0);
            tableLayoutPanel6.Controls.Add(Contraseña, 1, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(3, 177);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 1;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Size = new Size(940, 52);
            tableLayoutPanel6.TabIndex = 6;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.ForeColor = Color.AliceBlue;
            label8.Location = new Point(3, 0);
            label8.Name = "label8";
            label8.Size = new Size(464, 52);
            label8.TabIndex = 5;
            label8.Text = "Constraseña: ";
            label8.TextAlign = ContentAlignment.MiddleRight;
            label8.Click += label8_Click;
            // 
            // Contraseña
            // 
            Contraseña.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            Contraseña.Location = new Point(473, 3);
            Contraseña.Name = "Contraseña";
            Contraseña.Size = new Size(264, 23);
            Contraseña.TabIndex = 6;
            Contraseña.UseSystemPasswordChar = true;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Controls.Add(label7, 0, 0);
            tableLayoutPanel5.Controls.Add(Usuario, 1, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 119);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Size = new Size(940, 52);
            tableLayoutPanel5.TabIndex = 5;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Fill;
            label7.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.AliceBlue;
            label7.Location = new Point(3, 0);
            label7.Name = "label7";
            label7.Size = new Size(464, 52);
            label7.TabIndex = 5;
            label7.Text = "Usuario:         ";
            label7.TextAlign = ContentAlignment.MiddleRight;
            label7.Click += label7_Click;
            // 
            // Usuario
            // 
            Usuario.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            Usuario.Location = new Point(473, 3);
            Usuario.Name = "Usuario";
            Usuario.Size = new Size(264, 23);
            Usuario.TabIndex = 6;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
            tableLayoutPanel4.Controls.Add(label5, 0, 0);
            tableLayoutPanel4.Controls.Add(Rol, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 61);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Size = new Size(940, 52);
            tableLayoutPanel4.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.AliceBlue;
            label5.Location = new Point(3, 0);
            label5.Name = "label5";
            label5.Size = new Size(464, 52);
            label5.TabIndex = 4;
            label5.Text = "Rol:                ";
            label5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Rol
            // 
            Rol.AutoCompleteCustomSource.AddRange(new string[] { "Censista" });
            Rol.DropDownStyle = ComboBoxStyle.DropDownList;
            Rol.DropDownWidth = 200;
            Rol.FormattingEnabled = true;
            Rol.Location = new Point(473, 3);
            Rol.Name = "Rol";
            Rol.Size = new Size(264, 23);
            Rol.TabIndex = 5;
            // 
            // inicio_sesion
            // 
            inicio_sesion.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            inicio_sesion.AutoSize = true;
            inicio_sesion.BackColor = Color.SeaGreen;
            inicio_sesion.BackgroundImageLayout = ImageLayout.None;
            inicio_sesion.Cursor = Cursors.Hand;
            inicio_sesion.FlatStyle = FlatStyle.Popup;
            inicio_sesion.Font = new Font("Times New Roman", 26.25F, FontStyle.Bold);
            inicio_sesion.ForeColor = Color.White;
            inicio_sesion.Location = new Point(3, 235);
            inicio_sesion.Name = "inicio_sesion";
            inicio_sesion.Size = new Size(940, 54);
            inicio_sesion.TabIndex = 7;
            inicio_sesion.Text = "Comenzar";
            inicio_sesion.TextAlign = ContentAlignment.TopCenter;
            inicio_sesion.UseVisualStyleBackColor = false;
            inicio_sesion.Click += inicio_sesion_Click;
            // 
            // FormLogin
            // 
            AcceptButton = inicio_sesion;
            AccessibleName = "Dachirabu Kartama Rua";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.SeaGreen;
            CancelButton = Salir;
            ClientSize = new Size(946, 506);
            Controls.Add(tableLayoutPanel3);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(Salir);
            ForeColor = SystemColors.ActiveCaptionText;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormLogin";
            Text = "Dachirabu Kartama Rua";
            WindowState = FormWindowState.Maximized;
            FormClosed += FormLogin_FormClosed;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Salir;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox pictureBox1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label6;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel6;
        private Label label8;
        private TableLayoutPanel tableLayoutPanel5;
        private Label label7;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label5;
        private Button inicio_sesion;
        private ComboBox Rol;
        private TextBox Contraseña;
        private TextBox Usuario;
    }
}
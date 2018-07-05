namespace Igra
{
    partial class SpremeniEnoto
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
            this.comboBoxRasa = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label = new System.Windows.Forms.Label();
            this.buttonSpremeni = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownRazdalja = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownOklep = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownZivljenje = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownNapad = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPremik = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownCena = new System.Windows.Forms.NumericUpDown();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.comboBoxIme = new System.Windows.Forms.ComboBox();
            this.numericUpDownTip = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRazdalja)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOklep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZivljenje)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNapad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPremik)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCena)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTip)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxRasa
            // 
            this.comboBoxRasa.FormattingEnabled = true;
            this.comboBoxRasa.Items.AddRange(new object[] {
            "Vsi",
            "Bela",
            "Crna",
            "Rdeca",
            "Zelena",
            "Nevtralni"});
            this.comboBoxRasa.Location = new System.Drawing.Point(133, 9);
            this.comboBoxRasa.Name = "comboBoxRasa";
            this.comboBoxRasa.Size = new System.Drawing.Size(121, 21);
            this.comboBoxRasa.TabIndex = 41;
            this.comboBoxRasa.SelectedIndexChanged += new System.EventHandler(this.comboBoxRasa_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(95, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 40;
            this.label9.Text = "Rasa";
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(101, 40);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(24, 13);
            this.label.TabIndex = 39;
            this.label.Text = "Ime";
            // 
            // buttonSpremeni
            // 
            this.buttonSpremeni.Enabled = false;
            this.buttonSpremeni.Location = new System.Drawing.Point(177, 274);
            this.buttonSpremeni.Name = "buttonSpremeni";
            this.buttonSpremeni.Size = new System.Drawing.Size(75, 23);
            this.buttonSpremeni.TabIndex = 32;
            this.buttonSpremeni.Text = "Spremeni";
            this.buttonSpremeni.UseVisualStyleBackColor = true;
            this.buttonSpremeni.Click += new System.EventHandler(this.buttonSpremeni_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(39, 221);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Razdalja napada";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(91, 195);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 37;
            this.label6.Text = "Oklep";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(80, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 36;
            this.label5.Text = "Zivljenje";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(87, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 35;
            this.label4.Text = "Napad";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(87, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 34;
            this.label3.Text = "Premik";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(98, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "Text";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "Cena";
            // 
            // numericUpDownRazdalja
            // 
            this.numericUpDownRazdalja.Location = new System.Drawing.Point(133, 219);
            this.numericUpDownRazdalja.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownRazdalja.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRazdalja.Name = "numericUpDownRazdalja";
            this.numericUpDownRazdalja.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownRazdalja.TabIndex = 30;
            this.numericUpDownRazdalja.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownOklep
            // 
            this.numericUpDownOklep.Location = new System.Drawing.Point(132, 193);
            this.numericUpDownOklep.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownOklep.Name = "numericUpDownOklep";
            this.numericUpDownOklep.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownOklep.TabIndex = 29;
            // 
            // numericUpDownZivljenje
            // 
            this.numericUpDownZivljenje.Location = new System.Drawing.Point(132, 167);
            this.numericUpDownZivljenje.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownZivljenje.Name = "numericUpDownZivljenje";
            this.numericUpDownZivljenje.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownZivljenje.TabIndex = 28;
            this.numericUpDownZivljenje.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownNapad
            // 
            this.numericUpDownNapad.Location = new System.Drawing.Point(132, 141);
            this.numericUpDownNapad.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownNapad.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNapad.Name = "numericUpDownNapad";
            this.numericUpDownNapad.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownNapad.TabIndex = 27;
            this.numericUpDownNapad.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownPremik
            // 
            this.numericUpDownPremik.Location = new System.Drawing.Point(132, 115);
            this.numericUpDownPremik.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownPremik.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPremik.Name = "numericUpDownPremik";
            this.numericUpDownPremik.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownPremik.TabIndex = 26;
            this.numericUpDownPremik.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownCena
            // 
            this.numericUpDownCena.Location = new System.Drawing.Point(132, 63);
            this.numericUpDownCena.Name = "numericUpDownCena";
            this.numericUpDownCena.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownCena.TabIndex = 24;
            // 
            // textBoxText
            // 
            this.textBoxText.Location = new System.Drawing.Point(132, 89);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(120, 20);
            this.textBoxText.TabIndex = 25;
            // 
            // comboBoxIme
            // 
            this.comboBoxIme.Enabled = false;
            this.comboBoxIme.FormattingEnabled = true;
            this.comboBoxIme.Location = new System.Drawing.Point(133, 37);
            this.comboBoxIme.Name = "comboBoxIme";
            this.comboBoxIme.Size = new System.Drawing.Size(121, 21);
            this.comboBoxIme.TabIndex = 42;
            this.comboBoxIme.SelectedIndexChanged += new System.EventHandler(this.comboBoxIme_SelectedIndexChanged);
            // 
            // numericUpDownTip
            // 
            this.numericUpDownTip.Location = new System.Drawing.Point(132, 245);
            this.numericUpDownTip.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownTip.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTip.Name = "numericUpDownTip";
            this.numericUpDownTip.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownTip.TabIndex = 43;
            this.numericUpDownTip.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(103, 247);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 44;
            this.label8.Text = "Tip";
            // 
            // SpremeniEnoto
            // 
            this.ClientSize = new System.Drawing.Size(264, 307);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericUpDownTip);
            this.Controls.Add(this.comboBoxIme);
            this.Controls.Add(this.comboBoxRasa);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label);
            this.Controls.Add(this.buttonSpremeni);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownRazdalja);
            this.Controls.Add(this.numericUpDownOklep);
            this.Controls.Add(this.numericUpDownZivljenje);
            this.Controls.Add(this.numericUpDownNapad);
            this.Controls.Add(this.numericUpDownPremik);
            this.Controls.Add(this.numericUpDownCena);
            this.Controls.Add(this.textBoxText);
            this.Name = "SpremeniEnoto";
            this.Text = "Spremeni enoto";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRazdalja)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOklep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZivljenje)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNapad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPremik)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCena)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTip)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxRasa;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Button buttonSpremeni;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownRazdalja;
        private System.Windows.Forms.NumericUpDown numericUpDownOklep;
        private System.Windows.Forms.NumericUpDown numericUpDownZivljenje;
        private System.Windows.Forms.NumericUpDown numericUpDownNapad;
        private System.Windows.Forms.NumericUpDown numericUpDownPremik;
        private System.Windows.Forms.NumericUpDown numericUpDownCena;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.ComboBox comboBoxIme;
        private System.Windows.Forms.NumericUpDown numericUpDownTip;
        private System.Windows.Forms.Label label8;
    }
}

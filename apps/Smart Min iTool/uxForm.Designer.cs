namespace Smart_Mini_Tool
{
    partial class uxForm
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
            this.components = new System.ComponentModel.Container();
            this.uxEnable = new System.Windows.Forms.Button();
            this.uxPercent = new System.Windows.Forms.TextBox();
            this.uxSpell = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.uxExhaustion = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.uxMana = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // uxEnable
            // 
            this.uxEnable.Location = new System.Drawing.Point(177, 116);
            this.uxEnable.Name = "uxEnable";
            this.uxEnable.Size = new System.Drawing.Size(174, 41);
            this.uxEnable.TabIndex = 0;
            this.uxEnable.Text = "Enable";
            this.uxEnable.UseVisualStyleBackColor = true;
            this.uxEnable.Click += new System.EventHandler(this.uxEnable_Click);
            // 
            // uxPercent
            // 
            this.uxPercent.Location = new System.Drawing.Point(251, 12);
            this.uxPercent.Name = "uxPercent";
            this.uxPercent.Size = new System.Drawing.Size(100, 20);
            this.uxPercent.TabIndex = 1;
            this.uxPercent.Text = "90";
            // 
            // uxSpell
            // 
            this.uxSpell.Location = new System.Drawing.Point(251, 38);
            this.uxSpell.Name = "uxSpell";
            this.uxSpell.Size = new System.Drawing.Size(100, 20);
            this.uxSpell.TabIndex = 2;
            this.uxSpell.Text = "Exura";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(177, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Heal at %:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "With spell:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(179, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Exhaustion:";
            // 
            // uxExhaustion
            // 
            this.uxExhaustion.Location = new System.Drawing.Point(253, 64);
            this.uxExhaustion.Name = "uxExhaustion";
            this.uxExhaustion.Size = new System.Drawing.Size(100, 20);
            this.uxExhaustion.TabIndex = 5;
            this.uxExhaustion.Text = "1080";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(179, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Mana needed:";
            // 
            // uxMana
            // 
            this.uxMana.Location = new System.Drawing.Point(253, 90);
            this.uxMana.Name = "uxMana";
            this.uxMana.Size = new System.Drawing.Size(100, 20);
            this.uxMana.TabIndex = 7;
            this.uxMana.Text = "20";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Ativar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(169, 18);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 10;
            this.textBox1.Text = "Demon";
            // 
            // timer1
            // 
            this.timer1.Interval = 2;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // uxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(96, 39);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.uxMana);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.uxExhaustion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.uxSpell);
            this.Controls.Add(this.uxPercent);
            this.Controls.Add(this.uxEnable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "uxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Mini Tool";
            this.Load += new System.EventHandler(this.uxForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button uxEnable;
        private System.Windows.Forms.TextBox uxPercent;
        private System.Windows.Forms.TextBox uxSpell;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox uxExhaustion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox uxMana;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
    }
}


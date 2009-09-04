using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Mini_Tool {
    public partial class uxForm : Form {
        Kedrah.Util.Database db = new Kedrah.Util.Database();

        public uxForm() {
            InitializeComponent();
        }

        private void uxEnable_Click(object sender, EventArgs e) {
            uxEnable.Enabled = false;

            timer1.Start();
            db.CreaturesToFile();
        }

        private void uxForm_Load(object sender, EventArgs e) {

        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (db.pdone == -1)
                uxEnable.Text = "Starting download";
            else if (db.pdone == 500)
                uxEnable.Text = "Completed!";
            else
                uxEnable.Text = (db.pdone * 100 / 500).ToString() + "%";
            progressBar1.Value = (db.pdone * 100 / 500);
        }
    }
}

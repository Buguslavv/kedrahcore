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
        Kedrah.Core Kedrah;

        public uxForm() {
            Kedrah = new Kedrah.Core();
            if (Kedrah.Client == null)
                Environment.Exit(0);
            InitializeComponent();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Mini_Tool
{
    public partial class uxForm : Form
    {
        Kedrah.Core kedrah; // objeto principal da dll

        public uxForm()
        {//inicia as parada
            kedrah = new Kedrah.Core();
            if (kedrah.Client == null)
                Environment.Exit(0);
            InitializeComponent();
        }

        private void uxEnable_Click(object sender, EventArgs e)
        {
            uint percent, exhaustion, mana;//healer mais simples do mundo, acho que ta bugado isso, mas enfim
            if (UInt32.TryParse(uxPercent.Text, out percent) && percent <= 100 && UInt32.TryParse(uxExhaustion.Text, out exhaustion) && UInt32.TryParse(uxMana.Text, out mana))
            {
                uxPercent.Enabled = !uxPercent.Enabled;
                uxSpell.Enabled = !uxSpell.Enabled;
                if (!uxSpell.Enabled)
                {
                    kedrah.Modules.Heal.SpellLife.Add(new Kedrah.Modules.SpellPercent(percent, uxSpell.Text, mana));
                    kedrah.Modules.Heal.SpellExhaustion = exhaustion;
                    kedrah.Modules.Heal.Healer = true;
                    uxEnable.Text = "Disable";
                }
                else
                {
                    kedrah.Modules.Heal.SpellLife.Clear();
                    kedrah.Modules.Heal.Healer = false;
                    uxEnable.Text = "Enable";
                }
            }
            else
                MessageBox.Show("Mana needed/Percent/Exhaustion must be numeric! Percent must be between 0 and 100!");
        }
    }
}

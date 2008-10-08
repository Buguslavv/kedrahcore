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
        Kedrah.Core kedrah;

        public uxForm()
        {
            kedrah = new Kedrah.Core();
            if (kedrah.Client == null)
                Environment.Exit(0);
            kedrah.Play();
            InitializeComponent();
        }

        private void uxEnable_Click(object sender, EventArgs e)
        {
            uint percent, exhaustion, mana;
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

        private void uxForm_Load(object sender, EventArgs e)
        {
            kedrah.Modules.General.EnableLevelSpyKeys();
        }
    }
}

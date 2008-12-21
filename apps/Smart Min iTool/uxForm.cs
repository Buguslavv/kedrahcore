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

        int ammo;
        int ammo2;

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
            Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.F8, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (kedrah.Client.IsActive && kedrah.Client.LoggedIn)
                {
                    try
                    {
                        kedrah.Console.Spell(kedrah.Modules.Targeting.GetBestMageSpell(kedrah.Modules.Targeting.FindMonster(kedrah.BattleList.GetCreature(kedrah.Player.Target_ID).Name)));
                    }
                    catch { }
                }
                return true;
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ammo = kedrah.Inventory.GetSlot(Tibia.Constants.SlotNumber.Ammo).Count;
            button1.Text = "Ativado, não feche!";
            kedrah.Modules.General.WalkOverFields = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ammo2 = kedrah.Inventory.GetSlot(Tibia.Constants.SlotNumber.Ammo).Count;
            if (ammo > ammo2)
                kedrah.Console.Spell("Exori San");
            ammo = kedrah.Inventory.GetSlot(Tibia.Constants.SlotNumber.Ammo).Count;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Igra
{
    public partial class Nastavitve : Form
    {
        int stIzbranih = 2; //koliko igralcev je izbranih; na začetku sta 2
        //bool igralec4; //if true, 4 igralci, else 2
        bool[] playerData = new bool[8]; //2 * št igralcev

        public Nastavitve()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
                playerData[i] = false; //default so vsi false, potem se pa tisti ki so true prestavijo

            if (checkBox1.Checked)
            {
                playerData[0] = true;
                if (checkBoxRac1.Checked)
                    playerData[1] = true;
            }

            if (checkBox2.Checked)
            {
                playerData[2] = true;
                if (checkBoxRac2.Checked)
                    playerData[3] = true;
            }
            if (checkBox3.Checked)
            {
                playerData[4] = true;
                if (checkBoxRac3.Checked)
                    playerData[5] = true;
            }
            if (checkBox4.Checked)
            {
                playerData[6] = true;
                if (checkBoxRac4.Checked)
                    playerData[7] = true;
            }
            this.Close();
        }

        public bool stIgralcev() //za zunanjo uporabo
        {
            return radioButton4.Checked; //če je checked so 4, če ni sta 2
        }

        public bool[] data()
        {
            return playerData;
        }

        private void radioButtonStIgralcev_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) //če so 4 igralci, bojo vsi 4 aktivni
            {
                checkBox1.Checked = checkBox2.Checked = checkBox3.Checked = checkBox4.Checked = true;
                checkBox1.Enabled = checkBox2.Enabled = checkBox3.Enabled = checkBox4.Enabled = false;
                stIzbranih = 4;
            }
            else //če sta samo 2...
            {
                checkBox1.Checked =  checkBox4.Checked = true;
                checkBox2.Checked = checkBox3.Checked = false;
                checkBox1.Enabled = checkBox2.Enabled = checkBox3.Enabled = checkBox4.Enabled = true;
                stIzbranih = 2;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) //če so 4 ostalo ni pomembno
            {
                checkBoxRac1.Visible = true;
                return;
            }
            else if (checkBox1.Checked)
            {
                stIzbranih++;
                if (stIzbranih > 2)
                {
                    checkBox1.Checked = false;
                    MessageBox.Show("Ne moreš izbrati več igralcev!");
                    return;
                }
                checkBoxRac1.Visible = true;
            }
            else
            {
                stIzbranih--;
                checkBoxRac1.Visible = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) //če so 4 ostalo ni pomembno
            {
                checkBoxRac2.Visible = true;
                return;
            }
            else if (checkBox2.Checked)
            {
                stIzbranih++;
                if (stIzbranih > 2)
                {
                    checkBox2.Checked = false;
                    MessageBox.Show("Ne moreš izbrati več igralcev!");
                    return;
                }
                checkBoxRac2.Visible = true;
            }
            else
            {
                stIzbranih--;
                checkBoxRac2.Visible = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) //če so 4 ostalo ni pomembno
            {
                checkBoxRac3.Visible = true;
                return;
            }
            else if (checkBox3.Checked)
            {
                stIzbranih++;
                if (stIzbranih > 2)
                {
                    checkBox3.Checked = false;
                    MessageBox.Show("Ne moreš izbrati več igralcev!");
                    return;
                }
                checkBoxRac3.Visible = true;
            }
            else
            {
                stIzbranih--;
                checkBoxRac3.Visible = false;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) //če so 4 ostalo ni pomembno
            {
                checkBoxRac4.Visible = true;
                return;
            }
            else if (checkBox4.Checked)
            {
                stIzbranih++;
                if (stIzbranih > 2)
                {
                    checkBox4.Checked = false;
                    MessageBox.Show("Ne moreš izbrati več igralcev!");
                    return;
                }
                checkBoxRac4.Visible = true;
            }
            else
            {
                stIzbranih--;
                checkBoxRac4.Visible = false;
            }
        }
    }
}

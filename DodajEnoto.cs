using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Igra
{
    public partial class DodajEnoto : Form
    {
        public DodajEnoto()
        {
            InitializeComponent();
            comboBoxRasa.SelectedIndex = 0;
        }

        int stEnote(string datoteka) //vrne koliko enot je trenutno shranjenih iz česar lahko določiš naslednjo številko
        {
            const int stAt = 8;
            int count = 0;

            XmlReader reader = new XmlTextReader(datoteka);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        count++;
                        break;

                    default:
                        break;
                }
            }

            reader.Close();
            return count / (stAt+1) + 1; //vrne številko nove enote
        }

        private void buttonDodaj_Click(object sender, EventArgs e)
        {
            XmlDocument doc1 = new XmlDocument();
            /*XmlDocument doc2 = new XmlDocument();
            XmlDocument doc3 = new XmlDocument();
            XmlDocument doc4 = new XmlDocument();*/
            int stIteracij; //koliko datotek obdelujem
            string barva = ""; //za slike
            try
            {
                if (comboBoxRasa.Text != "Vsi") //če gre za specifično enoto, ki ni enaka/podobna med večimi rasami
                {
                    doc1.Load(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml");
                    stIteracij = 1;
                }
                else //če hočem dat notri vse, potem moram imet odprte vse 4 xml datoteke (ali pa jih odpirat in zapirat)
                {
                    stIteracij = 4;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
            for (int i = 0; i < stIteracij; i++) //izvrši 1× ali 4×, odvisno od tega a shranjujem specifično, ali za vse enako
            {
                XmlElement elem;
                if (stIteracij == 4)
                {
                    if (i == 1)
                    {
                        doc1.Load("bela.xml");
                        barva = "bela";
                    }
                    else if (i == 2)
                    {
                        doc1.Load("crna.xml");
                        barva = "crna";
                    }
                    else if (i == 3)
                    {
                        doc1.Load("rdeca.xml");
                        barva = "rdeca";
                    }
                    else
                    {
                        doc1.Load("zelena.xml");
                        barva = "zelena";
                    }
                }
                else
                    barva = comboBoxRasa.SelectedItem.ToString().ToLower();

                try
                {
                    elem = doc1.CreateElement(textBoxIme.Text.Replace(' ', '_'));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                XmlElement atribut = doc1.CreateElement(label1.Text.ToLower());
                atribut.InnerText = numericUpDownCena.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label2.Text.ToLower());
                atribut.InnerText = textBoxText.Text;
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label3.Text.ToLower());
                atribut.InnerText = numericUpDownPremik.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label4.Text.ToLower());
                atribut.InnerText = numericUpDownNapad.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label5.Text.ToLower());
                atribut.InnerText = numericUpDownZivljenje.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label6.Text.ToLower());
                atribut.InnerText = numericUpDownOklep.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement(label7.Text.ToLower().Replace(' ', '_'));
                atribut.InnerText = numericUpDownRazdalja.Value.ToString();
                elem.AppendChild(atribut);

                atribut = doc1.CreateElement("slika");
                atribut.InnerText = barva + stEnote(barva + ".xml") + ".png";
                elem.AppendChild(atribut);

                doc1.DocumentElement.AppendChild(elem); //doda enoto z vsemi lastnostmi v xml

                if (comboBoxRasa.Text == "Vsi")
                {
                    if(i == 1)
                        doc1.Save("bela.xml");
                    else if(i == 2)
                        doc1.Save("crna.xml");
                    else if (i == 3)
                        doc1.Save("rdeca.xml");
                    else
                        doc1.Save("zelena.xml");
                }
                else
                    doc1.Save(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml");
            }

            string izpis = "Enota uspešno shranjena. Za njeno uporabo bo najverjetneje potrebno igro ponovno zagnati ter v mapo 'Slike' dodati "; //reciklaža

            if (comboBoxRasa.Text == "Vsi")
                izpis += "slike:\n\rbela" + (stEnote("bela.xml")-1) + ".png\n\rcrna" + (stEnote("crna.xml")-1) + ".png\n\rrdeca" + (stEnote("rdeca.xml")-1) + ".png\n\rzelena" + (stEnote("zelena.xml")-1) + ".png";
            else
                izpis += "sliko " + comboBoxRasa.SelectedItem.ToString().ToLower() + (stEnote(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml")-1) + ".png";

            MessageBox.Show(izpis);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace Igra
{
    public partial class SpremeniEnoto : Form
    {
        int zacetni;
        List<string> atributi = new List<string>();
        List<string> podatki = new List<string>();
        List<string> enote = new List<string>();

        public SpremeniEnoto()
        {
            Debug.WriteLine("line");
            InitializeComponent();
        }

        private void comboBoxRasa_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxIme.Enabled = true; //po izbiri rase se ve katero datoteko naložit   

            if (comboBoxRasa.Text == "Vsi")
                naloziPodatke("bela.xml", atributi, podatki, enote);
            else
                naloziPodatke(comboBoxRasa.Text.ToLower() + ".xml", atributi, podatki, enote);
        }

        private void comboBoxIme_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i;
            for (i = 0; i < enote.Count; i++)
            {
                if (enote[i].Replace("_", " ") == comboBoxIme.Text)
                {
                    //MessageBox.Show(enote[i].Replace("_", " ") + " " + comboBoxIme.Text);
                    break;
                }
            }

            zacetni = i * (Gl.stAt);

            numericUpDownCena.Value = Int32.Parse(podatki[zacetni]);
            textBoxText.Text = podatki[zacetni + 1];
            numericUpDownPremik.Value = Int32.Parse(podatki[zacetni + 2]);
            numericUpDownNapad.Value = Int32.Parse(podatki[zacetni + 3]);
            numericUpDownZivljenje.Value = Int32.Parse(podatki[zacetni + 4]);
            numericUpDownOklep.Value = Int32.Parse(podatki[zacetni + 5]);
            numericUpDownRazdalja.Value = Int32.Parse(podatki[zacetni + 6]);
            numericUpDownTip.Value = Int32.Parse(podatki[zacetni + 7]);

            buttonSpremeni.Enabled = true;
        }

        void naloziPodatke(string datoteka, List<string> atributi, List<string> podatki, List<string> enote)
        {
            XmlReader reader = new XmlTextReader(datoteka);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        atributi.Add(reader.Name);
                        break;

                    case XmlNodeType.Text:
                        podatki.Add(reader.Value);
                        break;
                }
            }
            
            atributi.RemoveAt(0);
            for (int i = 0; i < atributi.Count; i++)
            {
                if (i % Gl.stAt == 0)
                {
                    enote.Add(atributi[i].Replace('_', ' ')); //nadomesti podčrtaje z presledki pred shranjevanjem za lepši pregled
                    atributi.RemoveAt(i);
                }
            }
            for (int i = 0; i < enote.Count; i++)
            {
                comboBoxIme.Items.Insert(i, enote[i]);
            }
            //MessageBox.Show(enote[0].ToString());
            reader.Close();
        }

        void shraniPodatke()
        {
            XmlDocument doc1 = new XmlDocument();

            int stIteracij; //koliko datotek obdelujem
            string barva = ""; //za slike
            try
            {
                if (comboBoxRasa.Text != "Vsi") //če gre za specifično enoto, ki ni enaka/podobna med večimi rasami
                {
                    string color = comboBoxRasa.SelectedItem.ToString().ToLower();
                    File.WriteAllText(@"" + color + ".xml", "<" + color + ">\n</" + color + ">");

                    doc1.Load(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml");
                    stIteracij = 1;
                }
                else //če hočem dat notri vse, potem moram imet odprte vse 4 xml datoteke (ali pa jih odpirat in zapirat)
                {
                    stIteracij = 4;
                    File.WriteAllText(@"bela.xml", "<bela>\n</bela>");
                    File.WriteAllText(@"crna.xml", "<crna>\n</crna>");
                    File.WriteAllText(@"rdeca.xml", "<rdeca>\n</rdeca>");
                    File.WriteAllText(@"zelena.xml", "<zelena>\n</zelena>");
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


                int count = 1;
                int st = 0;
                foreach (string enota in enote)
                {
                    try
                    {
                        elem = doc1.CreateElement(enota.Replace(' ', '_'));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    /* atribut = doc1.CreateElement("cena");
                     * atribut.InnerText = 5;
                     * elem.AppendChild(atribut);*/

                    XmlElement atribut = doc1.CreateElement(atributi[st]);
                    atribut.InnerText = podatki[st];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 1]);
                    atribut.InnerText = podatki[st + 1];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 2]);
                    atribut.InnerText = podatki[st + 2];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 3]);
                    atribut.InnerText = podatki[st + 3];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 4]);
                    atribut.InnerText = podatki[st + 4];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 5]);
                    atribut.InnerText = podatki[st + 5];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 6].Replace(' ', '_'));
                    atribut.InnerText = podatki[st + 6];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement(atributi[st + 7]);
                    atribut.InnerText = podatki[st + 7];
                    elem.AppendChild(atribut);

                    atribut = doc1.CreateElement("slika");
                    atribut.InnerText = barva + count.ToString() + ".png";
                    elem.AppendChild(atribut);

                    doc1.DocumentElement.AppendChild(elem); //doda enoto z vsemi lastnostmi v xml

                    st += Gl.stAt; //naslednja runda

                    count++;
                }
                

                if (comboBoxRasa.Text == "Vsi")
                {
                    if (i == 1)
                        doc1.Save("bela.xml");
                    else if (i == 2)
                        doc1.Save("crna.xml");
                    else if (i == 3)
                        doc1.Save("rdeca.xml");
                    else
                        doc1.Save("zelena.xml");
                }
                else
                    doc1.Save(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml");
            }

            string izpis = "Enota uspešno spremenjena!";

            /*if (comboBoxRasa.Text == "Vsi")
                izpis += "slike:\n\rbela" + (stEnote("bela.xml") - 1) + ".png\n\rcrna" + (stEnote("crna.xml") - 1) + ".png\n\rrdeca" + (stEnote("rdeca.xml") - 1) + ".png\n\rzelena" + (stEnote("zelena.xml") - 1) + ".png";
            else
                izpis += "sliko " + comboBoxRasa.SelectedItem.ToString().ToLower() + (stEnote(comboBoxRasa.SelectedItem.ToString().ToLower() + ".xml") - 1) + ".png";
            */
            MessageBox.Show(izpis);
        }

        private void buttonSpremeni_Click(object sender, EventArgs e)
        {
            enote[zacetni / Gl.stAt] = comboBoxIme.Text; //ime
            
            podatki[zacetni] = numericUpDownCena.Value.ToString();
            podatki[zacetni + 1] = textBoxText.Text;
            podatki[zacetni + 2] = numericUpDownPremik.Value.ToString();
            podatki[zacetni + 3] = numericUpDownNapad.Value.ToString();
            podatki[zacetni + 4] = numericUpDownZivljenje.Value.ToString();
            podatki[zacetni + 5] = numericUpDownOklep.Value.ToString();
            podatki[zacetni + 6] = numericUpDownRazdalja.Value.ToString();
            podatki[zacetni + 7] = numericUpDownTip.Value.ToString();

            shraniPodatke();
        }
    }
}

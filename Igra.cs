#define debug

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Igra
{
    public partial class Igra : Form
    {
        Point[] igralnoPolje = new Point[(Gl.N-1)*4]; //v igralnoObmocje so shranjeni skrajni indexi daljic, ki določajo igralno ploščo 

        const int sleep = 0; //kako dolgo spi
        int indexAktivne, naVrsti; //index igralca na vrsti (1-4)
        bool racunalnik, igralec4, kartaIzbrana; //false = 2 igralca ali true = 4
        Point aktivnaKarta, locNov, velikost; //velikost celice

        bool[] aktivni, AIcontroled; //stanja igralcev (aktiven/neaktiven); true, če je AI
        int[] stanja, dobljeneTocke; //stanja točk posameznih igralcev

        List<int> zivi; //kateri so še živi
        List<string> enote, atributi, podatki; //prekomplicirano, če bi vedno preverjal kateri je na vrsti, zato tu notri shranim tistega, ki je na vrsti...
        List<Karta> karte, premikiBack, napadiBack; //seznam kart na polju && seznam polj, na katera se lahko karta premakne

        public Igra()
        {
            InitializeComponent();
            //this.KeyPreview = true; //omogoči keyprewiev event, v katerem določiš kaj se v keydown zgodi (done on form)

            this.menuStrip1.BackColor = Color.Black;
            this.menuStrip1.ForeColor = Color.DeepPink;
            this.ForeColor = Color.DeepPink;

            igralec4 = false; //false = 2 igralca ali true = 4

            stanja = new int[4];
            dobljeneTocke = new int[4];
            aktivni = new bool[4];
            AIcontroled = new bool[4];
            karte = new List<Karta>();
            premikiBack = new List<Karta>();
            napadiBack = new List<Karta>();
            enote = new List<string>();
            atributi = new List<string>();
            podatki = new List<string>(); //prekomplicirano, če bi vedno preverjal kateri je na vrsti, zato tu notri shranim tistega, ki je na vrsti...
            zivi = new List<int>(); //kateri so še živi

            aktivni[0] = aktivni[3] = true;
            aktivni[1] = aktivni[2] = false; //začetno stanje, ki se lahko spremeni v nastavitvah

            AIcontroled[0] = AIcontroled[1] = AIcontroled[2] = false;
            AIcontroled[3] = true;

            this.BackgroundImageLayout = ImageLayout.Stretch;
            try
            {
                this.BackgroundImage = Properties.Resources.background;
            }
            catch (Exception)
            {
                MessageBox.Show("Slika za ozadje ni bila najdena.");
            }
            
            inicializacija();

            int j = 1;
            for (int i = 1; i < (Gl.N - 1) * 4; i += 4) //določitev igralnega polja
            {
                igralnoPolje[i - 1] = new Point(j * igralnoObmocje.Width / Gl.N, 0);
                igralnoPolje[i] = new Point(j * igralnoObmocje.Width / Gl.N, igralnoObmocje.Height);
                igralnoPolje[i + 1] = new Point(0, j * igralnoObmocje.Height / Gl.N);
                igralnoPolje[i + 2] = new Point(igralnoObmocje.Width, j * igralnoObmocje.Height / Gl.N);
                j++;
            }
            velikost = new Point(igralnoObmocje.Width / Gl.N, igralnoObmocje.Height / Gl.N); //velikost posamezne slike
            if (velikost.X == 0 || velikost.Y == 0) //program se je enkrat sesul zaradi tega ... no clue what happened ... tekel je v ozadju dalj časa in se je zgleda sesul, ko sem ga izbral
            {
                MessageBox.Show("Prišlo je do napake. Velikost posamezne enote: " + velikost.X + " " + velikost.Y);
                Application.Exit();
            }
        }



        /*
         * 
         * 
         * 
         * 
         * menu
         * 
         * 
         * 
         * 
         */



        private void izhodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dodajEnotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DodajEnoto nova = new DodajEnoto();
            nova.ShowDialog();
        }

        private void spremeniEnotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpremeniEnoto nova = new SpremeniEnoto();
            nova.ShowDialog();
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fullscreen();
        }

        private void lokalnoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Nastavitve nova = new Nastavitve();
            nova.ShowDialog();

            igralec4 = nova.stIgralcev(); //it works!

            for (int i = 0; i < 8; i += 2)
            {
                aktivni[i / 2] = nova.data()[i]; //teoretično, sam bo treba še dost nastavit verjetno ...
                AIcontroled[i / 2] = nova.data()[i + 1];
            }

            inicializacija();
        }



        /*
         * 
         * 
         * 
         * 
         * menu end
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * funkcije nad objekti
         * 
         * 
         * 
         * 
         */



        private void igralnoObmocje_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black);

            foreach (Karta karta in karte) //nariše vse karte na polje; koordinate so zgornji levi kot
            {
                if (karta.tip == 0)
                    e.Graphics.DrawImage(karta.slika, karta.lokacija.X, karta.lokacija.Y, velikost.X, velikost.Y);
                else
                    e.Graphics.DrawImage(karta.slika, karta.lokacija.X + (velikost.X / Gl.N / 2), karta.lokacija.Y + (velikost.Y / Gl.N / 2), velikost.X * (Gl.N - 1) / Gl.N, velikost.Y * (Gl.N - 1) / Gl.N);

                Font myFont = new Font("Dejavu Sans", 12); //eden redkih, ki podpira simbol za meče
                
                e.Graphics.DrawString("\u2764" + karta.zivljenje.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X + velikost.X - (karta.zivljenje.ToString().Length + 2) * 10, karta.lokacija.Y + velikost.Y - 18));
                e.Graphics.DrawString("\u2721" + karta.oklep.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X + velikost.X / 2 - (karta.zivljenje.ToString().Length + 2) * 5, karta.lokacija.Y + velikost.Y - 18));
                e.Graphics.DrawString("\u2694" + karta.napad.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X, karta.lokacija.Y + velikost.Y - 18));
                
            }

            foreach (Karta karta in premikiBack) //riše možne premike
                e.Graphics.DrawImage(Properties.Resources.premik, karta.lokacija.X, karta.lokacija.Y, velikost.X, velikost.Y);

            foreach (Karta karta in napadiBack) //riše možne napade
                e.Graphics.DrawImage(Properties.Resources.napad, karta.lokacija.X, karta.lokacija.Y, velikost.X, velikost.Y);

            for (int i = 0; i < igralnoPolje.Length; i += 2) //na koncu nariše polje, da enote ne prekrivajo črt (tako ali tako ne bi smele)
                e.Graphics.DrawLine(pen, igralnoPolje[i], igralnoPolje[i + 1]);
        }

        private void igralnoObmocje_MouseDown(object sender, MouseEventArgs e)
        {

            int x, y;
            int rasa = trenutnaRasa();

            x = e.X / velikost.X;
            y = e.Y / velikost.Y; //dobi številki polja v katerem se nahaja klik

            //MessageBox.Show(x.ToString() + ", " + y.ToString());

            //MessageBox.Show(locNov.X.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Global.N * 2).ToString() + "\n" + locNov.Y.ToString() + " == " + (igralnoObmocje.Height - igralnoObmocje.Height / Global.N * 2).ToString());

            x *= velikost.X;
            y *= velikost.Y; //dobi zgornji levi kot

            //če gledaš ploščo kot matriko od [0,0] do [7,7]
            locNov = new Point(x, y);
            //Karta izbrana = new Karta();

            
            if (aktivnaKarta.X == -1 && aktivnaKarta.Y == -1 && jeZasedena(locNov)) //izbere enoto, ki jo želi premakniti
            {
                kartaIzbrana = true;

                //MessageBox.Show(x.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Global.N).ToString());
                //for (int i = 0; i < karte.Count; i++) //gre skozi karte, dokler ne najde prave

                //if (jeZasedena(locNov))
                {
                    for (int i = 0; i < karte.Count; i++)
                    {
                        if (x == karte[i].lokacija.X && y == karte[i].lokacija.Y)
                        {
                            //izbrana = karte[i];
                            textBoxPodatki.Text = "Text: " + karte[i].text + "\r\n"
                                            + "Premik: " + karte[i].premik + "\r\n"
                                            + "Napad: " + karte[i].napad + "\r\n"
                                            + "Življenje: " + karte[i].zivljenje + "\r\n"
                                            + "Oklep: " + karte[i].oklep + "\r\n"
                                            + "Razdalja napada: " + karte[i].razdaljaNapada + "\r\n"
                                            + "Igralec: " + igralecString(karte[i].rasa);
                            indexAktivne = i;

                            break;
                        }
                    }
                }
                /*else
                    textBoxPodatki.Text = "";*/
                
                if (x == karte[indexAktivne].lokacija.X && y == karte[indexAktivne].lokacija.Y)
                {
                    if (karte[indexAktivne].premik <= 0) //če se ne more več premakniti je sploh ne izbere
                        return;
                    else if (karte[indexAktivne].rasa != rasa)
                    {
                        //MessageBox.Show("karte[indexAktivne] enota pripada drugemu igralcu!");
                        textBoxDogodki.Text += "Enota pripada drugemu igralcu!\r\n";
                        return;
                    }

                    aktivnaKarta.X = x;
                    aktivnaKarta.Y = y; //nastavi koordinate aktivne karte


                    oznaciPremike(karte[indexAktivne]); //pobarva mogoča polja za premik
                    //break;
                }
                
            }
            else if (veljavnaPostavitev(rasa) && !jeZasedena(locNov) && !kartaIzbrana) //če lahko postavi novo enoto in enota ni že izbrana (da se lahko premakne v bližino baze)
            {
                buttonPoklici.Enabled = true;
                aktivnaKarta.X = aktivnaKarta.Y = -1; //nastavi nazaj na -1, da pove, da je poteza zaključena
                return;
            }
            else //prestavi karto (izbere cilj)
            {
                kartaIzbrana = false;

                Point tmpPoint = new Point(x, y);
                bool polno = jeZasedena(tmpPoint); //preveri, ali je ciljna lokacija zasedena
                if (!polno) //če gre za premik
                    prestavi(indexAktivne, tmpPoint);
                else //če gre za napad
                    napadi(indexAktivne, tmpPoint);

                aktivnaKarta.X = aktivnaKarta.Y = -1; //nastavi nazaj na -1, da pove, da je poteza zaključena

                premikiBack.Clear();
                napadiBack.Clear();
                igralnoObmocje.Refresh();
            }
        }

        private void comboBoxEnote_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (comboBoxEnote.SelectedIndex == -1) //ko nastavim na -1 ne želim tega eventa
                return;*/
            textBoxPodatki.Text = "";
            string tmp;
            for (int i = comboBoxEnote.SelectedIndex * Gl.stAt; i < comboBoxEnote.SelectedIndex * Gl.stAt + Gl.stAt - 1; i++) //zadnji podatek je slika in ni pomemben
            {
                tmp = (atributi[i] + ": " + podatki[i] + "\r\n").Replace("_", " ");
                textBoxPodatki.Text += tmp.First().ToString().ToUpper() + String.Join("", tmp.Skip(1)); //izpis podatkov o enoti v textbox
            }
        }

        private void buttonVrzi_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Metanje je trenutno avtomatizirano. Verjetno se bo sistem pridobivanja točk spremenil.");
            /*Random rnd = new Random();
            int prva = rnd.Next(1, 7);
            int druga = rnd.Next(1, 7);
            textBoxVrzeno.Text = "Vrženo: " + prva.ToString() + ", " + druga.ToString();

            //TODO za posebne enote, unless you change it

            textBoxStanje.Text = (Int32.Parse(textBoxStanje.Text) + prva + druga).ToString();*/
        }

        private void buttonPoklici_Click(object sender, EventArgs e)
        {
            if (jeZasedena(locNov))
            {
                textBoxDogodki.Text += "Polje je že zasedeno! Izberi drugo polje.\r\n";
                return;
            }
            //else if (comboBoxEnote.Text != "")
            else
            {
                if (Int32.Parse(podatki[comboBoxEnote.SelectedIndex * Gl.stAt]) <= Int32.Parse(textBoxStanje.Text)) //v podatki so shranjeni podatki enot trenutnega igralca
                {
                    int i = comboBoxEnote.SelectedIndex;

                    try
                    {
                        dodaj(new Karta(trenutnaRasa(),
                                        comboBoxEnote.SelectedText,
                                        Int32.Parse(podatki[i * Gl.stAt]),
                                        podatki[i * Gl.stAt + 1],
                                        Int32.Parse(podatki[i * Gl.stAt + 2]),
                                        Int32.Parse(podatki[i * Gl.stAt + 3]),
                                        Int32.Parse(podatki[i * Gl.stAt + 4]),
                                        Int32.Parse(podatki[i * Gl.stAt + 5]),
                                        Int32.Parse(podatki[i * Gl.stAt + 6]),
                                        Int32.Parse(podatki[i * Gl.stAt + 7]),
                                        Image.FromFile("Slike/" + Regex.Replace(podatki[i * Gl.stAt + 8], @"[\d-]", "") + "/" + podatki[i * Gl.stAt + 8] + ".png"),
                                        locNov));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    //textBoxStanje.Text = (Int32.Parse(textBoxStanje.Text) - Int32.Parse(podatki[i * Global.stAt])).ToString();

                    //shrani novo enoto na dano lokacijo in pripravi polje za nadaljevanje
                    locNov.X = locNov.Y = -1;
                    buttonPoklici.Enabled = false;
                }
                else
                {
                    textBoxDogodki.Text += "Nimaš dovolj točk!\r\n";
                }
            }
        }

        private void buttonKonecPoteze_Click(object sender, EventArgs e)
        {
            koncajPotezo();
        }

        private void Igra_Resize(object sender, EventArgs e) //zaokroži velikost igralnega polja na števila deljiva z Global.N in tako prepreči napake v računanju
        {
            igralnoObmocje.Width = this.Width - 280; //da pri spreminjanju velikosti igralno polje ne postaja vedno manjše
            igralnoObmocje.Height = this.Height - 60;

            if (igralnoObmocje.Width % (Gl.N * 2) != 0) //Global.N*2 zaradi nepolnega risanja v funkciji igralnoObmocje_Paint(...)
                igralnoObmocje.Width -= igralnoObmocje.Width % (Gl.N * 2);
            if (igralnoObmocje.Height % (Gl.N * 2) != 0)
                igralnoObmocje.Height -= igralnoObmocje.Height % (Gl.N * 2);

            foreach (Karta karta in karte) //shrani številčno vrednost polja
            {
                karta.lokacija.X /= velikost.X;
                karta.lokacija.Y /= velikost.Y;
            }

            //MessageBox.Show(igralnoPolje[0].X + " " + igralnoPolje[3].Y); //mere celice
            int j = 1;
            for (int i = 1; i < (Gl.N - 1) * 4; i += 4) //določitev igralnega polja
            {
                igralnoPolje[i - 1] = new Point(j * igralnoObmocje.Width / Gl.N, 0);
                igralnoPolje[i] = new Point(j * igralnoObmocje.Width / Gl.N, igralnoObmocje.Height);
                igralnoPolje[i + 1] = new Point(0, j * igralnoObmocje.Height / Gl.N);
                igralnoPolje[i + 2] = new Point(igralnoObmocje.Width, j * igralnoObmocje.Height / Gl.N);
                j++;
            }
            velikost = new Point(igralnoObmocje.Width / Gl.N, igralnoObmocje.Height / Gl.N); //velikost posamezne slike

            foreach (Karta karta in karte) //shrani nove lokacije kart
            {
                karta.lokacija.X *= velikost.X;
                karta.lokacija.Y *= velikost.Y;
            }

            //this.panel1.Visible = true;

            igralnoObmocje.Refresh();
        }

        private void Igra_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode) //določi, da ti tipki posebej handlaš
            {
                case Keys.Pause:
                case Keys.Escape:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void Igra_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Pause:
                case Keys.Escape:
                    MessageBox.Show("GL.koncaj = true");
                    Gl.koncaj = true;
                    break;
            }
        }

        private void textBoxDogodki_TextChanged(object sender, EventArgs e)
        {
            if (textBoxDogodki.Lines.Count() >= textBoxDogodki.Height / Gl.font_size) //ko je poln izbriše prvo vrstico
                textBoxDogodki.Text = textBoxDogodki.Text.Substring(textBoxDogodki.Lines[0].Length + Environment.NewLine.Length);
        }



        /*
         * 
         * 
         * 
         * 
         * funkcije nad objekti konec
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * custom
         * 
         * 
         * 
         * 
         */


    

        void shraniZacetne()
        {
            //karte.Add(new Karta(1, Global.baza, 0, "", 0, 5, 50, 0, 3, Properties.Resources.Bela, new Point(0, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče +1 to all
            //karte.Add(new Karta(2, Global.baza, 0, "", 0, 5, 50, 0, 3, Properties.Resources.Zelena, new Point(igralnoObmocje.Width - igralnoObmocje.Width / Global.N, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
            //karte.Add(new Karta(3, Global.baza, 0, "", 0, 5, 50, 0, 3, Properties.Resources.Crna, new Point(0, igralnoObmocje.Height - igralnoObmocje.Height / Global.N))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
            //karte.Add(new Karta(4, Global.baza, 0, "", 0, 5, 50, 0, 3, Properties.Resources.Rdeca, new Point(igralnoObmocje.Width - igralnoObmocje.Width / Global.N, igralnoObmocje.Height - igralnoObmocje.Height / Global.N))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
            //trdnjave imajo premik, da lahko napadajo
            try
            {
                if (aktivni[0])
                    dodaj(new Karta(1, Gl.baza, 0, "Utrdba v kateri se trenirajo nove enote.", 1, 3, 50, Gl.oklepTrd, 1, 0, Image.FromFile("Slike/Trdnjave/Bela.png"), new Point(0, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče +1 to all
                if (aktivni[1])
                    dodaj(new Karta(2, Gl.baza, 0, "Utrdba v kateri se trenirajo nove enote.", 1, 3, 50, Gl.oklepTrd, 1, 0, Image.FromFile("Slike/Trdnjave/Crna.png"), new Point(igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
                if (aktivni[2])
                    dodaj(new Karta(3, Gl.baza, 0, "Utrdba v kateri se trenirajo nove enote.", 1, 3, 50, Gl.oklepTrd, 1, 0, Image.FromFile("Slike/Trdnjave/Rdeca.png"), new Point(0, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
                if (aktivni[3])
                    dodaj(new Karta(4, Gl.baza, 0, "Utrdba v kateri se trenirajo nove enote.", 1, 3, 50, Gl.oklepTrd, 1, 0, Image.FromFile("Slike/Trdnjave/Zelena.png"), new Point(igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N)));
            }
            catch (Exception)
            {
                MessageBox.Show("Ne najdem vseh trdnjav!");
                Application.Exit();
            }

            foreach (Karta karta in karte)
                karta.maxPremik = karta.premik; //nastavi premike
        }

        void dodaj(Karta karta) //kot argument vzame kot slike levo zgoraj
        {
            karta.maxPremik = karta.premik;
            karta.maxZivljenje = karta.zivljenje;
            karte.Add(karta);

            if (karta.tip != 0) //v primeru da je dodana karta baza, ne pride do sprmembe točk
            {
                int stanje = Int32.Parse(textBoxStanje.Text) - karta.cena;
                spremembaTock(stanje);
                //MessageBox.Show(karta.cena.ToString());
            }

            igralnoObmocje.Refresh();
        }

        int brisi(int index, int indexNap) //izbriše karto
        {
            dobljeneTocke[trenutnaRasa() - 1] += karte[index].cena; //napadalec dobi točke enake vrednosti uničene enote

            //textBoxDogodki.Text += "Enota " + karte[index].ime + " je bi la pokončana s strani enote " + karte[indexNap].ime + "\r\n";

            karte.RemoveAt(index);
            igralnoObmocje.Refresh();

            if (index < indexNap) //če je bila uničena enota dlje na polju
                indexNap--; //zmanjša index po brisanju, ker se polje prav tako zmanjša

            return indexNap;
        }

        void prestavi(int index, Point cilj) //index karte v obdelavi ter lokacija cilja
        {
            //MessageBox.Show((Math.Abs(cilj.X - karte[index].lokacija.X) / velikost.X + Math.Abs(cilj.Y - karte[index].lokacija.Y) / velikost.Y).ToString());
            if (karte[index].tip == 0)
                return;
            else if (karte[index].premik <= 0) //če ni več premikov
            {
                textBoxDogodki.Text += "Enota se ne more več premakniti! (to sporočilo ne bi smelo biti vidno)\r\n";
                return;
            }
            else if (karte[index].premik >= Math.Abs((cilj.X - karte[index].lokacija.X) / velikost.X) + Math.Abs((cilj.Y - karte[index].lokacija.Y) / velikost.Y)) //preveri veljavnost premika (I think...should comment before)
            {
                //MessageBox.Show((Math.Abs((cilj.X - karte[index].lokacija.X) / velikost.X) + Math.Abs((cilj.Y - karte[index].lokacija.Y) / velikost.Y)).ToString());
                karte[index].premik -= Math.Abs((cilj.X - karte[index].lokacija.X) / velikost.X) + Math.Abs((cilj.Y - karte[index].lokacija.Y) / velikost.Y);
                karte[index].lokacija = cilj;
                igralnoObmocje.Refresh();
            }
            else
                textBoxDogodki.Text += "Enota se lahko premakne le še za " + karte[index].premik + ".\r\n";
                //MessageBox.Show("Ta premik ni veljaven.", "Neveljavna poteza!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        string tipString(int tip)
        {
            switch (tip)
            {
                case 0:
                    return Gl.baza;
                case 1:
                    return "Pehota";
                case 2:
                    return "Lokostrelec";
                case 3:
                    return "Konjenik";
                case 4:
                    return "Artilerija";
                default:
                    return "UFO";
            }
        }

        int napadi(int indexNap, Point cilj) //funkcija za napad
        {
            //Debug.WriteLine("Razdalja med kartama v napadu: " + karte[indexNap].ime + " " + razdaljaMedKartama(karte[indexNap].lokacija, cilj));

            if (karte[indexNap].razdaljaNapada >= razdaljaMedKartama(karte[indexNap].lokacija, cilj)) //preveri, če je razdalja napada enote večja ali enaka razdalji med enotama
            {
                if (karte[indexNap].premik <= 0) //če ni več premikov
                {
                    textBoxDogodki.Text += "Enota ne more več napasti! (to sporočilo ne bi smelo biti vidno)\r\n";
                    return indexNap;
                }
                for (int i = 0; i < karte.Count; i++) //poišče ciljno karto
                {
                    if (karte[i].lokacija.X == cilj.X && karte[i].lokacija.Y == cilj.Y) //najdena ciljna karta
                    {
                        if (karte[i].tip == 0 && karte[indexNap].tip != 4 || karte[i].tip != 0 && karte[indexNap].tip == 4) //če artilerija napada karkoli razen trdnjave oz. če karkoli napada artilerijo
                        {
                            textBoxDogodki.Text += tipString(karte[indexNap].tip) + " ne more napasti enote tipa " + tipString(karte[i].tip).ToLower() + "!";
                            return indexNap;
                        }
                        else if (karte[indexNap].rasa == karte[i].rasa ||
                                (karte[indexNap].lokacija.X == karte[i].lokacija.X && karte[indexNap].lokacija.Y == karte[i].lokacija.Y))
                            return indexNap;
                        else if (karte[indexNap].napad >= karte[i].oklep + karte[i].zivljenje) //če karto ubije
                        {
                            if (karte[i].tip == 0) //poseben primer, ko pride do uničenja igralca
                            {
                                indexNap = igralecUnicen(indexNap, i);
                                if (racunalnik)
                                    AI(); //AI neha delat, or something
                            }
                            else
                                indexNap = brisi(i, indexNap);

                            //karte[index].lokacija = new Point(cilj.X, cilj.Y); //check if it works with only "cilj" instead of creating new point
                            break;
                        }
                        else if (karte[indexNap].napad <= karte[i].oklep) //če ne more prebiti oklepa
                        {
                            textBoxDogodki.Text += "Enota " + karte[i].ime + " ni bila poškodovana!\r\n";
                            break;
                        }
                        else //če spopad poteka normalno
                        {
                            karte[i].zivljenje -= karte[indexNap].napad - karte[i].oklep;
                            textBoxDogodki.Text += "Enoti " + karte[i].ime + " je bilo narejeno " + (karte[indexNap].napad - karte[i].oklep) + " škode!\r\n";
                            break;
                        }
                    }
                }

                igralnoObmocje.Refresh();
                try
                {
                    karte[indexNap].premik--;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(indexNap.ToString() + "\r\n" + ex.Message);
                }
            }
            else
                textBoxDogodki.Text += "Enota ne more napasti s takšne razdalje!\r\n";
                //MessageBox.Show("Enota ne more napasti s takšne razdalje.\n\rNapadalec: " + karte[indexNap].lokacija.X + " " + karte[indexNap].lokacija.Y + "\n\rBranilec: " + cilj.X + " " + cilj.Y + "\n\rRazdalja: " + razdaljaMedKartama(karte[indexNap].lokacija, cilj).ToString());

            return indexNap;
        }

        int brisanjeIgralca(Karta karta, int igralec, int indexNap)
        {
            aktivni[igralec] = false;
            for (int i = karte.Count - 1; i >= 0; i--) //brisanje vseh enot tega igralca
            {
                if (karte[i].rasa == igralec + 1)
                {
                    indexNap = brisi(i, indexNap);
                }
            }
            return indexNap;
        }

        int igralecUnicen(int indexNap, int indexTrd) //indeks karte, ki napada in trdnjave, ki je uničena, vrne posodobljen index napadalca
        {
            switch (karte[indexTrd].rasa)
            {
                case 1:
                    indexNap = brisanjeIgralca(karte[indexTrd], 0, indexNap); //izbriše igralce in na novo določi index napadalca, saj se je verjetno spremenil
                    textBoxDogodki.Text += "Beli so bili uničeni!\r\n";
                    break;
                case 2:
                    indexNap = brisanjeIgralca(karte[indexTrd], 1, indexNap);
                    textBoxDogodki.Text += "Črni so bili uničeni!\r\n";
                    break;
                case 3:
                    indexNap = brisanjeIgralca(karte[indexTrd], 2, indexNap);
                    textBoxDogodki.Text += "Rdeči so bili uničeni!\r\n";
                    break;
                case 4:
                    indexNap = brisanjeIgralca(karte[indexTrd], 3, indexNap);
                    textBoxDogodki.Text += "Zeleni so bili uničeni!\r\n";
                    break;
                default:
                    textBoxDogodki.Text += "Uničena je bila nepoznana rasa?!\r\n";
                    break;
            }

            int cnt = 0, igralec = 0;
            for (int i = 0; i < 4; i++)
            {
                if (aktivni[i])
                {
                    igralec = i + 1; //če ni več dovolj živih igralcev za nadaljevanje
                    cnt++;
                }
            }

            if (cnt < 2) //če je samo še en igralec
            {
                MessageBox.Show("Igre je konec. Zmagal je " + igralecString(igralec) + ".");
                igralnoObmocje.Enabled = false;
                for (int i = 0; i < 4; i++)
                    AIcontroled[i] = false; //da AI neha igrat
                racunalnik = false;
            }

            return indexNap;
        }

        int vrniIgralca() //vrne številko trenutnega igralca
        {
            return zivi[naVrsti - 1] + 1;
        }

        string igralecString(int igralec)
        {
            if(1 == igralec)
                return "beli";
            else if(2 == igralec)
                return "črni";
            else if(3 == igralec)
                return "rdeči";
            else if(4 == igralec)
                return "zeleni";
            else
                return "hrošč";
        }

        bool jeZasedena(Point lokacija) //preveri, če je lokacija zasedena
        {
            //Debug.WriteLine("Lokacija v funkciji jeZasedena(...) " + lokacija.X + " " + lokacija.Y);
            for (int i = 0; i < karte.Count; i++) //gre skozi vse karte in preverja, če je lokacija zasedena
                if (lokacija.X == karte[i].lokacija.X && lokacija.Y == karte[i].lokacija.Y)
                    return true;
            return false;
        }

        void naloziPodatke(string datoteka, List<string> atributi, List<string> podatki, List<string>enote)
        {
            XmlReader reader = new XmlTextReader(datoteka);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //atributi.Insert(0, reader.Name);
                        atributi.Add(reader.Name);
                        break;

                    case XmlNodeType.Text:
                        podatki.Add(reader.Value.Replace(".png", "")); //odstrani končnico slike za večnamensko uporabo (subfolderji za enote)
                        break;
                }
            }

            atributi.RemoveAt(0);
            for(int i=0; i<atributi.Count; i++)
            {
                if (i % Gl.stAt == 0)
                {
                    enote.Add(atributi[i].Replace('_', ' ')); //nadomesti podčrtaje z presledki pred shranjevanjem za lepši pregled
                    atributi.RemoveAt(i);
                }
            }

            for (int i = 0; i < enote.Count; i++)
                comboBoxEnote.Items.Insert(i, enote[i]);

            comboBoxEnote.SelectedIndex = 0;
        }

        void zacetekKroga() //kateri igralec je na vrsti
        {
            int stIgralcev = 0;
            zivi = new List<int>(); //seznam v katerem bodo zabeleženi indexi obstoječih igralcev
            for (int i = 0; i < 4; i++)
                if (aktivni[i])
                {
                    stIgralcev++; //vrne trenutno število igralcev
                    zivi.Add(i); //0=bela 1=crna 2=rdeca 3=zelena
                }

            if (naVrsti >= stIgralcev)
                naVrsti = 1;
            else
                naVrsti++;

            //MessageBox.Show(naVrsti.ToString() + " " + AIcontroled[naVrsti - 1]);

            if (naVrsti == 1)
            {
                dolociKdoJeNaVrsti(zivi[0]);
            }
            else if (naVrsti == 2)
            {
                dolociKdoJeNaVrsti(zivi[1]);
            }
            else if (naVrsti == 3)
            {
                dolociKdoJeNaVrsti(zivi[2]);
            }
            else if (naVrsti == 4)
            {
                dolociKdoJeNaVrsti(zivi[3]);
            }
            else
            {
                MessageBox.Show("Napaka, igralec ne obstaja :/");
            }

            textBoxStanje.Text = stanja[naVrsti - 1].ToString(); //nastavi začetno stanje
        }

        void dolociKdoJeNaVrsti(int igralec) //številka igralca, ki je na vrsti
        {
            int prosojnost = 200; //vrednost 0-255, kjer je 0 popolnoma prosojno, 255 pa popolnoma neprosojno
            if (igralec == 0)
            {
                naloziPodatke("bela.xml", atributi, podatki, enote);
                labelNaVrsti.Text = "Beli";
                igralnoObmocje.BackColor = Color.FromArgb(prosojnost, Color.LightYellow);
                racunalnik = AIcontroled[0];
            }
            else if (igralec == 1)
            {
                naloziPodatke("crna.xml", atributi, podatki, enote);
                labelNaVrsti.Text = "Črni";
                igralnoObmocje.BackColor = Color.FromArgb(prosojnost, Color.DarkGray);
                racunalnik = AIcontroled[1];    
            }
            else if (igralec == 2)
            {
                naloziPodatke("rdeca.xml", atributi, podatki, enote);
                labelNaVrsti.Text = "Rdeči";
                igralnoObmocje.BackColor = Color.FromArgb(prosojnost, Color.Pink);
                racunalnik = AIcontroled[2];
            }
            else if(igralec == 3)
            {
                naloziPodatke("zelena.xml", atributi, podatki, enote);
                labelNaVrsti.Text = "Zeleni";
                igralnoObmocje.BackColor = Color.FromArgb(prosojnost, Color.LightGreen);
                racunalnik = AIcontroled[3];
            }
            else
                MessageBox.Show("Nihče ni več živ?!");
        }

        void inicializacija()
        {
            aktivnaKarta = new Point(-2, -2);
            locNov = new Point(-1, -1);
            kartaIzbrana = false;
            naVrsti = 0;

            igralnoObmocje.Enabled = true;
            igralnoObmocje.BackColor = Color.Transparent;
            karte.Clear();
            atributi.Clear();
            enote.Clear();
            podatki.Clear();
            zivi.Clear();
            shraniZacetne();
            comboBoxEnote.Items.Clear();
            labelFazaIgre.Text = "";

            for (int i = 0; i < 4; i++) //vsa začetna stanja nastavi na default
            {
                stanja[i] = 0;
                dobljeneTocke[i] = 0;
            }

            igra();
        }

        void igra()
        {
            //določitev kdo je na vrsti
            zacetekKroga();

            povecavaTock();

            if (racunalnik)
                AI();
        }

        bool veljavnaPostavitev(int rasa) // 1-4
        {
            //MessageBox.Show(locNov.X.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2).ToString() + "\r\n" + locNov.Y.ToString() + " == " + (igralnoObmocje.Height - igralnoObmocje.Height / Gl.N).ToString());

            if (rasa == 1 &&
                (jeLokacijaKarte(locNov, igralnoObmocje.Width / Gl.N, 0) || //[1, 0]
                jeLokacijaKarte(locNov, 0, igralnoObmocje.Height / Gl.N) || //[0, 1]
                jeLokacijaKarte(locNov, igralnoObmocje.Width / Gl.N, igralnoObmocje.Height / Gl.N))) //[1, 1]
                return true;

            else if (rasa == 2 &&
                (jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, 0) || //[6, 0]
                jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, igralnoObmocje.Height / Gl.N) || //[7, 1]
                jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, igralnoObmocje.Height / Gl.N))) //[6, 1]
                return true;

            else if (rasa == 3 &&
                (jeLokacijaKarte(locNov, igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N) || //[1, 7]
                jeLokacijaKarte(locNov, 0, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2) || //[0, 6]
                jeLokacijaKarte(locNov, igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2))) //[1, 6]
                return true;

            else if (rasa == 4 &&
                (jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N) || //[6, 7]
                jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2) ||//[7, 6]
                jeLokacijaKarte(locNov, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2))) //[6, 6]
                return true;

            buttonPoklici.Enabled = false;
            return false;
        }

        int trenutnaRasa()
        {
            if (naVrsti == 1)
                return zivi[0] + 1;
            else if (naVrsti == 2)
                return zivi[1] + 1;
            else if (naVrsti == 3)
                return zivi[2] + 1;
            else if (naVrsti == 4)
                return zivi[3] + 1;
            else
            {
                MessageBox.Show("Ni veljavne rase?!");
                return 0;
            }
        } //1 - 4

        void oznaciPremike(Karta karta)
        {
            //if (karta.ime == Global.baza) //trdnjave se ne morejo premakniti, lahko le napadajo
              //  return;
            if (karta.premik > 0) //če se karta karta lahko premakne
            {
                int x, y, premik = karta.premik, razdalja;
                Point tarca;

                for (x = 0; x < igralnoObmocje.Width; x += velikost.X)
                {
                    for (y = 0; y < igralnoObmocje.Height; y += velikost.Y)
                    {
                        tarca = new Point(x, y);
                        if((razdalja = razdaljaMedKartama(karta.lokacija, tarca)) <= karta.premik) //premiki
                        {
                            if (!jeZasedena(tarca))
                                premikiBack.Add(new Karta(tarca));
                        }
                        
                        if (razdalja <= karta.razdaljaNapada)
                        {
                            if (!jeZasedena(tarca))
                                napadiBack.Add(new Karta(tarca));
                            else
                            {
                                foreach (Karta enota in karte) //pogleda, če je tam prijateljska karta
                                {
                                    if (enota.lokacija.X == x && enota.lokacija.Y == y)
                                    {
                                        if (enota.rasa != trenutnaRasa())
                                            napadiBack.Add(new Karta(tarca)); //nariše tarčo na prostorih v dosegu premika, kjer so sovražne enote
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                igralnoObmocje.Refresh(); //nariše
            }
        }

        void fullscreen()
        {
            this.BackgroundImage = null; //izbriše sliko

            if (fullscreenToolStripMenuItem.Checked)
            {
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                fullscreenToolStripMenuItem.Checked = false;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                //this.TopMost = true;
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                fullscreenToolStripMenuItem.Checked = true;
            }

            try
            {
                this.BackgroundImage = Properties.Resources.background; //ko je konec spreminjanja velikosti ponovno nastavi sliko
            }
            catch (Exception)
            {
                MessageBox.Show("Slika za ozadje ni bila najdena.");
            }
        }

        void povecavaTock()
        {
            //pridobitev podatkov
            //textBoxStanje.Text = stanja[naVrsti - 1].ToString(); //ni potrebno, se mi zdi

            //metanje kocke
            /*Random rnd = new Random();
            int prva = rnd.Next(1, 7);
            int druga = rnd.Next(1, 7);
            textBoxVrzeno.Text = "Vrženo: " + prva.ToString() + ", " + druga.ToString();*/
            //TODO za posebne enote, unless you change it

            int stanje = dobljeneTocke[trenutnaRasa() - 1] + 5; //za koliko se stanje poveča
            dobljeneTocke[trenutnaRasa() - 1] = 0; //resetira dobljene točke

            int tmp;
            if (!Int32.TryParse(textBoxStanje.Text, out tmp)) //če stanje ni int, se nastavi na 0
                textBoxStanje.Text = "0";

            spremembaTock(Int32.Parse(textBoxStanje.Text) + stanje);

            //stanja[naVrsti - 1] = Int32.Parse(textBoxStanje.Text) + prva + druga; //shrani novo stanje

            //textBoxStanje.Text = stanja[naVrsti - 1].ToString();
        }

        void spremembaTock(int novoStanje) //funkcija se uporablja pri klicanju nove karte, ter na začetku kroga
        {
            textBoxStanje.Text = novoStanje.ToString();
            stanja[naVrsti - 1] = novoStanje;
        }

        Point lokacijaAI()
        {
            for (locNov.X = 0; locNov.X <= igralnoObmocje.Width; locNov.X += velikost.X)
            {
                for (locNov.Y = 0; locNov.Y <= igralnoObmocje.Height; locNov.Y += velikost.Y)
                {
                    //MessageBox.Show(locNov.X.ToString() + " " + locNov.Y.ToString() + "\r\n" + velikost.X + " " + velikost.Y + "\r\n" + igralnoObmocje.Width + " " + igralnoObmocje.Height);
                    //MessageBox.Show(trenutnaRasa().ToString());
                    if (veljavnaPostavitev(trenutnaRasa()))
                    {
                        if(!jeZasedena(locNov))
                            return locNov;
                    }
                }
            }
            locNov.X = -1;
            locNov.Y = -1;

            return locNov; //če ne najde
        }

        bool jeLokacijaKarte(Point karta, int x, int y) //primerja, če se lokacija karte ujema z x in y
        {
            if (karta.X == x && karta.Y == y)
                return true;
            return false;
        }

        int zasedenost(int x1, int y1, int x2, int y2) //koordinate blizu baze, kjer se lahko spawnajo (podatke o srednji točki lahko pridobim iz teh dveh)
        {
            int zasedene = 0;
            foreach (Karta karta in karte)
            {
                if ((jeLokacijaKarte(karta.lokacija, x1, y1) ||
                    jeLokacijaKarte(karta.lokacija, x2, y2) ||
                    jeLokacijaKarte(karta.lokacija, x1, y2)) &&
                    karta.rasa != trenutnaRasa())
                    zasedene++;
                /*if ((karta.lokacija.X == igralnoObmocje.Width / Gl.N && karta.lokacija.Y == 0 || //[1, 0]
                    karta.lokacija.X == 0 && karta.lokacija.Y == igralnoObmocje.Height / Gl.N || //[0, 1]
                    karta.lokacija.X == igralnoObmocje.Width / Gl.N && karta.lokacija.Y == igralnoObmocje.Height / Gl.N) && //[1, 1]
                    karta.rasa != trenutnaRasa())
                    zasedene++;*/
            }
            return zasedene;
        }

        void poisciTrdnjavoZaBrisanje()
        {
            for (int i = 0; i < karte.Count; i++) //moralo bi najt v prvih štirih kartah
            {
                if (karte[i].tip == 0 && karte[i].rasa == trenutnaRasa())
                {
                    textBoxDogodki.Text += igralecString(trenutnaRasa()) + " " + Gl.baza + " je bila zavzeta!\r\n";
                    igralecUnicen(0, i); //prva številka ni relevantna
                    break;
                }
            }
        }

        void koncajPotezo()
        {
            int zasedene = 0; //če pride do tri pomeni, da so vsa mesta pri bazi zasedena s strani sovražnih enot
            if (trenutnaRasa() == 1) //če so vsa mesta ob bazi zasedena, preveri kdo jih zaseda
            {
                zasedene = zasedenost(igralnoObmocje.Width / Gl.N, 0, 0, igralnoObmocje.Height / Gl.N);
                if(zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();             
            }
            else if(trenutnaRasa() == 2)
            {
                zasedene = zasedenost(igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, 0, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, igralnoObmocje.Height / Gl.N);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }
            else if (trenutnaRasa() == 3)
            {
                zasedene = zasedenost(igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N, 0, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }
            else
            {
                zasedene = zasedenost(igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N, igralnoObmocje.Width - igralnoObmocje.Width / Gl.N, igralnoObmocje.Height - igralnoObmocje.Height / Gl.N * 2);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }            

            atributi.Clear();
            podatki.Clear();
            enote.Clear();
            comboBoxEnote.Items.Clear();

            premikiBack.Clear(); //počisti premike tudi
            napadiBack.Clear();

            foreach (Karta karta in karte)
                karta.premik = karta.maxPremik;
            igra();
            comboBoxEnote.Enabled = true;
            buttonPoklici.Enabled = false;
            textBoxPodatki.Text = "";
        }

        int razdVod(Point prva, Point druga) //vrne vodoravno razdaljo med kartama, oz. negativno vrednost, če je prva nad drugo
        {
            return prva.X / velikost.X - druga.X / velikost.X;
        }

        int razdNavp(Point prva, Point druga) //vrne navpično razdaljo med kartama, oz. negativno vrednost, če je prva levo od druge
        {
            return prva.Y / velikost.Y - druga.Y / velikost.Y;
        }

        int razdaljaMedKartama(Point prva, Point druga) //sprejme koordinate kart vrne pa razdaljo v poljih
        {
            return Math.Abs(razdVod(prva, druga)) + Math.Abs(razdNavp(prva, druga));
        }

        Point resetPremika(Point trenutna, Point tarca, bool vodoravno) //cilj = kamor se premakne, tarča = dejanska tarča (do nje še ne more prit)
        {
            Debug.WriteLine("Reset");
            Point point = new Point(-1, -1);
            Point tmpPoint; //za vmesne
            int razdalja = Int32.MaxValue; //če je več možnosti za premik izbere tistega, ki ga najbolj približa tarči

            if (trenutna.X > 0 && !jeZasedena(tmpPoint = new Point(trenutna.X - velikost.X, trenutna.Y))) //če je cilj na igralni površini in prost
            {
                point = tmpPoint;
                razdalja = razdaljaMedKartama(point, tarca);
            }
            
            if (trenutna.X < velikost.X * (Gl.N - 1) && !jeZasedena(tmpPoint = new Point(trenutna.X + velikost.X, trenutna.Y))) //če je cilj na igralni površini in prost
                if(razdalja > razdaljaMedKartama(tmpPoint, tarca))
                {
                    point = tmpPoint;
                    razdalja = razdaljaMedKartama(tmpPoint, tarca);
                }
            
            if (trenutna.Y > 0 && !jeZasedena(tmpPoint = new Point(trenutna.X, trenutna.Y - velikost.Y))) //če je cilj na igralni površini in prost
                if (razdalja > razdaljaMedKartama(tmpPoint, tarca))
                {
                    point = tmpPoint;
                    razdalja = razdaljaMedKartama(tmpPoint, tarca);
                }
            
            if (trenutna.Y < velikost.Y * (Gl.N - 1) && !jeZasedena(tmpPoint = new Point(trenutna.X, trenutna.Y + velikost.Y))) //če je cilj na igralni površini in prost
                if (razdalja > razdaljaMedKartama(tmpPoint, tarca))
                {
                    point = tmpPoint;
                    razdalja = razdaljaMedKartama(tmpPoint, tarca);
                }

            Debug.WriteLine(point.X + " " + point.Y);
            Debug.WriteLine("Failed");
            return point;
        }

        Point smerPremika(Point cilj, Point tarca, bool vodoravno) //cilj je lokacija na katero se premakne s tem premikom
        {
            if (razdVod(cilj, tarca) < 0) //če je na levi in mora desno (+)
            {
                if (vodoravno)
                    cilj.X += velikost.X;
                else
                {
                    if (razdNavp(cilj, tarca) < 0) //če je zgoraj in mora navzdol (+)
                        cilj.Y += velikost.Y;
                    else //če je spodaj in mora navzgor (-)
                        cilj.Y -= velikost.Y;
                }
            }
            else //če je na desni in mora levo (-)
            {
                if (vodoravno)
                    cilj.X -= velikost.X;
                else
                {
                    if (razdNavp(cilj, tarca) < 0) //če je zgoraj in mora navzdol (+)
                        cilj.Y += velikost.Y;
                    else //če je spodaj in mora navzgor (-)
                        cilj.Y -= velikost.Y;
                }
            }
            return cilj;
        }

        bool lahkoPoskoduje(int napad, int oklep)
        {
            if (napad > oklep)
                return true;
            return false;
        }

        int pathfinding(Point tarca, int i) //poskrbi za premike in napade, vrne nov index trenutne karte
        {
            Point trenutna = new Point(karte[i].lokacija.X, karte[i].lokacija.Y); //shrani začetno lokacijo iz katere se potem premika
            Point tmpTarca = new Point(-1, -1); //če ni tarče
            int tocke_premika = karte[i].premik;
            //Debug.WriteLine("Karta: " + i.ToString() + " " + karte[i].maxPremik + " " + karte[i].premik);

            while (tocke_premika > 0) //dokler se še lahko premakne
            {
                tmpTarca.X = tmpTarca.Y = -1;

                foreach(Karta karta in karte) //preveri, če lahko napade kakšno karto
                    if (karta.rasa != trenutnaRasa() && razdaljaMedKartama(trenutna, karta.lokacija) <= karte[i].razdaljaNapada && lahkoPoskoduje(karte[i].napad, karta.oklep))
                    {
                        //Debug.WriteLine("V dosegu karte s koordinatama: " + trenutna.X.ToString() + " " + trenutna.Y.ToString() + " je karta: " + karta.lokacija.X.ToString() + " " + karta.lokacija.Y.ToString());
                        tmpTarca.X = karta.lokacija.X;
                        tmpTarca.Y = karta.lokacija.Y;
                    }

                //if (Math.Abs(karte[i].lokacija.X - tarca.X) + Math.Abs(karte[i].lokacija.Y - tarca.Y) <= (karte[i].razdaljaNapada * (velikost.X + velikost.Y) / 2)) //če lahko doseže tarčo brez premika
                if (razdaljaMedKartama(trenutna, tarca) <= karte[i].razdaljaNapada && karte[i].tip == 4) //ko pride v razdaljo za napad trdnjave in je oven ali katapult
                {
                    while(tocke_premika > 0)
                    {
                        if (jeZasedena(tarca)) //če je še kaj tam
                        {
                            i = napadi(i, tarca);
                            tocke_premika--;
                        }
                        else
                            break;
                    }
                    //trenutna.X = tarca.X;
                    //trenutna.Y = tarca.Y;
                }
                else if (tmpTarca.X != -1 && tmpTarca.Y != -1) //če je bila tmpTarča določena in trdnjava ni v razdalji
                {
                    while (tocke_premika > 0)
                    {
                        if (jeZasedena(tmpTarca)) //če je še kaj tam
                        {
                            i = napadi(i, tmpTarca);
                            tocke_premika--;
                        }
                        else
                            break;
                    }
                }
                //else if (Math.Abs(razdVod(karte[i].lokacija, tarca)) < Math.Abs(razdNavp(karte[i].lokacija, tarca))) //pove, da ima po Y poti dlje, tako da bo priotiziral to smer
                else //če se samo premakne
                {
                    //MessageBox.Show(cilj.X.ToString() + " " + cilj.Y.ToString());
                    if (karte[i].tip == 0) //če je trdnjava se ne more premaknit
                        return i;

                    bool vodoravno; //v katero smer premaknit
                    if (Math.Abs(razdVod(trenutna, tarca)) > Math.Abs(razdNavp(trenutna, tarca))) // v vodoravni smeri je večja razdalja, tako da bo šel v to smer
                        vodoravno = true;
                    else
                        vodoravno = false;

#if debug
                    //MessageBox.Show(vodoravno.ToString());
                    //MessageBox.Show(trenutna.X.ToString() + " " + trenutna.Y.ToString());
#endif
                    tmpTarca = trenutna;

                    tmpTarca = smerPremika(tmpTarca, tarca, vodoravno);

                    /*foreach (Karta karta in karte) //gre skozi karte in poišče, če je katera na cilju
                        if (karta.rasa != trenutnaRasa() && jeZasedena(tmpTarca)) //če lokacija premika ni iste rase, bo napad
                        {
                            napadi(i, tmpTarca); //cilj bo ta lokacija
                            tocke_premika--;
                            if (tocke_premika == 0) //če z napadi zmanjka točk za premikanje
                                break;
                        }*/
                    //Debug.WriteLine("Tarča: (" + tmpTarca.X.ToString() + ", " + tmpTarca.Y.ToString() + ")");
#if debug
                    //MessageBox.Show("Točke: " + tocke_premika.ToString() + "\n\rZasedena: " + jeZasedena(tmpTarca).ToString());
                    /*if (tocke_premika == 1 && jeZasedena(tmpTarca))
                        //Debug.WriteLine("Pred spremembo: " + tmpTarca.X + " " + tmpTarca.Y);
                        MessageBox.Show("Pred spremembo: " + tmpTarca.X + " " + tmpTarca.Y);*/
#endif

                    if (jeZasedena(tmpTarca)) //če je ciljna lokacija za permik zasedena poskusi najti boljšo
                    {
                        tmpTarca = resetPremika(trenutna, tarca, vodoravno);
                        if (tmpTarca.X == -1 && tmpTarca.Y == -1) //če ni tarče
                            return i;
                    }

                    if (tocke_premika > 0) //če še lahko napravi kaj
                    {
                        prestavi(i, tmpTarca);
                        //Debug.WriteLine("Napaka v pathfindingu. Karta se je želela prestaviti na napačno " + tmpTarca.X.ToString() + " " + tmpTarca.Y.ToString());
                        tocke_premika--;

                        trenutna = tmpTarca; //shranjena zadnja lokacija za naslednjo iteracijo
                    }

                    Debug.WriteLine("Točke premika: " + tocke_premika.ToString());
                }
            }
            return i;
        }

        /*void prekini() //not working should figure how to put AI into a thread instead
        {
            while (true)
            {
                if (Gl.koncaj == true)
                {
                    koncaj = false;
                    DialogResult izhod = MessageBox.Show("Želiš prekiniti igro?", "Pause", MessageBoxButtons.YesNo);
                    if (izhod == DialogResult.Yes)
                        Application.Exit();
                }
                Thread.Sleep(1000);
            }
        }*/

        void AI() //labeltext je nastavljen na AI igralca
        {
            klici_vec_enot:

            int najvisji = 0; //najvišja enota, ki jo lahko "kupi"
            int locEnote = -1; //ni index, ampak lokacija na seznamu

            for(int i=0; i<podatki.Count; i+=Gl.stAt)
            {
                int cena = Int32.Parse(podatki[i]);
                if (Int32.Parse(textBoxStanje.Text) >= cena && najvisji <= cena) //na tem mestu je cena (me thinks :: smiley face ::)
                {
                    if (najvisji != cena)
                    {
                        najvisji = cena;
                        locEnote = i;
                    }
                    else
                    {
                        Random rnd = new Random();
                        int random = rnd.Next(0, 1);
                        if (random == 0) //malo naključnosti, če imajo enote isto ceno
                            locEnote = i;
                    }
                }
            }
            locNov = lokacijaAI();

            //MessageBox.Show(locNov.X.ToString() + ", " + locNov.Y.ToString());
            if (locNov.X != -1 && locNov.Y != -1 && locEnote != -1) //če je prostor
            {
                try
                {
                    dodaj(new Karta(trenutnaRasa(),
                                    "AI " + Regex.Replace(podatki[locEnote + 7], @"[\d-]", ""),
                                    Int32.Parse(podatki[locEnote]),
                                    podatki[locEnote + 1],
                                    Int32.Parse(podatki[locEnote + 2]),
                                    Int32.Parse(podatki[locEnote + 3]),
                                    Int32.Parse(podatki[locEnote + 4]),
                                    Int32.Parse(podatki[locEnote + 5]),
                                    Int32.Parse(podatki[locEnote + 6]),
                                    Int32.Parse(podatki[locEnote + 7]),
                                    Image.FromFile("Slike/" + Regex.Replace(podatki[locEnote + 8], @"[\d-]", "") + "/" + podatki[locEnote + 8] + ".png"),
                                    lokacijaAI()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("AI ne more dodati enote!\r\n" + ex.Message);
                }
                goto klici_vec_enot;
            }

            List<Karta> trdnjave = new List<Karta>();
            Karta glavnaTarca = new Karta();

            for(int i=0; i<karte.Count; i++)
            {
                if (karte[i].tip == 0 && karte[i].rasa != trenutnaRasa()) //za tarčo vzamejo sovražne trdnjave
                    trdnjave.Add(karte[i]);
                else if (karte[i].rasa == trenutnaRasa()) //napadalci, grejo v napad;
                {
                    if (trdnjave.Count > 1) //če je več sovražnikov
                    {
                        int razdalja = Int32.MaxValue; //največja razdalja
                        foreach (Karta trdnjava in trdnjave)
                            if (razdaljaMedKartama(karte[i].lokacija, trdnjava.lokacija) < razdalja)
                            {
                                razdalja = razdaljaMedKartama(karte[i].lokacija, trdnjava.lokacija);
                                glavnaTarca = trdnjava; //nastavi najbližjo trdnjavo za cilj
                            }
                    }
                    else if (karte[i].tip == 0) //trdnjave se ne morejo premikat in majo posebej
                    {
                        glavnaTarca.lokacija.X = velikost.X * Gl.N;
                        glavnaTarca.lokacija.Y = velikost.Y * Gl.N;
                    }
                    else
                        glavnaTarca = trdnjave[0];

                    i = pathfinding(glavnaTarca.lokacija, i); //shrani nov index karte

                    //Thread.Sleep(sleep);
                     //spi čas v milisekundah definiran zgoraj, da ne leti vse tako
                }
            }

            koncajPotezo();
        }

        /*
         * 
         * 
         * 
         * 
         * custom end
         * 
         * 
         * 
         * 
         */
    }
}

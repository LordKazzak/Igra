#define debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Igra
{
    public partial class Igra : Form
    {
        Point[] gameAreaBounds = new Point[(Gl.N-1)*4]; // Indexes of lines defining the playing board.

        const int sleep = 0;                // Sleep time
        int activeIndex, turn;              // Index of active player (1-4)
        bool computer, player4, chosenUnit; // false = 2 players, true = 4
        Point activeCard, locNov, size; 

        bool[] active, AIcontroled;         // Player state (active/inactive)
        int[] state, gainedPoints;          // Player points

        List<int> alive;                    // Active players
        List<string> units, attributes, data;      // Units of the active player.
        List<Unit> cards, moveBack, attackBack;    // List of units on the field valid fields.

        public Igra()
        {
            InitializeComponent();

            this.menuStrip1.BackColor = Color.Black;
            this.menuStrip1.ForeColor = Color.DeepPink;
            this.ForeColor = Color.DeepPink;

            textBoxEvents.Clear();

            player4 = false; //false = 2 players or true = 4

            state = new int[4];
            gainedPoints = new int[4];
            active = new bool[4];
            AIcontroled = new bool[4];
            cards = new List<Unit>();
            moveBack = new List<Unit>();
            attackBack = new List<Unit>();
            units = new List<string>();
            attributes = new List<string>();
            data = new List<string>();
            alive = new List<int>();

            active[0] = active[3] = true;
            active[1] = active[2] = false; // Starting state, changeable in options.

            AIcontroled[0] = AIcontroled[1] = AIcontroled[2] = false;
            AIcontroled[3] = true;

            this.BackgroundImageLayout = ImageLayout.Stretch;
            try
            {
                this.BackgroundImage = Properties.Resources.background;
            }
            catch (Exception)
            {
                MessageBox.Show("Background image not found!");
            }
            
            inicializacija();

            int j = 1;
            for (int i = 1; i < (Gl.N - 1) * 4; i += 4) // Designation of playing field
            {
                gameAreaBounds[i - 1] = new Point(j * gameArea.Width / Gl.N, 0);
                gameAreaBounds[i] = new Point(j * gameArea.Width / Gl.N, gameArea.Height);
                gameAreaBounds[i + 1] = new Point(0, j * gameArea.Height / Gl.N);
                gameAreaBounds[i + 2] = new Point(gameArea.Width, j * gameArea.Height / Gl.N);
                j++;
            }
            size = new Point(gameArea.Width / Gl.N, gameArea.Height / Gl.N); // Size of a tile.
            if (size.X == 0 || size.Y == 0)
            {
                MessageBox.Show("There was an error. Unit's size is: " + size.X + " " + size.Y);
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

            player4 = nova.stIgralcev(); //it works!

            for (int i = 0; i < 8; i += 2)
            {
                active[i / 2] = nova.data()[i]; //teoretično, sam bo treba še dost nastavit verjetno ...
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

            foreach (Unit karta in cards) // Draws all the cards.
            {
                if (karta.tip == 0)
                    e.Graphics.DrawImage(karta.slika, karta.lokacija.X, karta.lokacija.Y, size.X, size.Y);
                else
                    e.Graphics.DrawImage(karta.slika, karta.lokacija.X + (size.X / Gl.N / 2), karta.lokacija.Y + (size.Y / Gl.N / 2), size.X * (Gl.N - 1) / Gl.N, size.Y * (Gl.N - 1) / Gl.N);

                Font myFont = new Font("Dejavu Sans", 12); // One of the few supporting sword symbol.
                
                e.Graphics.DrawString("\u2764" + karta.zivljenje.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X + size.X - (karta.zivljenje.ToString().Length + 2) * 10, karta.lokacija.Y + size.Y - 18));
                e.Graphics.DrawString("\u2721" + karta.oklep.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X + size.X / 2 - (karta.zivljenje.ToString().Length + 2) * 5, karta.lokacija.Y + size.Y - 18));
                e.Graphics.DrawString("\u2694" + karta.napad.ToString(), myFont, Brushes.DeepPink, new Point(karta.lokacija.X, karta.lokacija.Y + size.Y - 18));
            }

            foreach (Unit karta in moveBack) // Displays possible moves
                e.Graphics.DrawImage(Properties.Resources.premik, karta.lokacija.X, karta.lokacija.Y, size.X, size.Y);

            foreach (Unit karta in attackBack) // Displays possible attacks
                e.Graphics.DrawImage(Properties.Resources.napad, karta.lokacija.X, karta.lokacija.Y, size.X, size.Y);

            for (int i = 0; i < gameAreaBounds.Length; i += 2)
                e.Graphics.DrawLine(pen, gameAreaBounds[i], gameAreaBounds[i + 1]);
        }

        private void igralnoObmocje_MouseDown(object sender, MouseEventArgs e)
        {
            int x, y;
            int faction = currentFaction();

            x = e.X / size.X;
            y = e.Y / size.Y;

            //MessageBox.Show(x.ToString() + ", " + y.ToString());

            //MessageBox.Show(locNov.X.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Global.N * 2).ToString() + "\n" + locNov.Y.ToString() + " == " + (igralnoObmocje.Height - igralnoObmocje.Height / Global.N * 2).ToString());

            x *= size.X;
            y *= size.Y; // Top left corner of the captured square.

            // Matrix [0,0] to [7,7]
            locNov = new Point(x, y);
                        
            if (activeCard.X == -1 && activeCard.Y == -1 && isOccupied(locNov)) //izbere enoto, ki jo želi premakniti
            {
                chosenUnit = true;

                for (int i = 0; i < cards.Count; i++)
                {
                    if (x == cards[i].lokacija.X && y == cards[i].lokacija.Y)
                    {
                        //izbrana = karte[i];
                        textBoxPodatki.Text = "Text:\t\t" + cards[i].text + "\r\n"
                                        + "Move:\t\t" + cards[i].premik + "\r\n"
                                        + "Attack:\t\t" + cards[i].napad + "\r\n"
                                        + "HP:\t\t" + cards[i].zivljenje + "\r\n"
                                        + "Armor:\t\t" + cards[i].oklep + "\r\n"
                                        + "Attack range:\t" + cards[i].razdaljaNapada + "\r\n"
                                        + "Player:\t\t" + playerString(cards[i].rasa);
                        activeIndex = i;
                        break;
                    }
                }

                if (x == cards[activeIndex].lokacija.X && y == cards[activeIndex].lokacija.Y)
                {
                    if (cards[activeIndex].premik <= 0) //če se ne more več premakniti je sploh ne izbere
                        return;
                    else if (cards[activeIndex].rasa != faction)
                    {
                        //MessageBox.Show("karte[indexAktivne] enota pripada drugemu igralcu!");
                        textBoxEvents.Text += "This unit belongs to another player!\r\n";
                        chosenUnit = false;
                        return;
                    }

                    activeCard.X = x;
                    activeCard.Y = y; // Sets coordinates of the active card.

                    markMoves(cards[activeIndex]);
                }

                //MessageBox.Show(x.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Global.N).ToString());
                //for (int i = 0; i < karte.Count; i++) //gre skozi karte, dokler ne najde prave
            }
            else if (validLocation(faction) && !isOccupied(locNov) && !chosenUnit)
            {
                buttonPoklici.Enabled = true;
                activeCard.X = activeCard.Y = -1; // Move completed.
                return;
            }
            else // Move unit
            {
                Point tmpPoint = new Point(x, y);
                bool polno = isOccupied(tmpPoint);
                if (!polno) // If move
                    prestavi(activeIndex, tmpPoint);
                else // If attack
                    napadi(activeIndex, tmpPoint);

                activeCard.X = activeCard.Y = -1; // Move completed.

                chosenUnit = false;

                moveBack.Clear();
                attackBack.Clear();
                gameArea.Refresh();
            }
        }

        private void comboBoxEnote_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxPodatki.Text = "";
            string tmp;
            for (int i = comboBoxEnote.SelectedIndex * Gl.stAt; i < comboBoxEnote.SelectedIndex * Gl.stAt + Gl.stAt - 1; i++) // Last data is image and not relevant here.
            {
                tmp = (attributes[i] + ": " + data[i] + "\r\n").Replace("_", " ");
                textBoxPodatki.Text += tmp.First().ToString().ToUpper() + String.Join("", tmp.Skip(1)); // Prints unit's data to textbox.
            }
        }

        private void buttonVrzi_Click(object sender, EventArgs e)
        {
            // dud
        }

        private void buttonPoklici_Click(object sender, EventArgs e)
        {
            if (isOccupied(locNov))
            {
                textBoxEvents.Text += "Field already taken.\r\n";
                return;
            }
            else
            {
                if (Int32.Parse(data[comboBoxEnote.SelectedIndex * Gl.stAt]) <= Int32.Parse(textBoxStanje.Text)) // Current player's units.
                {
                    int i = comboBoxEnote.SelectedIndex;

                    try
                    {
                        add(new Unit(currentFaction(),
                                        comboBoxEnote.SelectedText,
                                        Int32.Parse(data[i * Gl.stAt]),
                                        data[i * Gl.stAt + 1],
                                        Int32.Parse(data[i * Gl.stAt + 2]),
                                        Int32.Parse(data[i * Gl.stAt + 3]),
                                        Int32.Parse(data[i * Gl.stAt + 4]),
                                        Int32.Parse(data[i * Gl.stAt + 5]),
                                        Int32.Parse(data[i * Gl.stAt + 6]),
                                        Int32.Parse(data[i * Gl.stAt + 7]),
                                        Image.FromFile("../../Properties/Images/" + Regex.Replace(data[i * Gl.stAt + 8], @"[\d-]", "") + "/" + data[i * Gl.stAt + 8] + ".png"),
                                        locNov));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    // Saves the move.
                    locNov.X = locNov.Y = -1;
                    buttonPoklici.Enabled = false;
                }
                else
                {
                    textBoxEvents.Text += "Not enough points!\r\n";
                }
            }
        }

        private void buttonKonecPoteze_Click(object sender, EventArgs e)
        {
            koncajPotezo();
        }

        private void Igra_Resize(object sender, EventArgs e) // Makes sure the size of the field is divisible by Global.N (number of squares)
        {
            gameArea.Width = this.Width - 280;      // To prevent playing field from shrinking.
            gameArea.Height = this.Height - 60;

            if (gameArea.Width % (Gl.N * 2) != 0) //Global.N*2 due to innacurate drawing in igralnoObmocje_Paint(...)
                gameArea.Width -= gameArea.Width % (Gl.N * 2);
            if (gameArea.Height % (Gl.N * 2) != 0)
                gameArea.Height -= gameArea.Height % (Gl.N * 2);

            foreach (Unit karta in cards) // Stores location
            {
                karta.lokacija.X /= size.X;
                karta.lokacija.Y /= size.Y;
            }

            int j = 1;
            for (int i = 1; i < (Gl.N - 1) * 4; i += 4) // Defines field
            {
                gameAreaBounds[i - 1] = new Point(j * gameArea.Width / Gl.N, 0);
                gameAreaBounds[i] = new Point(j * gameArea.Width / Gl.N, gameArea.Height);
                gameAreaBounds[i + 1] = new Point(0, j * gameArea.Height / Gl.N);
                gameAreaBounds[i + 2] = new Point(gameArea.Width, j * gameArea.Height / Gl.N);
                j++;
            }
            size = new Point(gameArea.Width / Gl.N, gameArea.Height / Gl.N); // Image size

            foreach (Unit karta in cards) // Stores new location
            {
                karta.lokacija.X *= size.X;
                karta.lokacija.Y *= size.Y;
            }

            gameArea.Refresh();
        }

        private void Igra_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode) // Special keys
            {
                case Keys.Pause:
                    break;
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
            if (textBoxEvents.Lines.Count() >= textBoxEvents.Height / Gl.font_size) //ko je poln izbriše prvo vrstico
                textBoxEvents.Text = textBoxEvents.Text.Substring(textBoxEvents.Lines[0].Length + Environment.NewLine.Length);
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
                if (active[0])
                    add(new Unit(1, Gl.baza, 0, "Fortress where you recruit new units. To recruit a unit simply select it from dropdown and then select and adjacent tile.", 1, 3, 50, Gl.armorFort, 1, 0, Image.FromFile("../../Properties/Images/Forts/Bela.png"), new Point(0, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče +1 to all
                if (active[1])
                    add(new Unit(2, Gl.baza, 0, "Fortress where you recruit new units. To recruit a unit simply select it from dropdown and then select and adjacent tile.", 1, 3, 50, Gl.armorFort, 1, 0, Image.FromFile("../../Properties/Images/Forts/Crna.png"), new Point(gameArea.Width - gameArea.Width / Gl.N, 0))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
                if (active[2])
                    add(new Unit(3, Gl.baza, 0, "Fortress where you recruit new units. To recruit a unit simply select it from dropdown and then select and adjacent tile.", 1, 3, 50, Gl.armorFort, 1, 0, Image.FromFile("../../Properties/Images/Forts/Rdeca.png"), new Point(0, gameArea.Height - gameArea.Height / Gl.N))); //0 = belo, 1 = zeleno, 2 = črno, 3 = rdeče
                if (active[3])
                    add(new Unit(4, Gl.baza, 0, "Fortress where you recruit new units. To recruit a unit simply select it from dropdown and then select and adjacent tile.", 1, 3, 50, Gl.armorFort, 1, 0, Image.FromFile("../../Properties/Images/Forts/Zelena.png"), new Point(gameArea.Width - gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N)));
            }
            catch (Exception)
            {
                MessageBox.Show("Can not find Forts! (Properties/Images/Forts)");
                Application.Exit();
            }

            foreach (Unit karta in cards)
                karta.maxPremik = karta.premik; // Set moves
        }

        void add(Unit karta) // Top left corner as argument
        {
            karta.maxPremik = karta.premik;
            karta.maxZivljenje = karta.zivljenje;
            cards.Add(karta);

            if (karta.tip != 0) //v primeru da je dodana karta baza, ne pride do sprmembe točk
            {
                int stanje = Int32.Parse(textBoxStanje.Text) - karta.cena;
                spremembaTock(stanje);
                //MessageBox.Show(karta.cena.ToString());
            }

            gameArea.Refresh();
        }

        int delete(int index, int indexNap) //izbriše karto
        {
            gainedPoints[currentFaction() - 1] += cards[index].cena; //napadalec dobi točke enake vrednosti uničene enote

            //textBoxDogodki.Text += "Enota " + karte[index].ime + " je bi la pokončana s strani enote " + karte[indexNap].ime + "\r\n";

            cards.RemoveAt(index);
            gameArea.Refresh();

            if (index < indexNap) //če je bila uničena enota dlje na polju
                indexNap--; //zmanjša index po brisanju, ker se polje prav tako zmanjša

            return indexNap;
        }

        void prestavi(int index, Point cilj) //index karte v obdelavi ter lokacija cilja
        {
            if (!chosenUnit & !computer)
                return;

            //MessageBox.Show((Math.Abs(cilj.X - karte[index].lokacija.X) / velikost.X + Math.Abs(cilj.Y - karte[index].lokacija.Y) / velikost.Y).ToString());
            if (cards[index].tip == 0)
                return;
            else if (cards[index].premik <= 0) //če ni več premikov
            {
                textBoxEvents.Text += "The unit can no longer move.\r\n";
                return;
            }
            else if (cards[index].premik >= Math.Abs((cilj.X - cards[index].lokacija.X) / size.X) + Math.Abs((cilj.Y - cards[index].lokacija.Y) / size.Y)) //preveri veljavnost premika (I think...should comment before)
            {
                //MessageBox.Show((Math.Abs((cilj.X - karte[index].lokacija.X) / velikost.X) + Math.Abs((cilj.Y - karte[index].lokacija.Y) / velikost.Y)).ToString());
                cards[index].premik -= Math.Abs((cilj.X - cards[index].lokacija.X) / size.X) + Math.Abs((cilj.Y - cards[index].lokacija.Y) / size.Y);
                cards[index].lokacija = cilj;
                gameArea.Refresh();
            }
            else
                textBoxEvents.Text += "The unit has " + cards[index].premik + " movement points left.\r\n";
                //MessageBox.Show("Ta premik ni veljaven.", "Neveljavna poteza!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        string tipString(int tip)
        {
            switch (tip)
            {
                case 0:
                    return Gl.baza;
                case 1:
                    return "Infantry";
                case 2:
                    return "Archer";
                case 3:
                    return "Cavalry";
                case 4:
                    return "Artillery";
                default:
                    return "UFO";
            }
        }

        int napadi(int indexNap, Point cilj) //funkcija za napad
        {
            //Debug.WriteLine("Razdalja med kartama v napadu: " + karte[indexNap].ime + " " + razdaljaMedKartama(karte[indexNap].lokacija, cilj));
            if (!chosenUnit & !computer)
                return indexNap;

            if (cards[indexNap].razdaljaNapada >= razdaljaMedKartama(cards[indexNap].lokacija, cilj)) //preveri, če je razdalja napada enote večja ali enaka razdalji med enotama
            {
                if (cards[indexNap].premik <= 0) //če ni več premikov
                {
                    textBoxEvents.Text += "The unit can no longer attack.\r\n";
                    return indexNap;
                }
                for (int i = 0; i < cards.Count; i++) //poišče ciljno karto
                {
                    if (cards[i].lokacija.X == cilj.X && cards[i].lokacija.Y == cilj.Y) //najdena ciljna karta
                    {
                        if (cards[i].tip == 0 && cards[indexNap].tip != 4 || cards[i].tip != 0 && cards[indexNap].tip == 4) //če artilerija napada karkoli razen trdnjave oz. če karkoli napada artilerijo
                        {
                            textBoxEvents.Text += tipString(cards[indexNap].tip) + " can not attack a unit of type " + tipString(cards[i].tip).ToLower() + "\r\n!";
                            return indexNap;
                        }
                        else if (cards[indexNap].rasa == cards[i].rasa ||
                                (cards[indexNap].lokacija.X == cards[i].lokacija.X && cards[indexNap].lokacija.Y == cards[i].lokacija.Y))
                            return indexNap;
                        else if (cards[indexNap].napad >= cards[i].oklep + cards[i].zivljenje) //če karto ubije
                        {
                            if (cards[i].tip == 0) //poseben primer, ko pride do uničenja igralca
                            {
                                indexNap = igralecUnicen(indexNap, i);
                                if (computer)
                                    AI(); //AI neha delat, or something
                            }
                            else
                                indexNap = delete(i, indexNap);

                            //karte[index].lokacija = new Point(cilj.X, cilj.Y); //check if it works with only "cilj" instead of creating new point
                            break;
                        }
                        else if (cards[indexNap].napad <= cards[i].oklep) //če ne more prebiti oklepa
                        {
                            textBoxEvents.Text += "Unit " + cards[i].ime + " was not damaged because it's armor is too high!\r\n";
                            break;
                        }
                        else //če spopad poteka normalno
                        {
                            cards[i].zivljenje -= cards[indexNap].napad - cards[i].oklep;
                            textBoxEvents.Text += "Unit " + cards[i].ime + " lost " + (cards[indexNap].napad - cards[i].oklep) + " HP!\r\n";
                            break;
                        }
                    }
                }

                gameArea.Refresh();
                try
                {
                    cards[indexNap].premik--;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(indexNap.ToString() + "\r\n" + ex.Message);
                }
            }
            else
                textBoxEvents.Text += "Unit is too far away!\r\n";
                //MessageBox.Show("Enota ne more napasti s takšne razdalje.\n\rNapadalec: " + karte[indexNap].lokacija.X + " " + karte[indexNap].lokacija.Y + "\n\rBranilec: " + cilj.X + " " + cilj.Y + "\n\rRazdalja: " + razdaljaMedKartama(karte[indexNap].lokacija, cilj).ToString());

            return indexNap;
        }

        int brisanjeIgralca(Unit karta, int igralec, int indexNap)
        {
            active[igralec] = false;
            for (int i = cards.Count - 1; i >= 0; i--) //brisanje vseh enot tega igralca
            {
                if (cards[i].rasa == igralec + 1)
                {
                    indexNap = delete(i, indexNap);
                }
            }
            return indexNap;
        }

        int igralecUnicen(int indexNap, int indexTrd) //indeks karte, ki napada in trdnjave, ki je uničena, vrne posodobljen index napadalca
        {
            switch (cards[indexTrd].rasa)
            {
                case 1:
                    indexNap = brisanjeIgralca(cards[indexTrd], 0, indexNap); //izbriše igralce in na novo določi index napadalca, saj se je verjetno spremenil
                    textBoxEvents.Text += "White have been destroyed!\r\n";
                    break;
                case 2:
                    indexNap = brisanjeIgralca(cards[indexTrd], 1, indexNap);
                    textBoxEvents.Text += "Black have been destroyed!\r\n";
                    break;
                case 3:
                    indexNap = brisanjeIgralca(cards[indexTrd], 2, indexNap);
                    textBoxEvents.Text += "Red have been destroyed!\r\n";
                    break;
                case 4:
                    indexNap = brisanjeIgralca(cards[indexTrd], 3, indexNap);
                    textBoxEvents.Text += "Green have been destroyed!\r\n";
                    break;
                default:
                    textBoxEvents.Text += "Uničena je bila nepoznana rasa?!\r\n";
                    break;
            }

            int cnt = 0, igralec = 0;
            for (int i = 0; i < 4; i++)
            {
                if (active[i])
                {
                    igralec = i + 1; //če ni več dovolj živih igralcev za nadaljevanje
                    cnt++;
                }
            }

            if (cnt < 2) //če je samo še en igralec
            {
                MessageBox.Show("Game over. Victory goes to " + playerString(igralec) + ". Thanks for playing!");
                gameArea.Enabled = false;
                for (int i = 0; i < 4; i++)
                    AIcontroled[i] = false; //da AI neha igrat
                computer = false;
            }

            return indexNap;
        }

        int vrniIgralca() //vrne številko trenutnega igralca
        {
            return alive[turn - 1] + 1;
        }

        string playerString(int igralec)
        {
            if(1 == igralec)
                return "white";
            else if(2 == igralec)
                return "black";
            else if(3 == igralec)
                return "red";
            else if(4 == igralec)
                return "green";
            else
                return "UFO";
        }

        bool isOccupied(Point lokacija) //preveri, če je lokacija zasedena
        {
            //Debug.WriteLine("Lokacija v funkciji jeZasedena(...) " + lokacija.X + " " + lokacija.Y);
            for (int i = 0; i < cards.Count; i++) //gre skozi vse karte in preverja, če je lokacija zasedena
                if (lokacija.X == cards[i].lokacija.X && lokacija.Y == cards[i].lokacija.Y)
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
            alive = new List<int>(); //seznam v katerem bodo zabeleženi indexi obstoječih igralcev
            for (int i = 0; i < 4; i++)
                if (active[i])
                {
                    stIgralcev++; //vrne trenutno število igralcev
                    alive.Add(i); //0=bela 1=crna 2=rdeca 3=zelena
                }

            if (turn >= stIgralcev)
                turn = 1;
            else
                turn++;

            //MessageBox.Show(naVrsti.ToString() + " " + AIcontroled[naVrsti - 1]);

            if (turn == 1)
            {
                dolociKdoJeNaVrsti(alive[0]);
            }
            else if (turn == 2)
            {
                dolociKdoJeNaVrsti(alive[1]);
            }
            else if (turn == 3)
            {
                dolociKdoJeNaVrsti(alive[2]);
            }
            else if (turn == 4)
            {
                dolociKdoJeNaVrsti(alive[3]);
            }
            else
            {
                MessageBox.Show("Error: Player does not exist :/");
            }

            textBoxStanje.Text = state[turn - 1].ToString(); //nastavi začetno stanje
        }

        void dolociKdoJeNaVrsti(int igralec) //številka igralca, ki je na vrsti
        {
            int prosojnost = 200; //vrednost 0-255, kjer je 0 popolnoma prosojno, 255 pa popolnoma neprosojno
            if (igralec == 0)
            {
                naloziPodatke("../../Properties/Units/white.xml", attributes, data, units);
                labelNaVrsti.Text = "White";
                gameArea.BackColor = Color.FromArgb(prosojnost, Color.LightYellow);
                computer = AIcontroled[0];
            }
            else if (igralec == 1)
            {
                naloziPodatke("../../Properties/Units/black.xml", attributes, data, units);
                labelNaVrsti.Text = "Black";
                gameArea.BackColor = Color.FromArgb(prosojnost, Color.DarkGray);
                computer = AIcontroled[1];    
            }
            else if (igralec == 2)
            {
                naloziPodatke("../../Properties/Units/red.xml", attributes, data, units);
                labelNaVrsti.Text = "Red";
                gameArea.BackColor = Color.FromArgb(prosojnost, Color.Pink);
                computer = AIcontroled[2];
            }
            else if(igralec == 3)
            {
                naloziPodatke("../../Properties/Units/green.xml", attributes, data, units);
                labelNaVrsti.Text = "Green";
                gameArea.BackColor = Color.FromArgb(prosojnost, Color.LightGreen);
                computer = AIcontroled[3];
            }
            else
                MessageBox.Show("Nihče ni več živ?!");
        }

        void inicializacija()
        {
            activeCard = new Point(-2, -2);
            locNov = new Point(-1, -1);
            chosenUnit = false;
            turn = 0;

            gameArea.Enabled = true;
            gameArea.BackColor = Color.Transparent;
            cards.Clear();
            attributes.Clear();
            units.Clear();
            data.Clear();
            alive.Clear();
            shraniZacetne();
            comboBoxEnote.Items.Clear();
            labelFazaIgre.Text = "";

            for (int i = 0; i < 4; i++) //vsa začetna stanja nastavi na default
            {
                state[i] = 0;
                gainedPoints[i] = 0;
            }

            igra();
        }

        void igra()
        {
            //določitev kdo je na vrsti
            zacetekKroga();

            povecavaTock();

            if (computer)
                AI();
        }

        bool validLocation(int rasa) // 1-4
        {
            //MessageBox.Show(locNov.X.ToString() + " == " + (igralnoObmocje.Width - igralnoObmocje.Width / Gl.N * 2).ToString() + "\r\n" + locNov.Y.ToString() + " == " + (igralnoObmocje.Height - igralnoObmocje.Height / Gl.N).ToString());

            if (rasa == 1 &&
                (isUnitLocation(locNov, gameArea.Width / Gl.N, 0) || //[1, 0]
                isUnitLocation(locNov, 0, gameArea.Height / Gl.N) || //[0, 1]
                isUnitLocation(locNov, gameArea.Width / Gl.N, gameArea.Height / Gl.N))) //[1, 1]
                return true;

            else if (rasa == 2 &&
                (isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N * 2, 0) || //[6, 0]
                isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N, gameArea.Height / Gl.N) || //[7, 1]
                isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N * 2, gameArea.Height / Gl.N))) //[6, 1]
                return true;

            else if (rasa == 3 &&
                (isUnitLocation(locNov, gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N) || //[1, 7]
                isUnitLocation(locNov, 0, gameArea.Height - gameArea.Height / Gl.N * 2) || //[0, 6]
                isUnitLocation(locNov, gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N * 2))) //[1, 6]
                return true;

            else if (rasa == 4 &&
                (isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N * 2, gameArea.Height - gameArea.Height / Gl.N) || //[6, 7]
                isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N * 2) ||//[7, 6]
                isUnitLocation(locNov, gameArea.Width - gameArea.Width / Gl.N * 2, gameArea.Height - gameArea.Height / Gl.N * 2))) //[6, 6]
                return true;

            buttonPoklici.Enabled = false;
            return false;
        }

        int currentFaction()
        {
            if (turn == 1)
                return alive[0] + 1;
            else if (turn == 2)
                return alive[1] + 1;
            else if (turn == 3)
                return alive[2] + 1;
            else if (turn == 4)
                return alive[3] + 1;
            else
            {
                MessageBox.Show("Ni veljavne rase?!");
                return 0;
            }
        } //1 - 4

        void markMoves(Unit karta)
        {
            //if (karta.ime == Global.baza) //trdnjave se ne morejo premakniti, lahko le napadajo
              //  return;
            if (karta.premik > 0) //če se karta karta lahko premakne
            {
                int x, y, premik = karta.premik, razdalja;
                Point tarca;

                for (x = 0; x < gameArea.Width; x += size.X)
                {
                    for (y = 0; y < gameArea.Height; y += size.Y)
                    {
                        tarca = new Point(x, y);
                        if((razdalja = razdaljaMedKartama(karta.lokacija, tarca)) <= karta.premik) //premiki
                        {
                            if (!isOccupied(tarca))
                                moveBack.Add(new Unit(tarca));
                        }
                        
                        if (razdalja <= karta.razdaljaNapada)
                        {
                            if (!isOccupied(tarca))
                                attackBack.Add(new Unit(tarca));
                            else
                            {
                                foreach (Unit enota in cards) //pogleda, če je tam prijateljska karta
                                {
                                    if (enota.lokacija.X == x && enota.lokacija.Y == y)
                                    {
                                        if (enota.rasa != currentFaction())
                                            attackBack.Add(new Unit(tarca)); //nariše tarčo na prostorih v dosegu premika, kjer so sovražne enote
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                gameArea.Refresh(); //nariše
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
                MessageBox.Show("Background image not found.");
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

            int stanje = gainedPoints[currentFaction() - 1] + 5; //za koliko se stanje poveča
            gainedPoints[currentFaction() - 1] = 0; //resetira dobljene točke

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
            state[turn - 1] = novoStanje;
        }

        Point lokacijaAI()
        {
            for (locNov.X = 0; locNov.X <= gameArea.Width; locNov.X += size.X)
            {
                for (locNov.Y = 0; locNov.Y <= gameArea.Height; locNov.Y += size.Y)
                {
                    //MessageBox.Show(locNov.X.ToString() + " " + locNov.Y.ToString() + "\r\n" + velikost.X + " " + velikost.Y + "\r\n" + igralnoObmocje.Width + " " + igralnoObmocje.Height);
                    //MessageBox.Show(trenutnaRasa().ToString());
                    if (validLocation(currentFaction()))
                    {
                        if(!isOccupied(locNov))
                            return locNov;
                    }
                }
            }
            locNov.X = -1;
            locNov.Y = -1;

            return locNov; //če ne najde
        }

        bool isUnitLocation(Point karta, int x, int y) //primerja, če se lokacija karte ujema z x in y
        {
            if (karta.X == x && karta.Y == y)
                return true;
            return false;
        }

        int zasedenost(int x1, int y1, int x2, int y2) //koordinate blizu baze, kjer se lahko spawnajo (podatke o srednji točki lahko pridobim iz teh dveh)
        {
            int occupied = 0;
            foreach (Unit unit in cards)
            {
                if ((isUnitLocation(unit.lokacija, x1, y1) ||
                    isUnitLocation(unit.lokacija, x2, y2) ||
                    isUnitLocation(unit.lokacija, x1, y2)) &&
                    unit.rasa != currentFaction())
                    occupied++;
            }
            return occupied;
        }

        void poisciTrdnjavoZaBrisanje()
        {
            for (int i = 0; i < cards.Count; i++) //moralo bi najt v prvih štirih kartah
            {
                if (cards[i].tip == 0 && cards[i].rasa == currentFaction())
                {
                    textBoxEvents.Text += playerString(currentFaction()) + " " + Gl.baza + " je bila zavzeta!\r\n";
                    igralecUnicen(0, i); //prva številka ni relevantna
                    break;
                }
            }
        }

        void koncajPotezo()
        {
            int zasedene = 0; //če pride do tri pomeni, da so vsa mesta pri bazi zasedena s strani sovražnih enot
            if (currentFaction() == 1) //če so vsa mesta ob bazi zasedena, preveri kdo jih zaseda
            {
                zasedene = zasedenost(gameArea.Width / Gl.N, 0, 0, gameArea.Height / Gl.N);
                if(zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();             
            }
            else if(currentFaction() == 2)
            {
                zasedene = zasedenost(gameArea.Width - gameArea.Width / Gl.N * 2, 0, gameArea.Width - gameArea.Width / Gl.N, gameArea.Height / Gl.N);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }
            else if (currentFaction() == 3)
            {
                zasedene = zasedenost(gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N, 0, gameArea.Height - gameArea.Height / Gl.N * 2);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }
            else
            {
                zasedene = zasedenost(gameArea.Width - gameArea.Width / Gl.N * 2, gameArea.Height - gameArea.Height / Gl.N, gameArea.Width - gameArea.Width / Gl.N, gameArea.Height - gameArea.Height / Gl.N * 2);
                if (zasedene >= 3)
                    poisciTrdnjavoZaBrisanje();
            }            

            attributes.Clear();
            data.Clear();
            units.Clear();
            comboBoxEnote.Items.Clear();

            moveBack.Clear(); //počisti premike tudi
            attackBack.Clear();

            foreach (Unit karta in cards)
                karta.premik = karta.maxPremik;
            igra();
            comboBoxEnote.Enabled = true;
            buttonPoklici.Enabled = false;
            textBoxPodatki.Text = "";
        }

        int razdVod(Point prva, Point druga) //vrne vodoravno razdaljo med kartama, oz. negativno vrednost, če je prva nad drugo
        {
            return prva.X / size.X - druga.X / size.X;
        }

        int razdNavp(Point prva, Point druga) //vrne navpično razdaljo med kartama, oz. negativno vrednost, če je prva levo od druge
        {
            return prva.Y / size.Y - druga.Y / size.Y;
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

            if (trenutna.X > 0 && !isOccupied(tmpPoint = new Point(trenutna.X - size.X, trenutna.Y))) //če je cilj na igralni površini in prost
            {
                point = tmpPoint;
                razdalja = razdaljaMedKartama(point, tarca);
            }
            
            if (trenutna.X < size.X * (Gl.N - 1) && !isOccupied(tmpPoint = new Point(trenutna.X + size.X, trenutna.Y))) //če je cilj na igralni površini in prost
                if(razdalja > razdaljaMedKartama(tmpPoint, tarca))
                {
                    point = tmpPoint;
                    razdalja = razdaljaMedKartama(tmpPoint, tarca);
                }
            
            if (trenutna.Y > 0 && !isOccupied(tmpPoint = new Point(trenutna.X, trenutna.Y - size.Y))) //če je cilj na igralni površini in prost
                if (razdalja > razdaljaMedKartama(tmpPoint, tarca))
                {
                    point = tmpPoint;
                    razdalja = razdaljaMedKartama(tmpPoint, tarca);
                }
            
            if (trenutna.Y < size.Y * (Gl.N - 1) && !isOccupied(tmpPoint = new Point(trenutna.X, trenutna.Y + size.Y))) //če je cilj na igralni površini in prost
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
                    cilj.X += size.X;
                else
                {
                    if (razdNavp(cilj, tarca) < 0) //če je zgoraj in mora navzdol (+)
                        cilj.Y += size.Y;
                    else //če je spodaj in mora navzgor (-)
                        cilj.Y -= size.Y;
                }
            }
            else //če je na desni in mora levo (-)
            {
                if (vodoravno)
                    cilj.X -= size.X;
                else
                {
                    if (razdNavp(cilj, tarca) < 0) //če je zgoraj in mora navzdol (+)
                        cilj.Y += size.Y;
                    else //če je spodaj in mora navzgor (-)
                        cilj.Y -= size.Y;
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
            Point trenutna = new Point(cards[i].lokacija.X, cards[i].lokacija.Y); //shrani začetno lokacijo iz katere se potem premika
            Point tmpTarca = new Point(-1, -1); //če ni tarče
            int tocke_premika = cards[i].premik;
            //Debug.WriteLine("Karta: " + i.ToString() + " " + karte[i].maxPremik + " " + karte[i].premik);

            while (tocke_premika > 0) //dokler se še lahko premakne
            {
                tmpTarca.X = tmpTarca.Y = -1;

                foreach(Unit karta in cards) //preveri, če lahko napade kakšno karto
                    if (karta.rasa != currentFaction() && razdaljaMedKartama(trenutna, karta.lokacija) <= cards[i].razdaljaNapada && lahkoPoskoduje(cards[i].napad, karta.oklep))
                    {
                        //Debug.WriteLine("V dosegu karte s koordinatama: " + trenutna.X.ToString() + " " + trenutna.Y.ToString() + " je karta: " + karta.lokacija.X.ToString() + " " + karta.lokacija.Y.ToString());
                        tmpTarca.X = karta.lokacija.X;
                        tmpTarca.Y = karta.lokacija.Y;
                    }

                //if (Math.Abs(karte[i].lokacija.X - tarca.X) + Math.Abs(karte[i].lokacija.Y - tarca.Y) <= (karte[i].razdaljaNapada * (velikost.X + velikost.Y) / 2)) //če lahko doseže tarčo brez premika
                if (razdaljaMedKartama(trenutna, tarca) <= cards[i].razdaljaNapada && cards[i].tip == 4) //ko pride v razdaljo za napad trdnjave in je oven ali katapult
                {
                    while(tocke_premika > 0)
                    {
                        if (isOccupied(tarca)) //če je še kaj tam
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
                        if (isOccupied(tmpTarca)) //če je še kaj tam
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
                    if (cards[i].tip == 0) //če je trdnjava se ne more premaknit
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

#if debug
                    //MessageBox.Show("Točke: " + tocke_premika.ToString() + "\n\rZasedena: " + jeZasedena(tmpTarca).ToString());
                    /*if (tocke_premika == 1 && jeZasedena(tmpTarca))
                        //Debug.WriteLine("Pred spremembo: " + tmpTarca.X + " " + tmpTarca.Y);
                        MessageBox.Show("Pred spremembo: " + tmpTarca.X + " " + tmpTarca.Y);*/
#endif

                    if (isOccupied(tmpTarca)) //če je ciljna lokacija za permik zasedena poskusi najti boljšo
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

                    Debug.WriteLine("Movement points: " + tocke_premika.ToString());
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

            for(int i=0; i<data.Count; i+=Gl.stAt)
            {
                int cena = Int32.Parse(data[i]);
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
                    add(new Unit(currentFaction(),
                                    "AI " + Regex.Replace(data[locEnote + 7], @"[\d-]", ""),
                                    Int32.Parse(data[locEnote]),
                                    data[locEnote + 1],
                                    Int32.Parse(data[locEnote + 2]),
                                    Int32.Parse(data[locEnote + 3]),
                                    Int32.Parse(data[locEnote + 4]),
                                    Int32.Parse(data[locEnote + 5]),
                                    Int32.Parse(data[locEnote + 6]),
                                    Int32.Parse(data[locEnote + 7]),
                                    Image.FromFile("../../Properties/Images/" + Regex.Replace(data[locEnote + 8], @"[\d-]", "") + "/" + data[locEnote + 8] + ".png"),
                                    lokacijaAI()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("AI failed to add unit!\r\n" + ex.Message);
                }
                goto klici_vec_enot;
            }

            List<Unit> trdnjave = new List<Unit>();
            Unit glavnaTarca = new Unit();

            for(int i=0; i<cards.Count; i++)
            {
                if (cards[i].tip == 0 && cards[i].rasa != currentFaction()) //za tarčo vzamejo sovražne trdnjave
                    trdnjave.Add(cards[i]);
                else if (cards[i].rasa == currentFaction()) //napadalci, grejo v napad;
                {
                    if (trdnjave.Count > 1) //če je več sovražnikov
                    {
                        int razdalja = Int32.MaxValue; //največja razdalja
                        foreach (Unit trdnjava in trdnjave)
                            if (razdaljaMedKartama(cards[i].lokacija, trdnjava.lokacija) < razdalja)
                            {
                                razdalja = razdaljaMedKartama(cards[i].lokacija, trdnjava.lokacija);
                                glavnaTarca = trdnjava; //nastavi najbližjo trdnjavo za cilj
                            }
                    }
                    else if (cards[i].tip == 0) //trdnjave se ne morejo premikat in majo posebej
                    {
                        glavnaTarca.lokacija.X = size.X * Gl.N;
                        glavnaTarca.lokacija.Y = size.Y * Gl.N;
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

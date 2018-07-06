using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Igra
{
    class Unit
    {
        public int rasa;
        public string ime;
        public int cena;
        public string text;
        public int premik; //odšteva premik do nič
        public int maxPremik; //premik karte preden se začnejo poteze
        public int napad;
        public int zivljenje;
        public int maxZivljenje;
        public int oklep;
        public int razdaljaNapada;
        public int tip; //0 = trdnjava, 1 = pehota, 2 = lokostrelec, 3 = konjenik, 4 = artilerija
        public Image slika;
        public Point lokacija; //zgornji levi kot

        public Unit() //toliko, da je deklarirana
        { }

        public Unit(int Rasa, string Ime, int Cena, string Text, int Premik, int Napad, int Zivljenje, int Oklep, int RazdaljaNapada, int Tip, Image Slika, Point Lokacija)
        {
            rasa = Rasa;
            ime = Ime;
            cena = Cena;
            text = Text;
            premik = Premik;
            napad = Napad;
            zivljenje = Zivljenje;
            oklep = Oklep;
            razdaljaNapada = RazdaljaNapada;
            tip = Tip;
            slika = Slika;
            lokacija = new Point(Lokacija.X, Lokacija.Y);
        }

        /*public Karta(Karta karta) //kopiranje karte
        {
            rasa = karta.rasa;
            ime = karta.ime;
            cena = karta.cena;
            text = karta.text;
            premik = karta.premik;
            maxPremik = karta.maxPremik;
            napad = karta.napad;
            zivljenje = karta.zivljenje;
            maxZivljenje = karta.maxZivljenje;
            oklep = karta.oklep;
            razdaljaNapada = karta.razdaljaNapada;
            slika = karta.slika;
            lokacija = new Point(karta.lokacija.X, karta.lokacija.Y);
        }*/

        public Unit(Point Lokacija) //za barvanje premikov in podobno
        {
            lokacija = new Point(Lokacija.X, Lokacija.Y);
        }
    }

}

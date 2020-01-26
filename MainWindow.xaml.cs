using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>

    public class Parts {
        public UIElement UiElemente { get; set; }
        public Point Position { get; set; }
        public bool Kopf { get; set; }
    }
    public partial class MainWindow : Window {
        const int TeilGroesse = 25;
        const int StartLaenge = 4;
        const int StartGeschw = 400;
        const int MaxGeschw = 100;
        int laenge;
        int punkte = 0;
        SolidColorBrush PartsFarbe = Brushes.CadetBlue;
        SolidColorBrush KopfFarbe = Brushes.Blue;
        SolidColorBrush FutterFarbe = Brushes.Red;
        UIElement futter = null;
        Random zufall = new Random();
        DispatcherTimer spielTimer = new DispatcherTimer();
        enum Richtung { Links, Rechts, Hoch, Runter};
        Richtung richtung = Richtung.Rechts;
        List<Parts> snakeParts = new List<Parts>();
        
        public MainWindow() {
            InitializeComponent();
            spielTimer.Tick += SpielTimerEvent;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            ZeichneSpielfeld();
            NeuesSpiel();
        }
        private void ZeichneSpielfeld() {
            bool Fertig = false;
            int folgX = 0, folgY = 0;
            int zeilenZaehler = 0;
            bool folgIstGerade = false;

            while(Fertig == false) {
                Rectangle eck = new Rectangle {
                    Width = TeilGroesse,
                    Height = TeilGroesse,
                    Fill = folgIstGerade ? Brushes.LightGray : Brushes.White
                    //if(folgendIstGerade = true) {
                    //	Fill = Brushes.Gray;
                    //}else{ 
                    //	Fill =Brushes.LightGray;
                    //}
                };
                Spielfeld.Children.Add(eck);
                Canvas.SetTop(eck, folgY);
                Canvas.SetLeft(eck, folgX);

                folgIstGerade = !folgIstGerade;
                folgX += TeilGroesse;
                if(folgX >= Spielfeld.ActualWidth) {
                    folgX = 0;
                    folgY += TeilGroesse;
                    zeilenZaehler++;
                    folgIstGerade = (zeilenZaehler % 2 != 0);
                }
                if(folgY >= Spielfeld.ActualHeight) {
                    Fertig = true;
                }
            }
        }
        void ZeichneSnake() {
            foreach(Parts parts in snakeParts) {
                if(parts.UiElemente == null) {
                    parts.UiElemente = new Rectangle() {
                        Width = TeilGroesse,
                        Height = TeilGroesse,
                        Fill = parts.Kopf ? KopfFarbe : PartsFarbe
                    };
                    Spielfeld.Children.Add(parts.UiElemente);
                    Canvas.SetTop(parts.UiElemente, parts.Position.Y);
                    Canvas.SetLeft(parts.UiElemente, parts.Position.X);
                }
            }
        }
        void Bewegen() {
            while(snakeParts.Count >= laenge) {
                Spielfeld.Children.Remove(snakeParts[0].UiElemente);
                snakeParts.RemoveAt(0);
            }
            foreach(Parts parts in snakeParts) {
                (parts.UiElemente as Rectangle).Fill = PartsFarbe;
                parts.Kopf = false;
            }
            Parts kopf = snakeParts[snakeParts.Count - 1];
            double folgX = kopf.Position.X;
            double folgY = kopf.Position.Y;
            switch(richtung) {
                case Richtung.Links:
                    folgX -= TeilGroesse;
                    break;
                case Richtung.Rechts:
                    folgX += TeilGroesse;
                    break;
                case Richtung.Runter:
                    folgY += TeilGroesse;
                    break;
                case Richtung.Hoch:
                    folgY -= TeilGroesse;
                    break;
            }
            snakeParts.Add(new Parts() {
                Position = new Point(folgX, folgY),
                Kopf = true
            });
            ZeichneSnake();
            KollisionsAbfrage();
        }
        void SpielTimerEvent(object sender, EventArgs e) {
            Bewegen();
        }
        void NeuesSpiel() {
            foreach(Parts parts in snakeParts) {
                if(parts.UiElemente != null) {
                    Spielfeld.Children.Remove(parts.UiElemente);
                }
            }
            snakeParts.Clear();
            if(futter != null) {
                Spielfeld.Children.Remove(futter);
            }
            punkte = 0;
            laenge = StartLaenge;
            richtung = Richtung.Rechts;
            snakeParts.Add(new Parts() {
                Position = new Point(TeilGroesse * 5, TeilGroesse * 5)
            });
            spielTimer.Interval = TimeSpan.FromMilliseconds(StartGeschw);
            StatusUpdate();
            ZeichneSnake();
            ZeichneFutter();
            spielTimer.IsEnabled = true;
        }
        private Point FutterPosition() {
            int maxX = (int)(Spielfeld.ActualWidth / TeilGroesse);
            int maxY = (int)(Spielfeld.ActualHeight / TeilGroesse);
            int futterX = zufall.Next(0, maxX) * TeilGroesse;
            int futterY = zufall.Next(0, maxY) * TeilGroesse;

            foreach(Parts parts in snakeParts) {
                if(parts.Position.X == futterX && parts.Position.Y == futterY) {
                    return FutterPosition();
                }
            }
            return new Point(futterX, futterY);
        }
        void ZeichneFutter() {
            Point futterPosition = FutterPosition();
            futter = new Ellipse() {
                Width = TeilGroesse,
                Height = TeilGroesse,
                Fill = FutterFarbe
            };
            Spielfeld.Children.Add(futter);
            Canvas.SetTop(futter, futterPosition.Y);
            Canvas.SetLeft(futter, futterPosition.X);
        }
        private void WindowKeyDown(object sender, KeyEventArgs e) {
            Richtung originalRichtung = richtung;
            switch(e.Key) {
                case Key.Up:
                    if(richtung != Richtung.Runter) {
                        richtung = Richtung.Hoch;
                    }break;
                case Key.Down:
                    if(richtung != Richtung.Hoch) {
                        richtung = Richtung.Runter;
                    }break;
                case Key.Right:
                    if(richtung != Richtung.Links) {
                        richtung = Richtung.Rechts;
                    }break;
                case Key.Left:
                    if(richtung != Richtung.Rechts) {
                        richtung = Richtung.Links;
                    }break;
                case Key.Enter:
                    NeuesSpiel();
                    break;
            }
            if(richtung != originalRichtung) {
                Bewegen();
            }
        }
        void KollisionsAbfrage() {
            Parts kopf = snakeParts[snakeParts.Count - 1];

            if(kopf.Position.X == Canvas.GetLeft(futter) && kopf.Position.Y == Canvas.GetTop(futter)){
                Fressen();
                return;
            }
            if(kopf.Position.X < 0 || kopf.Position.X >= Spielfeld.ActualWidth || kopf.Position.Y < 0 || kopf.Position.Y >= Spielfeld.ActualHeight) {
                Spielende();
            }
            foreach(Parts parts in snakeParts.Take(snakeParts.Count - 1)) {
                if(kopf.Position.X == parts.Position.X && kopf.Position.Y == parts.Position.Y) {
                    Spielende();
                }
            }
        }
        void Fressen() {
            laenge++;
            punkte++;
            int timerInterval = Math.Max(MaxGeschw, (int)spielTimer.Interval.TotalMilliseconds - (punkte * 2));
            spielTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            Spielfeld.Children.Remove(futter);
            ZeichneFutter();
            StatusUpdate();
        }
        void StatusUpdate() {
            this.Title = $"Snake - Punkte: {punkte} - Geschwindigkeit: {spielTimer.Interval.TotalMilliseconds}";
        }
        void Spielende() {
            spielTimer.IsEnabled = false;
            MessageBox.Show($"Du bist Gestorben :'(\n\nPunkte:{punkte}\nFür ein neues Spiel drücke Enter");
        }
    }
}

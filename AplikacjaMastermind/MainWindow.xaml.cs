using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AplikacjaMastermind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Deklaracja tablicy kolorów
        private readonly Color[] pinColors =
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Orange,
            Colors.Indigo,
            Colors.Yellow,
            Colors.Purple
        };

        private readonly Color[] feedbackCBPins = { Colors.Black, Colors.White };

        private readonly Dictionary<Color, string> polishColors = new Dictionary<Color, string>
        {
            {Colors.Red, "Czerwony"},
            {Colors.Green, "Zielony"},
            {Colors.Blue, "Niebieski"},
            {Colors.Orange, "Pomarańczowy"},
            {Colors.Indigo, "Indigo"},
            {Colors.Yellow, "Żółty"},
            {Colors.Purple, "Fioletowy"},
        };

        //Zmienna przechowująca ukryte i wylosowane kolory
        private Color[] secretPins;

        private int currentRow = 0;



        //MAIN
        public MainWindow()
        {
            InitializeComponent();
            MainBoard.Children.Clear();
        }

        //Sprawdzanie pinów
        private void CheckGame_Click(object sender, RoutedEventArgs e)
        {
            var activeRow = MainBoard.Children.OfType<StackPanel>()
                .FirstOrDefault(row => row.Children.OfType<StackPanel>()
                .FirstOrDefault()?.Children.OfType<Ellipse>()
                .FirstOrDefault( e => e.IsEnabled) != null);

            if (activeRow == null)
            {
                MessageBox.Show("Brak aktywnego wiersza", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var mainCirclePanel = activeRow.Children.OfType<StackPanel>().First();
            var guessColors = mainCirclePanel.Children.OfType<Ellipse>().Select(e => (e.Fill as SolidColorBrush)?.Color ?? Colors.Gray)
                .ToArray();


            string kolorki = "";
            foreach (var color in guessColors)
            {
                kolorki += polishColors.ContainsKey(color) ? polishColors[color] : color.ToString();
            }

            //MessageBox.Show($"W: {kolorki}");


            foreach (var color in guessColors)
            {
                if (color == Colors.Gray)
                {
                    MessageBox.Show("Nie pokolorowałeś wszytkich poinów", "Błąd pinów", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var (correctPosition, correctColor) = checkPins(guessColors, secretPins);
            var feedbackPins = activeRow.Children.OfType<Grid>().First();

            foreach (var child in feedbackPins.Children.OfType<Ellipse>())
            {
                child.Fill = Brushes.Gray;
            }

            var feedbackCircles = feedbackPins.Children.OfType<Ellipse>().ToList();

            int feedbackIndex = 0;

            for (int i = 0; i < correctPosition && feedbackIndex < 4; i++, feedbackIndex++)
            {
                feedbackCircles[feedbackIndex].Fill = new SolidColorBrush(feedbackCBPins[0]);
            }

            for (int i = 0; i < correctColor && feedbackIndex < 4; i++, feedbackIndex++)
            {
                feedbackCircles[feedbackIndex].Fill = new SolidColorBrush(feedbackCBPins[1]);
            }

            foreach (var ellipse in mainCirclePanel.Children.OfType<Ellipse>())
            {
                ellipse.IsEnabled = false;
            }

            var nextRow = MainBoard.Children.OfType<StackPanel>().FirstOrDefault( row => (int)row.Tag > (int)activeRow.Tag );

            if (nextRow != null )
            {
                var nextMainCirclePanel = nextRow.Children.OfType<StackPanel>().First();
                foreach (var ellipse in nextMainCirclePanel.Children.OfType<Ellipse>())
                {
                    ellipse.IsEnabled = true;
                }
            }

            if (correctPosition == 4)
            {
                MessageBox.Show("Wygrałeś", "Wygrana", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (nextRow == null)
            {
                string kolory = "";
                string k = "";
                foreach (var color in secretPins)
                {
                    k = polishColors.ContainsKey(color) ? polishColors[color] : color.ToString();
                    kolory += k + " ";
                }
                MessageBox.Show($"Przegrałeś, wylosowane kolory to\n{kolory}", "Przegrana", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        private (int correctPositions, int correctColors) checkPins(Color[] guess, Color[] secret)
        {
            int correctPositions = 0;
            int correctColors = 0;

            var guessMatched = new bool[4];
            var secretMatched = new bool[4];

            for (int i = 0; i < 4; i++)
            {
                if (guess[i] == secret[i])
                {
                    correctPositions++;
                    guessMatched[i] = true;
                    secretMatched[i] = true;
                }
            }

            for (int i = 0;i < 4;i++)
            {
                if (guessMatched[i]) continue;
                for (int j = 0; j < 4; j++)
                {
                    if (secretMatched[j]) continue;
                    if (guess[i] == secret[j])
                    {
                        correctColors++;
                        secretMatched[j] = true;
                        break;
                    }
                }
            }
            return (correctPositions, correctColors);


        }

        //Nowa gra
        private void MenuNew(object sender, RoutedEventArgs e)
        {
            var PinSelect = MessageBox.Show("Chcesz losować piny z powtórzeniami?", "Wybór", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (PinSelect == MessageBoxResult.Yes)
            {
                secretPinsGenerator();
            }
            else
            {
                secretPinsGenerator2();
            }
            newBoard();
        }

        //Nowa plansza do gry
        private void newBoard()
        {
            MainBoard.Children.Clear();

            for (int i = 0; i < 10; i++)
            {
                StackPanel rowPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5),
                };
                rowPanel.Tag = i;

                StackPanel mainCirclePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };
                //Pętla do 4 pinów w szeregu
                for (int j = 0; j < 4; j++)
                {
                    mainCirclePanel.Children.Add(createCircle(30, i == 0, true));
                }
                

                //Grid na piny z odpowiedzią
                Grid FeedbackGrid = new Grid
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                FeedbackGrid.RowDefinitions.Add(new RowDefinition());
                FeedbackGrid.RowDefinitions.Add(new RowDefinition());
                FeedbackGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FeedbackGrid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        Ellipse FeedbackCircle = createCircle(20, true, false);
                        Grid.SetRow(FeedbackCircle, x);
                        Grid.SetColumn(FeedbackCircle, y);
                        FeedbackGrid.Children.Add(FeedbackCircle);
                    }
                }

                //Dodanie do MainBoard
                rowPanel.Children.Add(mainCirclePanel);
                rowPanel.Children.Add(FeedbackGrid);
                MainBoard.Children.Add(rowPanel);
            }
        }

        //Wybór kolorów pinów
        private void openColorPicker(Ellipse circle, Color[] openColors)
        {
            if (!circle.IsEnabled || circle == null) return;

            ContextMenu colorMenu = new ContextMenu();
            foreach (Color color in openColors)
            {
                MenuItem item = new MenuItem
                {
                    Header = polishColors.ContainsKey(color) ? polishColors[color] : color.ToString(),
                    Background = new SolidColorBrush(color)
                };
                //nasłuch na klikniecie z wyborem koloru i zmianą koloru
                item.Click += (s, e) => circle.Fill = new SolidColorBrush(color);
                colorMenu.Items.Add(item);
            }

            circle.ContextMenu = colorMenu;
            colorMenu.IsOpen = true;

        }

        //Tworzenie pinów
        private Ellipse createCircle(int size, bool isVisible, bool isMainCircle)
        {
            Ellipse circle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Colors.LightGray),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Margin = new Thickness(5),
                IsEnabled = isVisible
            };

            //Wywołanie click na mysz by dodać kolor
            if (isMainCircle)
            {
                circle.MouseLeftButtonDown += (s, e) => openColorPicker(s as Ellipse, pinColors);
            }
            else
            {
                circle.MouseLeftButtonDown += (s, e) => openColorPicker(s as Ellipse, new Color[] { Colors.White, Colors.Black });
            }
            return circle;
        }

        //Metoda generująca piny z powtórzeniami
        private void secretPinsGenerator()
        {
            Random random = new Random();
            secretPins = Enumerable.Range(0, 4).Select(_ => pinColors[random.Next(pinColors.Length)]).ToArray();

            string kolorki = "";
            foreach (var color in secretPins)
            {
                kolorki += polishColors.ContainsKey(color) ? polishColors[color] : color.ToString();
            }

            losowane.Text = ($"W:  {kolorki}  ");
        }

        //Metoda generująca piny bez powtórzeń
        private void secretPinsGenerator2()
        {
            Random random = new Random();
            secretPins = pinColors.OrderBy(_ => random.Next()).Take(4).ToArray();
            string kolorki = "";
            foreach (var color in secretPins)
            {
                kolorki += polishColors.ContainsKey(color) ? polishColors[color] : color.ToString();
            }

            losowane.Text = ($"W: {kolorki}");
        }
        //Zakończenie programu poprzez X
        protected override void OnClosing(CancelEventArgs e)
        {
            var odp = MessageBox.Show("Czy chcesz zakończyć grę?", "Zakończ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (odp != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        //Zakończenie programu poprzez menu wywołując nadpisaną metodę OnClosing ^^
        private void MenuEnd(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Rules(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1.Cel gry:\nCelem gry jest odgadnięcie tajnego kodu wygenerowanego przez system. " +
                "Kod składa się z czterech kolorów, a każdy kolor może wystąpić tylko raz.\r\n\r\n" +
                "2.Rozpoczęcie gry:\nNa początku rozgrywki system losowo generuje ukryty kod.\r\n\r\n" +
                "3.Tworzenie propozycji:\nGracz w każdej rundzie podaje swoją propozycję kodu, składającą się z czterech kolorów.\r\n\r\n" +
                "4.Ocena próby:\nPo każdej próbie system informuje gracza:\r\nIle kolorów zostało trafionych na właściwym miejscu.\r\n" +
                "Ile kolorów występuje w kodzie, ale są na niewłaściwym miejscu.\r\n\r\n" +
                "5.Ograniczenie prób:\nGracz ma określoną liczbę prób (10), aby odgadnąć kod.\r\n\r\n" +
                "6.Wygrana i przegrana:\r\nGracz wygrywa, jeśli poprawnie odgadnie kod przed wykorzystaniem wszystkich prób.\r\n" +
                "Jeśli gracz nie odgadnie kodu po wykorzystaniu wszystkich prób, przegrywa.", "Zasady", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MASTERMIND \n Wersja 1.0 \n 2025 © Oliwier Waldoch \n oliwier.wald@gmail.com", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
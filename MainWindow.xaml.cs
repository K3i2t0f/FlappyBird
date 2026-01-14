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
using System.Windows.Threading;

namespace FlappyBird
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();

        double gravity = 1.5;
        double birdVelocity = 0;
        double pipeSpeed = 4;

        Rectangle[] topPipes;
        Rectangle[] bottomPipes;

        Random rnd = new Random();

        bool gameStarted = false;
        bool isGameOver = false;

        public MainWindow()
        {
            InitializeComponent();

            topPipes = new Rectangle[] { PipeTop1, PipeTop2, PipeTop3 };
            bottomPipes = new Rectangle[] { PipeBottom1, PipeBottom2, PipeBottom3 };

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;

            ResetGame();

            // biztos fókusz a Canvas-ra, hogy a KeyDown működjön
            this.Loaded += (_, __) => GameCanvas.Focus();
        }

        private void ResetGame()
        {
            gameTimer.Stop();

            gameStarted = false;
            isGameOver = false;
            birdVelocity = 0;

            Canvas.SetTop(Bird, 217);

            RestartButton.Visibility = Visibility.Visible; // gomb mindig látszik, ezzel indítunk

            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], 800 + i * 250);
                Canvas.SetLeft(bottomPipes[i], 800 + i * 250);
                ResetPipe(i);
            }
        }

        private void StartGame()
        {
            gameStarted = true;
            isGameOver = false;
            birdVelocity = 0; // induláskor nem ugrik
            gameTimer.Start();

            RestartButton.Visibility = Visibility.Collapsed; // elindításkor eltűnik a gomb
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            // Madár gravitáció
            birdVelocity += gravity;
            Canvas.SetTop(Bird, Canvas.GetTop(Bird) + birdVelocity);

            // Csövek mozgatása
            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], Canvas.GetLeft(topPipes[i]) - pipeSpeed);
                Canvas.SetLeft(bottomPipes[i], Canvas.GetLeft(bottomPipes[i]) - pipeSpeed);

                if (Canvas.GetLeft(topPipes[i]) < -100)
                {
                    Canvas.SetLeft(topPipes[i], 800);
                    Canvas.SetLeft(bottomPipes[i], 800);
                    ResetPipe(i);
                }

                CheckCollision(topPipes[i]);
                CheckCollision(bottomPipes[i]);
            }

            // Madár képernyőn kívül
            if (Canvas.GetTop(Bird) < 0 || Canvas.GetTop(Bird) + Bird.Height > 450)
                GameOver();
        }

        private void ResetPipe(int index)
        {
            int gap = rnd.Next(130, 180);
            int topHeight = rnd.Next(80, 200);

            topPipes[index].Height = topHeight;
            bottomPipes[index].Height = 450 - (topHeight + gap);

            // Felső cső a Canvas tetején
            Canvas.SetTop(topPipes[index], 0);
            Canvas.SetTop(bottomPipes[index], topHeight + gap);
        }

        private void CheckCollision(Rectangle pipe)
        {
            if (isGameOver)
                return;

            Rect birdRect = new Rect(
                Canvas.GetLeft(Bird),
                Canvas.GetTop(Bird),
                Bird.Width,
                Bird.Height);

            Rect pipeRect = new Rect(
                Canvas.GetLeft(pipe),
                Canvas.GetTop(pipe),
                pipe.Width,
                pipe.Height);

            if (birdRect.IntersectsWith(pipeRect))
                GameOver();
        }

        private void GameOver()
        {
            if (isGameOver)
                return;

            isGameOver = true;
            gameStarted = false;
            gameTimer.Stop();

            RestartButton.Visibility = Visibility.Visible; // halál után újra látszik a gomb
        }

        // SPACE ugrás közben
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameStarted)
                return;

            if (e.Key == Key.Space)
            {
                birdVelocity = -10; // ugrás
            }
        }

        // Újraindító gomb
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            StartGame();
        }
    }
}
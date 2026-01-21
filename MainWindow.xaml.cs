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

        bool[] pipeScored = new bool[3];

        Random rnd = new Random();

        bool gameStarted = false;
        bool isGameOver = false;
        bool waitingForRestart = false;

        int score = 0;

        public MainWindow()
        {
            InitializeComponent();

            topPipes = new Rectangle[]
            {
                PipeTop1, PipeTop2, PipeTop3
            };

            bottomPipes = new Rectangle[]
            {
                PipeBottom1, PipeBottom2, PipeBottom3
            };

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;

            ResetGame();

            Loaded += (_, __) => GameCanvas.Focus();
        }

        // TELJES RESET (indításkor és restartnál)
        private void ResetGame()
        {
            gameTimer.Stop();

            gameStarted = false;
            isGameOver = false;
            waitingForRestart = false;

            birdVelocity = 0;

            score = 0;
            ScoreText.Text = "0";

            Canvas.SetTop(Bird, 217);

            GameOverPanel.Visibility = Visibility.Collapsed;

            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], 800 + i * 250);
                Canvas.SetLeft(bottomPipes[i], 800 + i * 250);

                pipeScored[i] = false;
                ResetPipe(i);
            }
        }

        // Játék elindítása
        private void StartGame()
        {
            gameStarted = true;
            isGameOver = false;
            birdVelocity = 0;
            gameTimer.Start();
        }

        // Fő játék ciklus
        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            // Madár gravitáció
            birdVelocity += gravity;
            Canvas.SetTop(Bird, Canvas.GetTop(Bird) + birdVelocity);

            for (int i = 0; i < 3; i++)
            {
                // Csövek mozgatása
                Canvas.SetLeft(topPipes[i], Canvas.GetLeft(topPipes[i]) - pipeSpeed);
                Canvas.SetLeft(bottomPipes[i], Canvas.GetLeft(bottomPipes[i]) - pipeSpeed);

                // Cső visszarakása jobbra
                if (Canvas.GetLeft(topPipes[i]) < -100)
                {
                    double maxX = 0;
                    for (int j = 0; j < 3; j++)
                        maxX = Math.Max(maxX, Canvas.GetLeft(topPipes[j]));

                    Canvas.SetLeft(topPipes[i], maxX + 250);
                    Canvas.SetLeft(bottomPipes[i], maxX + 250);

                    pipeScored[i] = false;
                    ResetPipe(i);
                }

                // Pontozás
                if (!pipeScored[i] &&
                    Canvas.GetLeft(topPipes[i]) + topPipes[i].Width <
                    Canvas.GetLeft(Bird))
                {
                    score++;
                    ScoreText.Text = score.ToString();
                    pipeScored[i] = true;
                }

                // Ütközés
                CheckCollision(topPipes[i]);
                CheckCollision(bottomPipes[i]);
            }

            // Fent vagy lent kiesés
            if (Canvas.GetTop(Bird) < 0 ||
                Canvas.GetTop(Bird) + Bird.Height > 450)
            {
                GameOver();
            }
        }

        // Csövek magasság + rés
        private void ResetPipe(int index)
        {
            int gap = rnd.Next(130, 180);
            int topHeight = rnd.Next(80, 200);

            topPipes[index].Height = topHeight;
            bottomPipes[index].Height = 450 - (topHeight + gap);

            Canvas.SetTop(topPipes[index], 0);
            Canvas.SetTop(bottomPipes[index], topHeight + gap);
        }

        // Ütközés ellenőrzés
        private void CheckCollision(Rectangle pipe)
        {
            if (isGameOver)
                return;

            Rect birdRect = new Rect(
                Canvas.GetLeft(Bird)+14,
                Canvas.GetTop(Bird)+14,
                Bird.Width-28,
                Bird.Height-28);

            Rect pipeRect = new Rect(
                Canvas.GetLeft(pipe),
                Canvas.GetTop(pipe),
                pipe.Width,
                pipe.Height);

            if (birdRect.IntersectsWith(pipeRect))
                GameOver();
        }

        // Játék vége
        private void GameOver()
        {
            if (isGameOver)
                return;

            isGameOver = true;
            gameStarted = false;
            waitingForRestart = true;

            gameTimer.Stop();

            FinalScoreText.Text = $"Pontszám: {score}";
            GameOverPanel.Visibility = Visibility.Visible;
        }

        // SPACE kezelés
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Halál után SPACE teljesen tiltva
            if (waitingForRestart)
                return;

            if (e.Key != Key.Space)
                return;

            if (!gameStarted)
            {
                StartGame();
                birdVelocity = -10;
                return;
            }

            birdVelocity = -10;
        }

        // Restart gomb
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }
    }
}
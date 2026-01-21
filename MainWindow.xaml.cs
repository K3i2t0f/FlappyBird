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

        bool gameLoaded = false;
        bool gameStarted = false;
        bool isGameOver = false;

        int score = 0;
        const double PIPE_FULL_HEIGHT = 354;

        public MainWindow()
        {
            InitializeComponent();

            topPipes = new[] { PipeTop1, PipeTop2, PipeTop3 };
            bottomPipes = new[] { PipeBottom1, PipeBottom2, PipeBottom3 };

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Collapsed;
            DifficultyMenu.Visibility = Visibility.Visible;
        }

        private void EasyButton_Click(object sender, RoutedEventArgs e)
        {
            MenuOverlay.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;
            StartEasyGame();
        }

        private void StartEasyGame()
        {
            ResetGame();

            gameLoaded = true;
            gameStarted = false;

            ScoreText.Visibility = Visibility.Visible;
            Bird.Visibility = Visibility.Visible;

            foreach (var p in topPipes) p.Visibility = Visibility.Visible;
            foreach (var p in bottomPipes) p.Visibility = Visibility.Visible;

            GameCanvas.Focus();
        }

        private void ResetGame()
        {
            gameTimer.Stop();
            gameStarted = false;
            isGameOver = false;

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

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameStarted || isGameOver) return;

            birdVelocity += gravity;
            Canvas.SetTop(Bird, Canvas.GetTop(Bird) + birdVelocity);

            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], Canvas.GetLeft(topPipes[i]) - pipeSpeed);
                Canvas.SetLeft(bottomPipes[i], Canvas.GetLeft(bottomPipes[i]) - pipeSpeed);

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

                if (!pipeScored[i] &&
                    Canvas.GetLeft(topPipes[i]) + topPipes[i].Width <
                    Canvas.GetLeft(Bird))
                {
                    score++;
                    ScoreText.Text = score.ToString();
                    pipeScored[i] = true;
                }

                CheckCollision(topPipes[i]);
                CheckCollision(bottomPipes[i]);
            }

            if (Canvas.GetTop(Bird) < 0 || Canvas.GetTop(Bird) + Bird.Height > 450)
                GameOver();
        }

        private void ResetPipe(int index)
        {
            int gap = rnd.Next(130, 180);
            double gapCenterY = rnd.Next(170, 280);

            double gapTop = gapCenterY - gap / 2;
            double gapBottom = gapCenterY + gap / 2;

            topPipes[index].Height = PIPE_FULL_HEIGHT;
            Canvas.SetTop(topPipes[index], gapTop - PIPE_FULL_HEIGHT);

            bottomPipes[index].Height = 450 - gapBottom;
            Canvas.SetTop(bottomPipes[index], gapBottom);
        }

        private void CheckCollision(Rectangle pipe)
        {
            Rect birdRect = new Rect(
                Canvas.GetLeft(Bird) + 14,
                Canvas.GetTop(Bird) + 14,
                Bird.Width - 28,
                Bird.Height - 28);

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
            isGameOver = true;
            gameStarted = false;
            gameTimer.Stop();

            FinalScoreText.Text = $"Pontszám: {score}";
            GameOverPanel.Visibility = Visibility.Visible;
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameLoaded || isGameOver) return;
            if (e.Key != Key.Space) return;

            if (!gameStarted)
            {
                gameStarted = true;
                gameTimer.Start();
            }

            birdVelocity = -10;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            gameLoaded = false;
            gameStarted = false;
            isGameOver = false;

            BackButton.Visibility = Visibility.Collapsed;

            ScoreText.Visibility = Visibility.Collapsed;
            Bird.Visibility = Visibility.Collapsed;
            GameOverPanel.Visibility = Visibility.Collapsed;

            foreach (var p in topPipes) p.Visibility = Visibility.Collapsed;
            foreach (var p in bottomPipes) p.Visibility = Visibility.Collapsed;

            MenuOverlay.Visibility = Visibility.Visible;
            MainMenu.Visibility = Visibility.Collapsed;
            DifficultyMenu.Visibility = Visibility.Visible;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }
    }
}
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

        double easyPipeSpeed = 4;
        double mediumPipeSpeed = 6;
        double hardPipeSpeed = 8;
        double pipeSpeed;

        double normalJump = -10;
        double heavyJump = -7;

        bool mediumMode = false;
        bool hardMode = false;
        bool gameStarted = false;
        bool isGameOver = false;

        Rectangle[] topPipes;
        Rectangle[] bottomPipes;
        Rectangle[] rainDrops;

        bool[] pipeScored = new bool[3];
        Random rnd = new Random();

        int score = 0;
        const double PIPE_FULL_HEIGHT = 354;

        public MainWindow()
        {
            InitializeComponent();

            topPipes = new[] { PipeTop1, PipeTop2, PipeTop3 };
            bottomPipes = new[] { PipeBottom1, PipeBottom2, PipeBottom3 };
            rainDrops = new[] { Rain1, Rain2, Rain3, Rain4, Rain5 };

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
            mediumMode = false;
            hardMode = false;
            pipeSpeed = easyPipeSpeed;
            StartGame();
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            mediumMode = true;
            hardMode = false;
            pipeSpeed = mediumPipeSpeed;
            StartGame();
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            mediumMode = true;
            hardMode = true;
            pipeSpeed = hardPipeSpeed;
            StartGame();
        }

        private void StartGame()
        {
            MenuOverlay.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;

            ResetGame();

            Bird.Visibility = Visibility.Visible;
            ScoreText.Visibility = Visibility.Visible;

            foreach (var p in topPipes) p.Visibility = Visibility.Visible;
            foreach (var p in bottomPipes) p.Visibility = Visibility.Visible;

            foreach (var r in rainDrops)
            {
                r.Visibility = mediumMode ? Visibility.Visible : Visibility.Collapsed;
                Canvas.SetTop(r, rnd.Next(-600, 0));
                Canvas.SetLeft(r, rnd.Next(0, 800));
            }

            // 🌫️ KÖD: Hard módban mindig aktív
            FogOverlay.Visibility = hardMode ? Visibility.Visible : Visibility.Collapsed;

            GameCanvas.Focus();
        }

        private void ResetGame()
        {
            gameTimer.Stop();
            birdVelocity = 0;
            score = 0;
            ScoreText.Text = "0";
            gameStarted = false;
            isGameOver = false;

            Canvas.SetTop(Bird, 217);
            GameOverPanel.Visibility = Visibility.Collapsed;
            FogOverlay.Visibility = Visibility.Collapsed;

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

                if (Canvas.GetLeft(topPipes[i]) < -120)
                {
                    double maxX = Math.Max(Canvas.GetLeft(topPipes[0]),
                                 Math.Max(Canvas.GetLeft(topPipes[1]), Canvas.GetLeft(topPipes[2])));

                    Canvas.SetLeft(topPipes[i], maxX + 250);
                    Canvas.SetLeft(bottomPipes[i], maxX + 250);
                    pipeScored[i] = false;
                    ResetPipe(i);
                }

                if (!pipeScored[i] &&
                    Canvas.GetLeft(topPipes[i]) + topPipes[i].Width < Canvas.GetLeft(Bird))
                {
                    score++;
                    ScoreText.Text = score.ToString();
                    pipeScored[i] = true;
                }

                CheckCollision(topPipes[i]);
                CheckCollision(bottomPipes[i]);
            }

            if (mediumMode)
                UpdateRain();

            if (Canvas.GetTop(Bird) < 0 || Canvas.GetTop(Bird) + Bird.Height > 450)
                GameOver();
        }

        private void UpdateRain()
        {
            foreach (var r in rainDrops)
            {
                Canvas.SetTop(r, Canvas.GetTop(r) + 14);

                if (Canvas.GetTop(r) > 450)
                {
                    Canvas.SetTop(r, rnd.Next(-400, 0));
                    Canvas.SetLeft(r, rnd.Next(0, 800));
                }
            }
        }

        private void ResetPipe(int index)
        {
            int gap = rnd.Next(120, 160);
            double centerY = rnd.Next(160, 290);

            topPipes[index].Height = PIPE_FULL_HEIGHT;
            Canvas.SetTop(topPipes[index], centerY - gap / 2 - PIPE_FULL_HEIGHT);

            bottomPipes[index].Height = 450 - (centerY + gap / 2);
            Canvas.SetTop(bottomPipes[index], centerY + gap / 2);
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

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver || e.Key != Key.Space) return;

            if (!gameStarted)
            {
                gameStarted = true;
                gameTimer.Start();
            }

            birdVelocity = mediumMode ? heavyJump : normalJump;
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();
            FinalScoreText.Text = $"Pontszám: {score}";
            GameOverPanel.Visibility = Visibility.Visible;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();

            foreach (var r in rainDrops)
                r.Visibility = Visibility.Collapsed;

            BackButton.Visibility = Visibility.Collapsed;
            ScoreText.Visibility = Visibility.Collapsed;
            Bird.Visibility = Visibility.Collapsed;
            GameOverPanel.Visibility = Visibility.Collapsed;

            foreach (var p in topPipes) p.Visibility = Visibility.Collapsed;
            foreach (var p in bottomPipes) p.Visibility = Visibility.Collapsed;

            FogOverlay.Visibility = Visibility.Collapsed;

            MenuOverlay.Visibility = Visibility.Visible;
            MainMenu.Visibility = Visibility.Visible;
            DifficultyMenu.Visibility = Visibility.Collapsed;
        }
    }
}
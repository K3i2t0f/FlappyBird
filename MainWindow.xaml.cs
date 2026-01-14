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

        public MainWindow()
        {
            InitializeComponent();

            topPipes = new Rectangle[] { PipeTop1, PipeTop2, PipeTop3 };
            bottomPipes = new Rectangle[] { PipeBottom1, PipeBottom2, PipeBottom3 };

            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], 400 + i * 200);
                Canvas.SetLeft(bottomPipes[i], 400 + i * 200);
                ResetPipe(i);
            }

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Madár gravitáció
            birdVelocity += gravity;
            Canvas.SetTop(Bird, Canvas.GetTop(Bird) + birdVelocity);

            for (int i = 0; i < 3; i++)
            {
                Canvas.SetLeft(topPipes[i], Canvas.GetLeft(topPipes[i]) - pipeSpeed);
                Canvas.SetLeft(bottomPipes[i], Canvas.GetLeft(bottomPipes[i]) - pipeSpeed);

                if (Canvas.GetLeft(topPipes[i]) < -60)
                {
                    Canvas.SetLeft(topPipes[i], 400);
                    Canvas.SetLeft(bottomPipes[i], 400);
                    ResetPipe(i);
                }

                CheckCollision(topPipes[i]);
                CheckCollision(bottomPipes[i]);
            }

            if (Canvas.GetTop(Bird) < 0 || Canvas.GetTop(Bird) > 570)
                GameOver();
        }

        private void ResetPipe(int index)
        {
            int gap = rnd.Next(120, 180);
            int topHeight = rnd.Next(100, 250);

            topPipes[index].Height = topHeight;
            bottomPipes[index].Height = 600 - (topHeight + gap);

            Canvas.SetTop(topPipes[index], 0);
            Canvas.SetTop(bottomPipes[index], topHeight + gap);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                birdVelocity = -15;
        }

        private void CheckCollision(Rectangle pipe)
        {
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
            gameTimer.Stop();
            MessageBox.Show("Game Over!");
        }
    }
}
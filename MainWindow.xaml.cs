using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
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

namespace Audio_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<FileInfo> files = new List<FileInfo>();
        List<FileInfo> listeningHistory = new List<FileInfo>();
        int currentSongIndex = 0;
        bool isPlaying = false;
        bool isRepeating = false;
        bool isShuffled = false;
        private TimeSpan currentTime;
        private TimeSpan totalDuration;
        private Timer timer;
        public MainWindow()
        {
            InitializeComponent();
            songList.DisplayMemberPath = Name;
            timer = new Timer(UpdateTimerCallback, null, Timeout.Infinite, 1000);
        }

        private void UpdateTimerCallback(object state)
        {
            Dispatcher.Invoke(() =>
            {
                currentTime = mediaElement.Position;

                nowTime_textblock.Text = currentTime.ToString(@"mm\:ss");
                remainingTime_textblock.Text = (totalDuration - currentTime).ToString(@"mm\:ss");

                positionSong_button.Value = currentTime.TotalSeconds;
            });
        }

        private void play_button_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                if (isPlaying)
                {
                    mediaElement.Pause();
                    play_button.Content = new PackIcon { Kind = PackIconKind.Play, Height = 40, Width = 40, HorizontalAlignment= HorizontalAlignment.Center, VerticalAlignment= VerticalAlignment.Center };
                }
                else
                {
                    mediaElement.Play();
                    play_button.Content = new PackIcon { Kind = PackIconKind.Pause, Height = 40, Width = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                }

                isPlaying = !isPlaying;
            }
        }

        private void nextSong_button_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex < files.Count - 1)
            {
                currentSongIndex++; 
            }
            else
            {
                currentSongIndex = 0; 
            }

            PlaySelectedSong();
        }

        private void backSong_button_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex > 0)
            {
                currentSongIndex--;
            }
            else
            {
                currentSongIndex = files.Count - 1;
            }

            PlaySelectedSong();
        }


        private void shuffle_button_Click(object sender, RoutedEventArgs e)
        {
            if (!isShuffled)
            {
                Random rng = new Random();
                files = files.OrderBy(x => rng.Next()).ToList();
                isShuffled = true;
            }
            else
            {
                files = files.OrderBy(file => file.Name).ToList();
                isShuffled = false;
            }

            currentSongIndex = 0;
            PlaySelectedSong();
        }

        private void replay_button_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;

            if (isRepeating)
            {
                replay_button.Content = new PackIcon { Kind = PackIconKind.Replay, Height = 25, Width = 25, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Red };
            }
            else
            {
                replay_button.Content = new PackIcon { Kind = PackIconKind.Replay, Height = 25, Width = 25, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            }
        }

        private void folderMusic_button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                files = Directory.GetFiles(dialog.FileName)
                                  .Select(item => new FileInfo(item))
                                  .Where(file => IsAudioFile(file.Extension.ToLower()))
                                  .ToList();

                var displayNames = files.Select(file => file.Name);
                songList.ItemsSource = displayNames;

                if (files.Count > 0)
                {
                    currentSongIndex = 0; 
                    PlaySelectedSong();
                }
            }
        }
        private void PlaySelectedSong()
        {
            string selectedAudioPath = files[currentSongIndex].FullName;
            mediaElement.Source = new Uri(selectedAudioPath);
            mediaElement.Play();
            isPlaying = true;
            play_button.Content = new PackIcon { Kind = PackIconKind.Pause, Height = 40, Width = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            if (!listeningHistory.Contains(files[currentSongIndex]))
            {
                listeningHistory.Add(files[currentSongIndex]);
            }
        }

        private bool IsAudioFile(string extension)
        {
            return extension == ".mp3" || extension == ".m4a" || extension == ".wav";
        }

        private void historyMusic_button_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow history = new HistoryWindow(listeningHistory, mediaElement);
            var result = history.ShowDialog();
        }

        private void volume_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null)
            {
                mediaElement.Volume = volume_slider.Value / 100;
            }
        }

        private void positionSong_button_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(positionSong_button.Value);
        }

        private void songList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songList.SelectedIndex != -1) // Проверяем, что что-то выбрано
            {
                currentSongIndex = songList.SelectedIndex;
                PlaySelectedSong();
            }
        }


        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isRepeating)
            {
                mediaElement.Position = TimeSpan.Zero;
                mediaElement.Play();
            }
            else
            {
                NextSong();
            }
        }

        private void NextSong()
        {
            if (currentSongIndex < files.Count - 1)
            {
                currentSongIndex++;
            }
            else
            {
                currentSongIndex = 0;
            }

            PlaySelectedSong();
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            totalDuration = mediaElement.NaturalDuration.TimeSpan;

            positionSong_button.Maximum = totalDuration.TotalSeconds;

            timer.Change(0, 1000);
        }
    }
}
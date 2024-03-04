using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Audio_Player
{
    /// <summary>
    /// Логика взаимодействия для HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private List<FileInfo> listeningHistory;
        private MediaElement mediaElement;
        public bool? DialogResult { get; private set; }
        public HistoryWindow(List<FileInfo> history, MediaElement mediaElement)
        {
            InitializeComponent();
            this.listeningHistory = history;
            this.mediaElement = mediaElement;
            historyList.ItemsSource = listeningHistory.Select(file => file.Name);
            historyList.SelectionChanged += historyList_SelectionChanged;
        }

        private void historyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (historyList.SelectedIndex != -1) 
            {
                int selectedIndex = historyList.SelectedIndex; 
                if (selectedIndex >= 0 && selectedIndex < listeningHistory.Count) 
                {
                    PlaySelectedSong(selectedIndex); 
                }
            }
        }
        private void PlaySelectedSong(int selectedIndex)
        {
            string selectedAudioPath = listeningHistory[selectedIndex].FullName;
            mediaElement.Source = new Uri(selectedAudioPath);
            mediaElement.Play();

            this.DialogResult = true;
            this.Close();
        }

    }
}

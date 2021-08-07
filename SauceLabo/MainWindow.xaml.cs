using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SauceLabo.Modules;

namespace SauceLabo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Library library;
        DownloadManager downloadManager;
        DateTime buildDate = new DateTime(2021, 8, 7);

        public MainWindow()
        {
            InitializeComponent();

            lblBuildDate.Content = $"Build date: {buildDate:dd/MM/yyyy}";

            downloadManager = new DownloadManager() {
                txbOutput = txbOutput
            };
        }

        private void TxbUri_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                downloadManager.doFetch(txbUri.Text);
        }

        private void TxbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            txbOutput.ScrollToEnd();
        }

        private void BtnLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (library == null ||
                !library.IsLoaded)
            {
                library = new Library();
                library.Show();
            }
            else {
                library.Focus();
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            library.Close();
        }
    }
}

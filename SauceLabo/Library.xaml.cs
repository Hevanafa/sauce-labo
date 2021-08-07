using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using SauceLabo.Modules;

namespace SauceLabo
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Window
    {
        public Library()
        {
            InitializeComponent();

            refreshList(true);
        }

        async Task<string> readLibraryEntries()
        {
            try
            {
                if (!File.Exists(DownloadManager.libraryFilename))
                    File.Create(DownloadManager.libraryFilename);

                var sr = new StreamReader(DownloadManager.libraryFilename);

                var file = await sr.ReadToEndAsync();

                sr.Close();
                sr.Dispose();

                return file;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        string searchTerm
        {
            get
            {
                return txbSearch.Text.Trim().ToLower();
            }
        }

        string[][] filteredTable
        {
            get
            {
                return entryRows.Where(x =>
                    x.ToLower().Contains(searchTerm)
                ).Select(x => x.Split('\t'))
                .ToArray();
            }
        }


        string entries;
        string[] entryRows;
        string[][] libraryTable;

        async void refreshList(bool purge = false)
        {
            if (purge)
                entries = null;

            // Done: search
            if (entries == null)
            {
                lbDoujin.Items.Add("Reading list...");
                entries = await readLibraryEntries();

                if (entries == "")
                    return;

                entryRows = entries
                    .Split('\n')
                    .Where(x => x.Length > 0)
                    .Select(x => x.Trim('\r', '\n'))
                    .ToArray();

                libraryTable = entryRows
                    .Select(x => x.Split('\t'))
                    .ToArray();
            }

            lbDoujin.Items.Clear();

            foreach (var row in (searchTerm.Length > 0 ? filteredTable : libraryTable))
            {
                var enTitle = row[2];
                lbDoujin.Items.Add(enTitle);
            }
        }

        private void LbDoujin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Done: load the cover
            // Done: load the information
            // Todo: upgrade the information box to use Rich Text
            if (lbDoujin.SelectedIndex < 0)
                txbInformation.Text = "";
            else
            {
                var row = libraryTable.Where(x =>
                    x.Contains(lbDoujin.SelectedItem)
                ).ToArray()
                .First();

                //originalLink,
                //enTitle,
                //jpTitle,
                //filename,
                //tags

                var appPath = AppDomain.CurrentDomain.BaseDirectory;
                var img = new BitmapImage(new Uri(
                    Path.Combine(
                        appPath,
                        row[4]
                    )
                ));

                imgCover.Source = img;

                txbInformation.Text = string.Join(
                    "\n",
                    "Link: " + row[1],
                    "EN Title: " + row[2],
                    "JP Title: " + (row[3].Length > 0 ? row[3] : "N/A"),
                    "Tags: " + row[5]
                );
            }
        }

        private void BtnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            refreshList(true);
        }

        private void TxbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshList();
        }

        private void ImgCover_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var idx = lbDoujin.SelectedIndex;

            // Done: open the source image
            if (e.ClickCount == 2 &&
                idx >= 0)
            {
                Process.Start(
                    searchTerm.Length > 0
                    ? filteredTable[idx][4]
                    : libraryTable[idx][4]);
            }
        }

        async void deleteSelectedEntry(int idx) {
            // Pending test: delete from normal table
            // Pending test: delete from filtered table

            string targetId = searchTerm.Length == 0
                ? libraryTable[idx][0]
                : filteredTable[idx][0];

            //entryRows.Where(x => !x.StartsWith(targetId));
            await deleteEntryById(targetId);

            refreshList(true);
        }

        async Task deleteEntryById(string id)
        {
            entryRows = entryRows
                .Where(x => !x.StartsWith(id))
                .ToArray();

            var sw = new StreamWriter(DownloadManager.libraryFilename);

            foreach (var row in entryRows)
                await sw.WriteLineAsync(
                    row
                );

            sw.Close();
            sw.Dispose();
        }

        private void LbDoujin_KeyDown(object sender, KeyEventArgs e)
        {
            var idx = lbDoujin.SelectedIndex;

            // Done: implement entry deletion
            if (e.Key == Key.Delete &&
                idx >= 0)
            {
                if (MessageBox.Show(
                    $"Delete {lbDoujin.SelectedItem}?",
                    "Delete Item",
                    MessageBoxButton.YesNo
                    ) == MessageBoxResult.Yes)
                {
                    deleteSelectedEntry(idx);
                }
            }
        }
    }
}

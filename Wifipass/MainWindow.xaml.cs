using System.Windows;
using System.Windows.Controls;

namespace Wifipass
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadContent();
            if (!mainWindow.IsFocused)
            {
                mainWindow.Focus();
            }
        }

        private void LoadContent()
        {
            var ws = new WifiStorage();
            ws.GetNamesAndPasswords();
            content.Items.Clear();

            for (int i = 0; i < ws.names.Count; i++)
            {
                content.Items.Add(new WifiEntity { name = ws.names[i], password = ws.passwords[i] });
            }
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            LoadContent();
            descriptionTextBlock.Text = "Wi-Fi networks and passwords checked!";
        }

        private void Button_Click_Copy(object sender, RoutedEventArgs e)
        {
            if(content.SelectedCells.Count != 0)
            {
                var cellInfo = content.SelectedCells[0];
                var cell = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                Clipboard.SetText(cell.ToString());
                descriptionTextBlock.Text = "Copied value: " + cell.ToString();
            }
        }

        private DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;

            return null;
        }
    }
}

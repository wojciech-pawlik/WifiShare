using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WifiShare.Logic;

namespace WifiShare
{
    public partial class MainWindow : Window
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MainWindow> _windowLogger;
        private readonly ILogger<WifiStorage> _logicLogger;
        private readonly WifiStorage _wifiStorage;

        public MainWindow(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _windowLogger = _loggerFactory.CreateLogger<MainWindow>();
            _logicLogger = _loggerFactory.CreateLogger<WifiStorage>();
            InitializeComponent();

            if (!mainWindow.IsFocused)
            {
                mainWindow.Focus();
            }

            _wifiStorage = new WifiStorage(_logicLogger);
            LoadContent();
        }

        private void LoadContent()
        {
            _wifiStorage.WifiEntities.Clear();
            try
            {
                _wifiStorage.GetNamesAndPasswords();
                content.Items.Clear();
                foreach (var wifiEntity in _wifiStorage.WifiEntities)
                {
                    content.Items.Add(wifiEntity);
                }
                logTextBlock.Text = Properties.Resources.ResourceManager.GetString("ActionLoaded");
            }
            catch (Exception ex)
            {
                logTextBlock.Text = ex.Message;
            }
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            LoadContent();
        }

        private void Button_Click_Copy(object sender, RoutedEventArgs e)
        {
            var itemsToCopy = _wifiStorage.GetEntities(content.SelectedItems.Cast<WifiEntity>());

            if (itemsToCopy.Count == 1)
            {
                var itemToCopy = itemsToCopy.First();
                Clipboard.SetText(itemToCopy.Password);
                logTextBlock.Text = string.Format(Properties.Resources.ResourceManager.GetString("ActionCopied"),
                    itemToCopy.Name);
                return;
            }

            Clipboard.SetText(_wifiStorage.ExportToText(content.SelectedItems.Cast<WifiEntity>()));
            var copyInfo = string.Join(", ", itemsToCopy.Select(x => x.Name).ToList());
            logTextBlock.Text = string.Format(Properties.Resources.ResourceManager.GetString("ActionCopiedMany"),
                    copyInfo);
        }

        private void Button_Click_Show(object sender, RoutedEventArgs e)
        {
            var itemsToShowHide = content.SelectedCells.Count == 0
                ? content.Items
                : content.SelectedItems;

            foreach (WifiEntity item in itemsToShowHide)
            {
                if (item != null && item.IsHidden)
                {
                    ShowHidePassword(item);
                }
            }
        }

        private void Button_Click_Hide(object sender, RoutedEventArgs e)
        {
            var itemsToShowHide = content.SelectedCells.Count == 0
                ? content.Items
                : content.SelectedItems;

            foreach (WifiEntity item in itemsToShowHide)
            {
                if (item != null && !item.IsHidden)
                {
                    ShowHidePassword(item);
                }
            }
        }

        private void Button_Click_Export(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "wifi_networks";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt|JSON files (.json)|*.json";

            if (dialog.ShowDialog() == true)
            {
                string filename = dialog.FileName;
                string extension = Path.GetExtension(filename);

                var selectedItems = content.SelectedItems.Cast<WifiEntity>();

                var dataToExport = extension == ".json"
                    ? _wifiStorage.ExportToJson(selectedItems)
                    : _wifiStorage.ExportToText(selectedItems);

                File.WriteAllText(filename, dataToExport);
            }
        }

        private void DeselectAll(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var element = e.OriginalSource as FrameworkElement;

                if (element != null && element.Parent is DataGridColumnHeader ||
                    element is ScrollViewer || element is Border)
                {
                    content.SelectedItems.Clear();
                }
            }
        }

        private void SelectAll(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                content.SelectAll();
                e.Handled = true;
            }
        }

        private void ShowHidePassword(WifiEntity item)
        {
            (item.Password, item.PasswordHidden) = (item.PasswordHidden, item.Password);
            item.IsHidden = !item.IsHidden;
        }
    }
}



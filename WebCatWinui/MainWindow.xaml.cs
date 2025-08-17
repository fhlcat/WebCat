using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WebCat;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WebCatWinui
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewmodel _viewModel = new();

        public MainWindow()
        {
            _viewModel.WorkFinishedAsync += ShowResultAsync;
            InitializeComponent();
        }

        private async Task ShowResultAsync(Main.MainResult record)
        {
            var results = record.Value.Select(pair =>
                pair.Key.Title +
                ':' +
                string.Join(", ", pair.Value)
            );
            var content = record.Query + string.Join('\n', results);
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Save your work?",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = content
            };

            await dialog.ShowAsync();
        }
    }
}
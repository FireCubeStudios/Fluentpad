using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Fluentpad.Views
{
    public sealed partial class PlantxtViewPage : Page, INotifyPropertyChanged
    {
        public PlantxtViewPage()
        {
            InitializeComponent();
            var fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();

            // Put the individual fonts in the list
            foreach (string font in fonts)
            {
                Fonts.Add(new Tuple<string, FontFamily>(font, new FontFamily(font)));
            }
        }
        public List<Tuple<string, FontFamily>> Fonts { get; } = new List<Tuple<string, FontFamily>>()
            {
                new Tuple<string, FontFamily>("Arial", new FontFamily("Arial")),
                new Tuple<string, FontFamily>("Comic Sans MS", new FontFamily("Comic Sans MS")),
                new Tuple<string, FontFamily>("Courier New", new FontFamily("Courier New")),
                new Tuple<string, FontFamily>("Segoe UI", new FontFamily("Segoe UI")),
                new Tuple<string, FontFamily>("Times New Roman", new FontFamily("Times New Roman"))
            };
        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
        /// <summary>
        /// Temporary store the copy of text when it loaded, 
        /// if it didn't match the textbox=it changed
        /// </summary>
       

        private bool IsCttrlPressed()
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        public List<double> FontSizes { get; } = new List<double>()
            {
                8,
                9,
                10,
                11,
                12,
                14,
                16,
                18,
                20,
                24,
                28,
                36,
                48,
                72
            };
  
        private void PlainText_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void Combo3_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Combo3.SelectedIndex = 9;


                Combo3.TextSubmitted += Combo3_TextSubmitted;
            
        }

        private void Combo2_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Combo2.SelectedIndex = 2;
        }
        private void Combo3_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            bool isDouble = double.TryParse(sender.Text, out double newValue);

            // Set the selected item if:
            // - The value successfully parsed to double AND
            // - The value is in the list of sizes OR is a custom value between 8 and 100
            if (isDouble && (FontSizes.Contains(newValue) || (newValue < 100 && newValue > 8)))
            {
                // Update the SelectedItem to the new value. 
                sender.SelectedItem = newValue;
            }
            else
            {
                // If the item is invalid, reject it and revert the text. 
                sender.Text = sender.SelectedValue.ToString();

                var dialog = new ContentDialog
                {
                    Content = "The font size must be a number between 8 and 100.",
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Close
                };
                var task = dialog.ShowAsync();
            }

            // Mark the event as handled so the framework doesn’t update the selected item automatically. 
            args.Handled = true;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlainText.Height = Window.Current.Bounds.Height - 80;
            PlainText.Width = Window.Current.Bounds.Width;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji);
        }

      
       private List<int> LineIndices = new List<int>();
        private void GetPosition(int position)
        {
            var target = position;

            var lines = LineIndices.Where(i => i < target).ToList();

            if (lines.Count != 0)
            {
                var lastMarker = lines.Max(i => i);

                CurrentPos.Text = "Column: " + (position - lastMarker).ToString();
                LinePos.Text = "Line: " + (LineIndices.IndexOf(lastMarker) + 2).ToString();
            }
            else
            {
                LinePos.Text = "Line: 1";
                CurrentPos.Text = "Column: " + (position + 1).ToString();
            }
        }
     

        private void PlainText_SelectionChanged(object sender, RoutedEventArgs e)
        {
           GetPosition(PlainText.SelectionStart + PlainText.SelectionLength);
        }
        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            PlainText.SelectionChanged -= PlainText_SelectionChanged;
        }
        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
                PlainText.SelectionChanged -= PlainText_SelectionChanged;
                Reindex();

                PlainText_SelectionChanged(sender, e);

                PlainText.SelectionChanged += PlainText_SelectionChanged;
            
        }
        private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ScrollZoom != null)
            {
                ScrollZoom.ChangeView(null, null, (float)e.NewValue);
            }
        }
        public void Reindex()
        {
 

            var index = -1;
            var text = PlainText.Text.Replace(Environment.NewLine, "\r");
            while ((index = text.IndexOf('\r', index + 1)) > -1)
            {
                index++;
                LineIndices.Add(index);

                if (index + 1 >= text.Length) break;
            }

            GetPosition(PlainText.SelectionStart + PlainText.SelectionLength);
        }

        private void ScrollZoom_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ZoomSlider.Value = ScrollZoom.ZoomFactor;
        }

        private void ScrollZoom_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ZoomSlider.Value = ScrollZoom.ZoomFactor;
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleThemeTeachingTip1.IsOpen = true;
        }
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add("*");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {

                //PlainText.Text = await FileIO.ReadTextAsync(file);
                var provider = new StorageFileDataProvider();
                var bytes = await provider.LoadDataAsync(file);
                var reader = new EncodingReader();
                reader.AddBytes(bytes);

                documentViewModel.CurrentEncoding = Settings.DefaultEncoding switch
                {
                    "UTF-8" => Encoding.UTF8,
                    "Utf8" => Encoding.UTF8,
                    "UTF-16 LE" => Encoding.Unicode,
                    "UTF-16 BE" => Encoding.BigEndianUnicode,
                    "UTF-32" => Encoding.UTF32,
                    _ => Encoding.ASCII
                };

                byte[] bom = null;

                try
                {
                    ByteOrderMarks.ToList().ForEach(pair =>
                    {
                        var (key, value) = pair;

                        if (!bytes.AsSpan(0, value.Length).StartsWith(value.AsSpan(0, value.Length))) return;

                        var encoding = key switch
                        {
                            ByteOrderMark.Utf8 => Encoding.UTF8,
                            ByteOrderMark.Utf16Be => Encoding.BigEndianUnicode,
                            ByteOrderMark.Utf16Le => Encoding.Unicode,
                            ByteOrderMark.Utf32Be => Encoding.UTF32,
                            ByteOrderMark.Utf32Le => Encoding.UTF32,
                            ByteOrderMark.Utf7A => Encoding.UTF7,
                            ByteOrderMark.Utf7B => Encoding.UTF7,
                            ByteOrderMark.Utf7C => Encoding.UTF7,
                            ByteOrderMark.Utf7D => Encoding.UTF7,
                            ByteOrderMark.Utf7E => Encoding.UTF7,
                            _ => Encoding.ASCII
                        };

                        documentViewModel.CurrentEncoding = encoding;

                        reader = new EncodingReader();
                        reader.AddBytes(bytes.AsSpan().Slice(value.Length).ToArray());
                        bom = value;
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(new EventId(), $"Error loading {file.Name}.", ex);
                    Settings.Status(ex.Message, TimeSpan.FromSeconds(60), Verbosity.Error);
                }

                var text = reader.Read(documentViewModel.CurrentEncoding);
            }
        }
    }

}


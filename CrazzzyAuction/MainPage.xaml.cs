// ============================================================================
//               CrazzzyAuction by CrazzzyPeter (c) 2021-2021 
// ============================================================================
//
// Contacts:
//   https://github.com/crazzzypeter
//   https://www.youtube.com/c/crazzzypeter
//   https://www.twitch.tv/crazzzypeter
//   instagram: @crazzzypeter
//   telegram: @crazzzypeter
//
// ============================================================================
// License: GNU GPL 2.0
// ============================================================================    
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace CrazzzyAuction
{
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("ConvertBack not supported");
        }
    }

    public class ChanceToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Math.Round((double)value * 100, 1).ToString() + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("ConvertBack not supported");
        }
    }

    public class AuctionModeToIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == parameter as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("ConvertBack not supported");
        }
    }

    public class AuctionBankToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "NULL";
            return "Банк: " + ((int)value).ToString() + " руб.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("ConvertBack not supported");
        }
    }

    public class TimeSpanToTimerValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "NULL";
            return ((TimeSpan)value + new TimeSpan(10000 * 999)).ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("ConvertBack not supported");
        }
    }

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly AuctionModel _Auction;
        private readonly AuctionTimerModel _Timer;

        public AuctionTimerModel Timer
        {
            get
            {
                return _Timer;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;

            _Auction = App.AuctionModel;
            if (Auction.Items.Count == 0)
            {
                ClearAll();
            }
            
            listView.ItemsSource = Auction.SortedItems;

            _Timer = new AuctionTimerModel();
            Timer.Time = new TimeSpan(0, 10, 0);
        }

        public AuctionModel Auction
        {
            get
            {
                return _Auction;
            }
        }

        private void ClearAll()
        {
            Auction.Clear();
            Auction.Add();
            Auction.Add();
            Auction.Add();
        }

        private Lot LotFromSender(object sender)
        {
            if (sender is FrameworkElement control) {
                if (control.DataContext != null)
                    return (control.DataContext as Lot);
            }
            return null;
        }

        private void TextBox_Amount_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Auction.IncreaseRate(LotFromSender(sender));
            }
        }

        private void TextBox_Addition_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Auction.IncreaseRate(LotFromSender(sender));
            }
        }

        // фикс вставки многострочного текста
        private async void FixedTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                e.Handled = true;

                // Get content from the clipboard.
                var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
                {
                    try
                    {
                        var text = await dataPackageView.GetTextAsync();

                        string singleLineText = text.Replace("\r\n", "");

                        textBox.Text = singleLineText;
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
        }

        private void TextBox_Amount_LostFocus(object sender, RoutedEventArgs e)
        {
            Auction.IncreaseRate(LotFromSender(sender));
        }

        private void TextBox_Addition_LostFocus(object sender, RoutedEventArgs e)
        {
            Auction.IncreaseRate(LotFromSender(sender));
        }

        private void Button_AddMoney_Click(object sender, RoutedEventArgs e)
        {
            Auction.IncreaseRate(LotFromSender(sender));
        }

        private void Rectangle_Color_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Auction.MakeRandomColor(LotFromSender(sender));
        }

        // очистка всего
        private async void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog clearDialog = new ContentDialog
            {
                Title = "Очистить все?",
                Content = "Очистка приведет к удалению всех лотов, без возможности восстановления. Очистить все?",
                PrimaryButtonText = "Да",
                CloseButtonText = "Нет"
            };

            ContentDialogResult result = await clearDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ClearAll();
            }
        }

        // удаление лота
        private async void Button_DeleteLot_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Удалить лот?",
                PrimaryButtonText = "Да",
                CloseButtonText = "Нет"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Auction.Delete(LotFromSender(sender));
            }
        }
 
        private void RadioButton_AuctionMode_Click(object sender, RoutedEventArgs e)
        {
            var stringMode = (sender as Control).Tag as string;
            
            if(stringMode == AuctionMode.Normal.ToString())
                Auction.Mode = AuctionMode.Normal;
            if (stringMode == AuctionMode.Reversed.ToString())
                Auction.Mode = AuctionMode.Reversed;
            if (stringMode == AuctionMode.Roulette.ToString())
                Auction.Mode = AuctionMode.Roulette;
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            Auction.Add();
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!Timer.IsRunning)
                Timer.Start();
            else
                Timer.Stop();
        }

        private void Button_ResetTime_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            Timer.Time = new TimeSpan(0);
        }

        private void Button_Add1Time_Click(object sender, RoutedEventArgs e)
        {
            Timer.Time += new TimeSpan(0, 1, 0);
        }

        private void Button_Add2Time_Click(object sender, RoutedEventArgs e)
        {
            Timer.Time += new TimeSpan(0, 2, 0);
        }

        private void Button_Set10Time_Click(object sender, RoutedEventArgs e)
        {
            Timer.Time = new TimeSpan(0, 10, 0);
            Timer.Stop();
        }

        private void Button_Sub1Time_Click(object sender, RoutedEventArgs e)
        {
            Timer.Time += new TimeSpan(0, -1, 0);
        }
    }
}

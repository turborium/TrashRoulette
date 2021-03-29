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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrazzzyAuction
{
    public interface ILot
    {
        string Name { get; set; }
        string Amount { get; set; }
        string Addition { get; set; }
        double Chance { get; }
        Color Color { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Lot : ILot, INotifyPropertyChanged
    {
        // name
        private string _Name;
        // amount
        private string _Amount;
        private int? _AmountValue;
        // addition
        private string _Addition;
        private int? _AdditionValue;
        // chance
        private double _Chance;
        // color
        private Color _Color;
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public Lot() {
            // name
            _Name = "";
            // amount
            _Amount = "";
            _AmountValue = 0;
            // addition
            _Addition = "";
            _AdditionValue = 0;
            // chance
            _Chance = 0;
            // color
            MakeRandomColor();
        }
 
        public double Chance
        {
            get
            {
                return _Chance;
            }
            internal set
            {
                _Chance = value;
                NotifyPropertyChanged();
            }
        }

        [JsonProperty]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged();
            }
        }

        [JsonProperty]
        public string Amount
        {
            get
            {
                return _Amount;
            }
            set
            {
                _Amount = value;
                _AmountValue = ParseNumber(value);
                NotifyPropertyChanged();
            }
        }

        public int? AmountValue
        {
            get
            {
                return _AmountValue;
            }
        }

        [JsonProperty]
        public string Addition
        {
            get
            {
                return _Addition;
            }
            set
            {
                _Addition = value;
                _AdditionValue = ParseNumber(value);
                NotifyPropertyChanged();
            }
        }

        public int? AdditionValue
        {
            get
            {
                return _AdditionValue;
            }
        }

        [JsonProperty]
        public Color Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
                NotifyPropertyChanged();
            }
        }

        internal void MakeRandomColor()
        {
            var random = new Random();
            _Color = ColorFromHSV(random.NextDouble() * 360, random.NextDouble() * 0.5 + 0.5, random.NextDouble() * 0.0 + 1.0);
            NotifyPropertyChanged("Color");
        }
 
        // The ranges are 0 - 360 for hue, and 0 - 1 for saturation or value.
        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private int? ParseNumber(string Str)
        {
            double value;

            if (Str == "")
                return 0;
            if (double.TryParse(Str, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return (int)value;
            if (double.TryParse(Str, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return (int)value;

            return null;
        }

        internal void IncreaseRate()
        {
            if(AmountValue != null && AdditionValue != null)
            {
                var NewAmount = AmountValue + AdditionValue;

                Addition = "";
                if (NewAmount != 0)
                    Amount = NewAmount.ToString();
                else
                    Amount = "";
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class LotComparer: IComparer<Lot>
    {
        public int Compare(Lot x, Lot y)
        {
            if (x.AmountValue == null)
                return 1;

            if (y.AmountValue == null)
                return -1;

            return (int)y.AmountValue - (int)x.AmountValue;
        }
    }

    internal class ReverseLotComparer : IComparer<Lot>
    {
        public int Compare(Lot x, Lot y)
        {
            if (x.AmountValue == null)
                return 1;

            if (y.AmountValue == null)
                return -1;

            if ((int)x.AmountValue == 0)
                return 1;

            if ((int)y.AmountValue == 0)
                return -1;

            return (int)x.AmountValue - (int)y.AmountValue;
        }
    }

    public enum AuctionMode
    {
        [EnumMember(Value = "Normal")]
        Normal,
        [EnumMember(Value = "Reversed")]
        Reversed,
        [EnumMember(Value = "Roulette")]
        Roulette
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AuctionModel : INotifyPropertyChanged
    {
        // items
        [JsonProperty("Items")]
        private readonly ObservableCollection<ILot> _Items;
        private readonly ReadOnlyObservableCollection<ILot> _ReadOnlyItems;
        // sorted items
        private readonly ObservableCollection<ILot> _SortedItems;
        private readonly ReadOnlyObservableCollection<ILot> _ReadOnlySortedItems;
        // bank
        private int _Bank;
        // auk mode
        private AuctionMode _Mode;
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public AuctionModel()
        {
            // items
            _Items = new ObservableCollection<ILot>();
            _ReadOnlyItems = new ReadOnlyObservableCollection<ILot>(_Items);
            // sorted items
            _SortedItems = new ObservableCollection<ILot>();
            _ReadOnlySortedItems = new ReadOnlyObservableCollection<ILot>(_SortedItems);
            // init
            _Items.CollectionChanged += Items_CollectionChanged;
        }

        public int Bank {
            get
            {
                return _Bank;
            } 
        }

        [JsonProperty]
        public AuctionMode Mode {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
                RebuildSortedItems();
                NotifyPropertyChanged();
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildSortedItems();
            UpdateItems();
        }

        public ReadOnlyObservableCollection<ILot> Items
        {
            get
            {
                return _ReadOnlyItems;
            } 
        }

        public ReadOnlyObservableCollection<ILot> SortedItems
        {
            get
            {
                return _ReadOnlySortedItems;
            }
        }

        public void Clear()
        {
            _Items.Clear();
        }

        public ILot Add()
        {
            var lot = new Lot();

            _Items.Add(lot);
            UpdateItems();

            return lot;
        }

        public void Delete(ILot item)
        {
            _Items.Remove(item);
        }

        public void IncreaseRate(ILot item)
        {
            (item as Lot).IncreaseRate();
            RebuildSortedItems();
            UpdateItems();
        }

        public void MakeRandomColor(ILot item)
        {
            (item as Lot).MakeRandomColor();
        }

        private void RebuildSortedItems()
        {
            IComparer<Lot> comparer;

            if (Mode != AuctionMode.Reversed)
                comparer = new LotComparer();
            else
                comparer = new ReverseLotComparer();

            var sortedItems = _Items.OrderBy(x => x as Lot, comparer);

            // remove
            var needToRemove = new List<Lot>();
            foreach (var item in _SortedItems)
            {
                if (!_Items.Contains(item))
                    needToRemove.Add(item as Lot);
            }
            foreach (var item in needToRemove)
                _SortedItems.Remove(item);
            // add
            foreach (var item in _Items)
            {
                if (!_SortedItems.Contains(item))
                    _SortedItems.Add(item as Lot);
            }
            // sort
            for (int i = 0; i < sortedItems.Count(); i++)
            {
                var index = _SortedItems.IndexOf(sortedItems.ElementAt(i) as Lot);
                if (index != i)
                    _SortedItems.Move(index, i);
            }
            /*
            // set sorted indexes
            for (int i = 0; i < sortedItems.Count(); i++)
            {
                sortedItems.ElementAt(i).SortedIndex = i;
            }
            */
        }
 
        private void UpdateItems()
        {
            _Bank = 0;
            int positiveBank = 0;

            // calc bank
            foreach (var item in _Items)
            {
                if ((item as Lot).AmountValue != null)
                {
                    var value = (int)(item as Lot).AmountValue;
                    _Bank += Math.Abs(value);
                    positiveBank += Math.Max(0, value);
                }
            }
            // update chance
            foreach (var item in _Items)
            {
                if ((item as Lot).AmountValue != null && positiveBank != 0)
                    (item as Lot).Chance = Math.Max(0, (double)(item as Lot).AmountValue / positiveBank);
                else
                    (item as Lot).Chance = 0;
            }
            // need upd bank
            NotifyPropertyChanged("Bank");
        }
 
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConverterILotToLot : CustomCreationConverter<ILot>
    {
        public override ILot Create(Type objectType)
        {
            return new Lot();
        }
    }
}

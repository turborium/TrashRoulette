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
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CrazzzyAuction
{
    public static partial class ListViewDividerExtensions
    {
        private class UltraHack
        {
            public delegate void DividerItemsVectorChangedEvent(ListViewBase ListView, IObservableVector<object> sender, IVectorChangedEventArgs args);
            public event DividerItemsVectorChangedEvent OnDividerItemsVectorChanged;
            public readonly ListViewBase ListView;
            public UltraHack(ListViewBase listView)
            {
                ListView = listView;
            }
            public void DividerItemsVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs args)
            {
                //  Debug.WriteLine(sender.GetHashCode().ToString());
                /*Debug.WriteLine(sender.Count.ToString());
                Debug.WriteLine(sender.IsReadOnly.ToString());
                Debug.WriteLine(args.CollectionChange.ToString());
                Debug.WriteLine(args.Index.ToString());*/

                OnDividerItemsVectorChanged?.Invoke(ListView, sender, args);
            }
        }

        private static Dictionary<ListViewBase, UltraHack> _itemsForList = new Dictionary<ListViewBase, UltraHack>();

        public static readonly DependencyProperty DividerNameProperty = DependencyProperty.RegisterAttached("DividerName", typeof(string), typeof(ListViewDividerExtensions), new PropertyMetadata(null, OnDividerNamePropertyChanged));

        public static string GetDividerName(ListViewBase obj)
        {
            return (string)obj.GetValue(DividerNameProperty);
        }

        public static void SetDividerName(ListViewBase obj, string value)
        {
            obj.SetValue(DividerNameProperty, value);
        }

        private static void OnDividerNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ListViewBase listViewBase = sender as ListViewBase;

            if (listViewBase == null)
            {
                return;
            }

            listViewBase.ContainerContentChanging -= DividerContainerContentChanging;
            _itemsForList.TryGetValue(listViewBase, out UltraHack hack);
            if (hack != null)
            {
                listViewBase.Items.VectorChanged -= hack.DividerItemsVectorChanged;
                hack.OnDividerItemsVectorChanged -= DividerItemsVectorChanged;
            }
            listViewBase.Unloaded -= OnListViewBaseUnloaded;

            if (DividerNameProperty != null)
            {
                listViewBase.ContainerContentChanging += DividerContainerContentChanging;
                if (hack == null)
                {
                    hack = new UltraHack(listViewBase);
                    _itemsForList[listViewBase] = hack;
                }
                listViewBase.Items.VectorChanged += hack.DividerItemsVectorChanged;
                hack.OnDividerItemsVectorChanged += DividerItemsVectorChanged;
                listViewBase.Unloaded += OnListViewBaseUnloaded;
            }
        }

        private static void DividerContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var itemContainer = args.ItemContainer as SelectorItem;

            SetItemContainerDivider(sender, itemContainer, args.ItemIndex);
        }

        private static void OnListViewBaseUnloaded(object sender, RoutedEventArgs e)
        {
            ListViewBase listViewBase = sender as ListViewBase;

            listViewBase.ContainerContentChanging -= DividerContainerContentChanging;
            
            listViewBase.Items.VectorChanged -= _itemsForList[listViewBase].DividerItemsVectorChanged; //listViewBase.Items.VectorChanged -= DividerItemsVectorChanged;
            _itemsForList[listViewBase].OnDividerItemsVectorChanged -= DividerItemsVectorChanged;
            _itemsForList.Remove(listViewBase);

            listViewBase.Unloaded -= OnListViewBaseUnloaded;
        }

        private static void DividerItemsVectorChanged(ListViewBase listView, IObservableVector<object> sender, IVectorChangedEventArgs args)
        {
      
            if (args.Index != 0 && args.Index != 1)
            {
                return;
            }

            if ((args.CollectionChange == CollectionChange.ItemInserted) || (args.CollectionChange == CollectionChange.ItemRemoved))
            {
                if (listView == null)
                {
                    return;
                }

                for (int i = 0; i < sender.Count; i++)
                {
                    var itemContainer = listView.ContainerFromIndex(i) as SelectorItem;
                    
                    if (itemContainer != null)
                    {
                        SetItemContainerDivider(listView, itemContainer, i);
                    }
                }
            } 
        }

        private static void SetItemContainerDivider(ListViewBase sender, SelectorItem itemContainer, int itemIndex)
        {
            string name = GetDividerName(sender);

            var divider = (itemContainer.ContentTemplateRoot as FrameworkElement).FindName(name) as FrameworkElement;

            if (divider == null)
                return;

            if (itemIndex == 0)
                divider.Visibility = Visibility.Collapsed;
            else
                divider.Visibility = Visibility.Visible;
        }
    }
}
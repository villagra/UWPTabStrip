using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPTabStripControl.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPTabStripControl.Controls
{
    public sealed partial class TabStrip : UserControl
    {
        Compositor _compositor;
        SpriteVisual _barIndicatorVisual;
        public event EventHandler<string> SelectionChanged;
        
        public List<String> Items
        {
            get { return (List<String>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<String>), typeof(TabStrip), new PropertyMetadata(null, (d, e) => { ((TabStrip)d).Render(); }));


        public int IndicatorWidth
        {
            get { return (int)GetValue(IndicatorWidthProperty); }
            set { SetValue(IndicatorWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IndicatorWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndicatorWidthProperty =
            DependencyProperty.Register("IndicatorWidth", typeof(int), typeof(TabStrip), new PropertyMetadata(40, (d, e) => { ((TabStrip)d).UpdateIndicator(); }));


        public Color IndicatorColor
        {
            get { return (Color)GetValue(IndicatorColorProperty); }
            set { SetValue(IndicatorColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndicatorColorProperty =
            DependencyProperty.Register("IndicatorColor", typeof(Color), typeof(TabStrip), new PropertyMetadata(Colors.Red, (d, e) => { ((TabStrip)d).UpdateIndicator(); }));

        public Style LeftButtonStyle
        {
            get { return (Style)GetValue(LeftButtonStyleProperty); }
            set { SetValue(LeftButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonStyleProperty =
            DependencyProperty.Register("LeftButtonStyle", typeof(Style), typeof(TabStrip), new PropertyMetadata(null, (d, e) => 
            {
                ((TabStrip)d).btnLeft.Style = ((TabStrip)d).LeftButtonStyle;                
            }));

        public Style RightButtonStyle
        {
            get { return (Style)GetValue(RightButtonStyleProperty); }
            set { SetValue(RightButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonStyleProperty =
            DependencyProperty.Register("RightButtonStyle", typeof(Style), typeof(TabStrip), new PropertyMetadata(null, (d, e) =>
            {
                ((TabStrip)d).btnRight.Style = ((TabStrip)d).RightButtonStyle;
            }));

        public int SelectedIndex
        {
            get
            {
                var selectedItem = pnlItems.Children.OfType<TabStripItem>().FirstOrDefault(p => p.IsSelected == true);
                if (selectedItem != null)
                {
                    return pnlItems.Children.IndexOf(selectedItem);
                }
                return -1;
            }

            set
            {
                if (value == -1 || value > pnlItems.Children.Count - 1)
                {
                    SelectItem(null);
                }
                else
                {
                    SelectItem(pnlItems.Children[value] as TabStripItem);
                }                
            }
        }        

        public TabStrip()
        {
            this.InitializeComponent();

            LeftButtonStyle = this.Resources["LeftButtonStyle"] as Style;
            RightButtonStyle = this.Resources["RightButtonStyle"] as Style;

            _compositor = Window.Current.Compositor;
            _barIndicatorVisual = _compositor.CreateSpriteVisual();
            
            _barIndicatorVisual.Brush = _compositor.CreateColorBrush(IndicatorColor);
            _barIndicatorVisual.Size = new System.Numerics.Vector2(IndicatorWidth, 2);
            _barIndicatorVisual.Opacity = 0;
            pnlIndicator.SetChildVisual(_barIndicatorVisual);

            this.SizeChanged += (x, y) => RenderPositionButtons();
            this.Loaded += TabStrip_Loaded;
            this.btnLeft.Tapped += BtnLeft_Tapped;
            this.btnRight.Tapped += BtnRight_Tapped;
            this.scroller.ViewChanged += Scroller_ViewChanged;
        }

        private void BtnLeft_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var newPosition = scroller.HorizontalOffset - scroller.ActualWidth;
            newPosition = newPosition < 0 ? 0 : newPosition;

            scroller.ChangeView(newPosition, null, null);
        }

        private void BtnRight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var newPosition = scroller.HorizontalOffset + scroller.ActualWidth;
            scroller.ChangeView(newPosition, null, null);
        }

        private void TabStrip_Loaded(object sender, RoutedEventArgs e)
        {
            var idx = SelectedIndex;
            if (idx != -1)
            {
                var x = GetIndicatorPositionForItem(pnlItems.Children[idx] as TabStripItem);
                var animation = _compositor.CreateVector3KeyFrameAnimation(null, new Vector3(x.ToFloat(), 0, 0), 250);
                _barIndicatorVisual.StartAnimation("Offset", animation);
            }
        }

        private void Render()
        {
            pnlItems.Children.Clear();
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    TabStripItem mi = new TabStripItem();
                    mi.DataContext = item;
                    pnlItems.Children.Add(mi);
                    mi.Tapped += Mi_Tapped;
                }

                RenderPositionButtons();
            }
        }

        private void UpdateIndicator()
        {
            _barIndicatorVisual.Size = new System.Numerics.Vector2(IndicatorWidth, 2);
            _barIndicatorVisual.Brush = _compositor.CreateColorBrush(IndicatorColor);
        }

        private void Mi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            TabStripItem mi = sender as TabStripItem;
            if (mi != null)
            {
                SelectItem(mi);
                SelectionChanged?.Invoke(this, mi.DataContext.ToString());
            }
        }

        private void RenderPositionButtons()
        {
            if (scroller.ExtentWidth > scroller.ActualWidth)
            {
                pnlbuttons.Visibility = Visibility.Visible;                
                Scroller_ViewChanged(this, null);
            }
            else
            {
                pnlbuttons.Visibility = Visibility.Collapsed;
            }            
        }

        private void Scroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            btnLeft.Visibility = scroller.HorizontalOffset > 0 ? Visibility.Visible : Visibility.Collapsed;
            btnRight.Visibility = scroller.HorizontalOffset < (scroller.ExtentWidth - scroller.ActualWidth) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SelectItem(TabStripItem mi)
        {
            var selectedItems = pnlItems.Children.OfType<TabStripItem>().Where(p => p.IsSelected == true).ToList();
            if (selectedItems.Count() > 0)
            {
                foreach (var item in selectedItems)
                {
                    if (item != mi)
                    {
                        item.IsSelected = false;
                    }
                }
            }

            if (mi != null)
            {
                ScalarKeyFrameAnimation fadeIn = null;                
                if (selectedItems.Count == 0)
                {
                    fadeIn = _compositor.CreateScalarKeyFrameAnimation(null, 1, 250, 0);
                }

                mi.IsSelected = true;                
                var x = GetIndicatorPositionForItem(mi);

                if (fadeIn != null)
                {
                    _barIndicatorVisual.Offset = new Vector3(x.ToFloat(), 0, 0);
                    _barIndicatorVisual.StartAnimation("Opacity", fadeIn);
                }
                else
                {
                    var animation = _compositor.CreateVector3KeyFrameAnimation(null, new Vector3(x.ToFloat(), 0, 0), 250);
                    _barIndicatorVisual.StartAnimation("Offset", animation);
                }
            }
            else
            {
                var fadeOut = _compositor.CreateScalarKeyFrameAnimation(null, 0, 250, 0);
                _barIndicatorVisual.StartAnimation("Opacity", fadeOut);
            }
        }

        private Double GetIndicatorPositionForItem(TabStripItem item)
        {
            var position = item.RelativePosition(pnlItems);
            return position.X - IndicatorWidth / 2 + item.ActualWidth / 2;
        }

    }
}

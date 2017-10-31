using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class TabStripItem : UserControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TabStripItem), new PropertyMetadata(false, (d, e) => { ((TabStripItem)d).UpdateOpacity(); }));

        public TabStripItem()
        {
            this.InitializeComponent();
            this.PointerEntered += OptionsMenuItem_PointerEntered;
            this.PointerExited += OptionsMenuItem_PointerExited;

            UpdateOpacity();
        }

        private void OptionsMenuItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            UpdateOpacity();
        }

        private void UpdateOpacity()
        {
            this.Visual().Opacity = IsSelected ? 1f : 0.5f;
        }

        private void OptionsMenuItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var fadeIn = Window.Current.Compositor.CreateScalarKeyFrameAnimation(null, 0.9f, 250, 0, null, AnimationIterationBehavior.Count);
                this.Visual().StartAnimation("Opacity", fadeIn);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPTabStripControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            List<string> items1 = new List<string>();
            items1.Add("Blue");
            items1.Add("Red");
            items1.Add("Yellow");
            items1.Add("Orange");
            items1.Add("Purple");
            items1.Add("Black");
            items1.Add("White");

            tabStrip1.Items = items1;
            tabStrip1.SelectionChanged += TabStrip_SelectionChanged;
            tabStrip1.SelectedIndex = 0;

            tabStrip2.Items = items1;
            tabStrip2.SelectionChanged += TabStrip_SelectionChanged;
            tabStrip2.SelectedIndex = 1;
        }

        private void TabStrip_SelectionChanged(object sender, string e)
        {
            txtSelectedItem.Text = $"Selected: {e}";
        }
    }
}

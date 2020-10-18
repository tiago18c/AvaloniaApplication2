using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace AvaloniaApplication2
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void evt(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Console.WriteLine("aaaaaaa");
        }


        public void mouseMoved(object source, PointerEventArgs e)
        {
            var p = e.GetPosition(this);

            Console.WriteLine(p);
        }
    }
}

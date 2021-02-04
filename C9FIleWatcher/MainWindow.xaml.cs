using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C9FIleWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Helper to make app access seamless.
        private C9FIleWatcher.App App()
        {
            return ((C9FIleWatcher.App) Application.Current);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        C9FIleWatcher.App MyApp()
        {
            C9FIleWatcher.App myApp = (C9FIleWatcher.App) Application.Current;

            return (myApp);
        }

        private void ImageToDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    bool newOne = MyApp().AddTo(file);

                    if (newOne)
                    {
                        var border = new Border();
                        border.BorderThickness = new Thickness(5);
                        border.BorderBrush = new SolidColorBrush(Colors.Red);

                        DockPanel panel = new DockPanel();

                        border.Child = panel;

                        Label label = new Label();
                        label.Content = file;
                        panel.Children.Add(label);

                        DockPanel.SetDock(border, Dock.Top);
                        DockPanel_WorkingItems.Children.Add(border);
                    }
                }
            }
        }

        private void ImageFromDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                   bool newOne = MyApp().AddFrom(file);

                   if (newOne)
                   {
                       var border = new Border();
                       border.BorderThickness = new Thickness(5);
                       border.BorderBrush = new SolidColorBrush(Colors.Green);

                       DockPanel panel = new DockPanel();

                       border.Child = panel;

                       Label label = new Label();
                       label.Content = file;
                       panel.Children.Add(label);

                       DockPanel.SetDock(border, Dock.Top);
                       DockPanel_WorkingItems.Children.Add(border);
                   }
                }
            }
        }
    }
}

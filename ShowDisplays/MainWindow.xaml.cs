﻿using System;
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

namespace ShowDisplays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Get the list of displays connected to this system.
        GetDisplays displays = new GetDisplays();

        public MainWindow()
        {        
            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {          
            int where = 0;
            foreach (var item in displays.displays)
            {
                var control = new Button();
                control.Content = item.Path;
                
                //DisplaysList.Children.Add(control);
                
                MainGrid.Children.Add(control);
                

                //Grid.SetRow(control, where);
            }
        }
    }
}

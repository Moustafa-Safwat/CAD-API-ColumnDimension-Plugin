using ColumnDimensions.ViewMode;
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
using System.Windows.Shapes;

namespace ColumnDimensions.View
{
    /// <summary>
    /// Interaction logic for UserInterface1.xaml
    /// </summary>
    public partial class UserInterface1 : Window
    {
        public UserInterface1 WindowObject { get; set; }
        public UserInterface1()
        {
            InitializeComponent();
            WindowObject = this;
            this.DataContext = new ColDim(WindowObject);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

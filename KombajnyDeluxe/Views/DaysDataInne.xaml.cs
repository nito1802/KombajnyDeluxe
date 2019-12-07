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

namespace KombajnDoPracy
{
    /// <summary>
    /// Interaction logic for DaysDataInne.xaml
    /// </summary>
    public partial class DaysDataInne : Window
    {
        public List<DayDataModel> DayDataModels { get; set; }

        public DaysDataInne(List<DayDataModel> dayDataModels)
        {
            InitializeComponent();

            this.DataContext = this;
            DayDataModels = dayDataModels;
        }
    }
}

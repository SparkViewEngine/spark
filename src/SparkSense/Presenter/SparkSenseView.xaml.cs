using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SparkSense.Presenter
{
    public partial class SparkSenseView : UserControl
    {
        private SparkSensePresenter _presenter;
        public SparkSenseView(SparkSensePresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
            DataContext = _presenter;
            listViewCompletions.SelectionChanged += (s, e) => listViewCompletions.ScrollIntoView(listViewCompletions.SelectedItem);
        }
    }
}

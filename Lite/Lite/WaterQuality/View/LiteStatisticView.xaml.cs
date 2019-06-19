using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
    public partial class LiteStatisticView : UserControl
    {
        public LiteStatisticView()
        {
            InitializeComponent();
            chart.Visibility = Visibility.Collapsed;
        }

    
    }
    // Class to store sales data
    public class SalesInfo
    {
        public DateTime SaleDate { get; set; }
        public int Sales { get; set; }
    }

    // Collection of sales data
    public class SalesInfoCollection : IEnumerable<SalesInfo>
    {
        public IEnumerator<SalesInfo> GetEnumerator()
        {
           // yield return new SalesInfo { SaleDate = DateTime.Parse("12/27/2011", CultureInfo.InvariantCulture), Sales = 10 };

          //  yield return new SalesInfo { SaleDate = DateTime.Parse("12/28/2011", CultureInfo.InvariantCulture), Sales = 3 };

            yield return new SalesInfo { SaleDate = DateTime.Parse("01/01/2010", CultureInfo.InvariantCulture), Sales = 5 };

            yield return new SalesInfo { SaleDate = DateTime.Parse("01/01/2011", CultureInfo.InvariantCulture), Sales = 1 };
            yield return new SalesInfo { SaleDate = DateTime.Parse("04/01/2012", CultureInfo.InvariantCulture), Sales = 2 };
            yield return new SalesInfo { SaleDate = DateTime.Parse("05/01/2015", CultureInfo.InvariantCulture), Sales = 9 };
            yield return new SalesInfo { SaleDate = DateTime.Parse("03/01/2017", CultureInfo.InvariantCulture), Sales = 8 };
            yield return new SalesInfo { SaleDate = DateTime.Parse("01/17/2019", CultureInfo.InvariantCulture), Sales = 0 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SalesInfo>)this).GetEnumerator();
        }
    }
    }

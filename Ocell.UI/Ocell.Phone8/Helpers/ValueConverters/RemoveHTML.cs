using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;
using System.Web;


namespace Ocell
{    
    public class RemoveHTML : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            string what = value.GetType()=="".GetType()?(string)value:value.ToString();
            Regex exp = new Regex("<[^>]+>");
            return exp.Replace(HttpUtility.HtmlDecode(what), (target) => { return ""; });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

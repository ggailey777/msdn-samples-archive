//   Copyright 2011 Glenn Gailey

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Windows.Data;
using SQLPassBrowser.OratorModelNS;

namespace SQLPassBrowser
{   
    public class DateConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (parameter == null)
                {
                    return ((DateTime)value).ToLongDateString();
                }
                else
                {
                    if ((string)parameter == "time")
                    {
                        return ((DateTime)value).ToShortTimeString();
                    }
                    else if ((string)parameter == "date")
                    {
                        return ((DateTime)value).ToShortDateString();
                    }
                    else
                    {
                        return ((DateTime)value).ToLongDateString();
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TruncateTitle : IValueConverter
    {
        string[] delimiter = new string[] { "--" };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string title = (string)value;

            if (title.Contains(delimiter[0]))
            {
                return title.Split(delimiter, StringSplitOptions.None)[0];
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TruncateTitleElipses : IValueConverter
    {
        string[] delimiter = new string[] { "--" };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string title = (string)value;
            int maxLength;

            if (parameter == null || !int.TryParse(parameter.ToString(), out maxLength))
            {
                maxLength = 30;
            }           

            if (title.Length > maxLength)
            {
                return title.Substring(0, maxLength-3) + "...";
            }
            else
            {
                return title;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TruncateAbstract : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int maxLength;

            if (parameter == null || !int.TryParse(parameter.ToString(), out maxLength))
            {
                maxLength = 30;
            }  

            if (value != null && ((string)value).Length > maxLength)
            {
                return ((string)value).Substring(0, maxLength) + "...";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using VPGUI.Models;

namespace VPGUI.Converters
{
    public class TreeItemToHierarchyConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VpTreeEntryViewModel)
            {
                VpTreeEntryViewModel entryModel = value as VpTreeEntryViewModel;

                var entries = new List<VpTreeEntryViewModel>();

                VpTreeEntryViewModel current = entryModel;

                while (current != null)
                {
                    entries.Insert(0, current);

                    current = current.ModelParent;
                }

                return entries;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

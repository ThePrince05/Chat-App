using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client__.Net_.Converters
{
    public class GroupStatusToMessageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = Groups.Count
            // values[1] = SelectedGroup
            int groupCount = (int)(values[0] ?? 0);
            object selectedGroup = values[1];

            if (groupCount == 0)
                return "Please create a new group";
            if (groupCount > 0 && selectedGroup == null)
                return "Please select a group";

            return ""; // no message
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

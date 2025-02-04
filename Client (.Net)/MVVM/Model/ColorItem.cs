using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client__.Net_.MVVM.Model
{
    public class ColorItem
    {
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }
        public string HexValue => Color.Color.ToString(); // Converts to hex

        public ColorItem(string name, SolidColorBrush color)
        {
            Name = name;
            Color = color;
        }
    }


}

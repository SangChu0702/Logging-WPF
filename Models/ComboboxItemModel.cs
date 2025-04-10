using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UARTLogging.Models
{
    public class ComboBoxItemModel<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }

        public ComboBoxItemModel(T value, string display)
        {
            Value = value;
            Display = display;
        }
    }
}

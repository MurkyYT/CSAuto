using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    public static class TypeConvertor
    {
        public static T ConvertPropertyInfoToOriginalType<T>(this PropertyInfo propertyInfo, object parent)
        {
            var source = propertyInfo.GetValue(parent, null);
            var destination = Activator.CreateInstance(propertyInfo.PropertyType);

            foreach (PropertyInfo prop in destination.GetType().GetProperties().ToList())
            {
                var value = source.GetType().GetProperty(prop.Name).GetValue(source, null);
                prop.SetValue(destination, value, null);
            }

            return (T)destination;
        }
    }
}

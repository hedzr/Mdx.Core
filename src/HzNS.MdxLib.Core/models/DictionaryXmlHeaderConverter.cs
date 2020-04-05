using System;
using System.ComponentModel;
using System.Globalization;

namespace HzNS.MdxLib.models
{
    public class DictionaryXmlHeaderConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(DictionaryXmlHeader) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            return destinationType == typeof(string) && value is DictionaryXmlHeader so
                ? so.GeneratedByEngineVersion
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
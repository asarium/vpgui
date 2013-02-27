using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace EnumTools
{
    

    [ContentProperty("DisplayValue")]
    [TypeConverter(typeof(EnumItemTypeConverter))]
    [Serializable()]
    public class EnumItem 
    {
        /// <summary>
        /// Gets or sets the enumeration value.
        /// </summary>
        [Localizability(LocalizationCategory.None,Readability=Readability.Readable,Modifiability=Modifiability.Unmodifiable)]
        public Object Value { get; set; }

        /// <summary>
        /// Gets or sets the value to be displayed for the enumeration value.
        /// </summary>
        [Localizability(LocalizationCategory.None)]
        public Object DisplayValue { get; set; }

        private String m_text;

        /// <summary>
        /// Gets or sets the text to be shown for this enumeration value
        /// </summary>
        /// <remarks>
        /// If this property is not set explicitly the DisplayValue's textual value (returned by calling ToString)
        /// will be returned. If DisplayValue is not set to a non-null value the Values textual value will will be
        /// returned.
        /// </remarks>
        [Localizability(LocalizationCategory.Text)]
        public String Text
        {
            get {
                return m_text ?? ((DisplayValue != null) ? DisplayValue.ToString() : ((Value != null ? Value.ToString() : null)));
            }
            set { m_text = value; }
        }

        /// <summary>
        /// Gets a value determing if the Text property has been explicitly set or not.
        /// </summary>
        public bool HasText { get { return m_text != null; } }

        public override string ToString()
        {
            return Text;
        }
    }

    
}

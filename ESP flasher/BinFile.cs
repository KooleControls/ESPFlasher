using BasPriveLIB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ESP_flasher
{
    public class BinFile : PropertySensitive
    {
        private static Dictionary<string, int> _CharSpaces = new Dictionary<string, int>();
        private int _Address = 0;
        private string _File = "Select file...";

        [TypeConverter(typeof(AddressConverter))]
        public int Address { get { return GetPar(_Address); } set { SetPar(ref _Address, value); } }

        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string File { get { return GetPar(_File); } set { SetPar(ref _File, value); } }


        private Dictionary<string, int> CharSpaces { get { return GetPar(_CharSpaces); } set { SetPar(ref _CharSpaces, value); } }


        public override string ToString()
        {
            PropertyInfo[] props = new PropertyInfo[]
            {
                this.GetType().GetProperty(nameof(File)),
                this.GetType().GetProperty(nameof(Address)),
            };

            //

            string result = "";

            foreach (PropertyInfo pi in props)
            {

                switch (pi.Name)
                {
                    case nameof(Address):
                        result += "0x" + ((int)pi.GetValue(this)).ToString("X8");
                        break;
                    default:
                        result += pi.GetValue(this);
                        break;
                }


                result += "    "; //Minimum space between properties


                if (!CharSpaces.ContainsKey(pi.Name))
                    CharSpaces[pi.Name] = result.Length;
                else
                {
                    if (result.Length > CharSpaces[pi.Name])
                    {
                        CharSpaces[pi.Name] = result.Length;
                    }
                }

                result += new string(' ', CharSpaces[pi.Name] - result.Length);
            }

            return result;
        }




        private class AddressConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
            {
                return destType == typeof(string);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                int addr = 0;
                string val = ((string)value).ToLower();

                if (val.Contains("0x"))
                    int.TryParse(val.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture.NumberFormat, out addr);
                else
                    int.TryParse(val, out addr);              
                
                return addr;
            }
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                return "0x" + string.Format("{0:X8}", (int)value);
            }
        }
    }


    




}

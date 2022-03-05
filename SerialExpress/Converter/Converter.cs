using SerialExpress.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SerialExpress.Converter
{
    // InputString to SerialPortStream
    public class StringToBytesConverter : IValueConverter
    {
        public static byte[] Convert(string value)
        {
            var bytes = new List<byte>();
            for (int i = 0; i < value.Length; ++i)
            {
                if (value[i] == 0x5c || value[i] == 0xa5)
                {
                    i++;
                    if (i < value.Length)
                    {
                        switch (value[i])
                        {
                            case 'x':   /* hex */
                                if (i + 2 < value.Length)
                                {
                                    bytes.Add(System.Convert.ToByte(new string(value[i], 2), 16));
                                    i += 2;
                                }
                                break;
                            case 'r':   /* CR */
                                bytes.Add((byte)0x0d);
                                break;
                            case 'n':   /* LF */
                                bytes.Add((byte)0x0a);
                                break;
                            case 't':   /* TAB */
                                break;
                            case (char)0x5c:    /* back slash */
                            case (char)0xa5:    /* yen */
                                bytes.Add((byte)0xa5);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    bytes.Add((byte)value[i]);
                }
            }
            return bytes.ToArray();
        }
        public static string ConvertBack(byte[] value)
        {
            var sb = new StringBuilder();
            for (int i=0;i<value.Length ;++i )
            {
                int data = value[i];
                if (data < 0) break;
                if (data < 0x20 || 0x7e < data)
                {
                    sb.Append('¥');
                    switch (data)
                    {
                        case 0x0d:  /* CR */
                            sb.Append('r');
                            break;
                        case 0x0a:  /* LF */
                            sb.Append('n');
                            break;
                        case 0x09:  /* TAB */
                            sb.Append('t');
                            break;
                        case 0x5c:  /* back slash*/
                        case 0xa5:  /* yen*/
                            sb.Append('¥');
                            break;
                        default:
                            sb.Append('x');
                            sb.Append(data.ToString("X02"));
                            break;
                    }
                }
                else
                {
                    sb.Append((char)data);
                }
            }
            return sb.ToString();
        }
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is not string) throw new ArgumentException($"value is null or not string, value:{value}");
            return Convert((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[]) throw new ArgumentException($"value is not byte[], value:{value}");
            return ConvertBack((byte[])value);
        }
    }
    public class BytesToStringConverter : IValueConverter
    {
        public static string Convert(byte[] value)
        {
            var sb = new StringBuilder(0x100);

            for (int i=0;i<value.Length ;++i )
            {
                int data = value[i];
                if (data < 0) break;
                if ((data < 0x20 || 0x7e < data) && data != 0x09/*TAB*/)
                {
                    //sb.Append("[" + data.ToString("X02") + "]");
                    if (data == 0x1b && (value[i + 1] == '['))
                    {
                        while (value[++i] != 'm') ;
                    }
                }
                else
                {
                    sb.Append((char)data);
                }
            }
            return sb.ToString();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[]) throw new ArgumentException($"value is not byte[], value:{value}");
            if (parameter is not string) throw new ArgumentException($"parameter is not string, parameter:{parameter}");
            return Convert((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BytesToHexStringConverter : IValueConverter
    {
        public static string Convert(byte[] value)
        {
            return BitConverter.ToString(value).Replace('-', ' ');
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is not byte[]) throw new ArgumentException($"value is null or not byte[], value:{value}");
            return Convert((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Data;
using System.Linq;
using System.Reflection;



namespace CmdArgParser
{
    public static class CommandlineParser
    {

        public static void ShowHelp<T>()
        {
            var properties = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<Option>() != null);

            Console.WriteLine("Usage:");
            Console.WriteLine("\t" + System.AppDomain.CurrentDomain.FriendlyName + " [options]");
            Console.WriteLine("Options:");
            foreach (var p in properties)
            {
                Option o = p.GetCustomAttribute<Option>();
                Console.WriteLine("\t-" + o.ShortOption + " \t" + o.HelpText );
            }
        }

        public static T Parse<T>()
        {
            T result = Activator.CreateInstance<T>();
            string[] args = Environment.GetCommandLineArgs();

            PropertyInfo pi = null;

            var properties = typeof(T).GetProperties().Where(p=>p.GetCustomAttribute<Option>() != null);

            foreach (string arg in args)
            {

                if (arg.StartsWith("--"))
                    pi = properties.FirstOrDefault(p => p.GetCustomAttribute<Option>().LongOption == arg.Substring(2));
                else if (arg.StartsWith("-"))
                    pi = properties.FirstOrDefault(p => p.GetCustomAttribute<Option>().ShortOption == arg[1]);
                else
                {
                    PopulateProperty(pi, result, arg);
                    pi = null;
                }
            }

            PopulateProperty(pi, result);

            return result;
        }

        static void PopulateProperty<T>(PropertyInfo pi, T obj, string val = "")
        {
            if (pi != null && obj != null)
            {
                if (pi.PropertyType == typeof(string))
                {
                    pi.SetValue(obj, val.Trim('"'));
                }
                if (pi.PropertyType == typeof(bool))
                {
                    pi.SetValue(obj, true);
                }
            }
        }

    }

}


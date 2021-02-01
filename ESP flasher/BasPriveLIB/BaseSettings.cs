using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BasPriveLib
{
    /// <summary>
    /// Used to create settings that are static and therefore accessable across the whole project.
    /// </summary>
    /// <typeparam name="T1">The type of the settings object.</typeparam>
    public class BaseSettings<T1>
    {
        public static readonly string defaultAppFolder = $"/data/{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}";
        public static readonly string defaultSettingsFile = Path.Combine(defaultAppFolder, "settings.json");
        private static Dictionary<string, object> fields = new Dictionary<string, object>();
        private readonly static Serializer serializer = new JSON();

        /// <summary>
        /// Save the current state of the settings to a file.
        /// </summary>
        /// <param name="file">Path to file</param>
        public static void Save(string file)
        {
            CreateDirIfNotExists(Path.GetDirectoryName(file));
            using (Stream stream = File.Open(file, FileMode.Create, FileAccess.Write))
                Save(stream);
        }

        /// <summary>
        /// Load the settings from a file.
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="createIfNotExist">When true, a new file with default values will be created  when the file doens't exist. Otherwise this will throw an exception if the file doensn't exist.</param>
        public static void Load(string file, bool createIfNotExist = false)
        {
            if (File.Exists(file))
            {
                using (Stream stream = File.Open(file, FileMode.Open, FileAccess.Read))
                    Load(stream);
            }
            else
            {
                if (createIfNotExist)
                    CreateDefaultSettings(file);
                else
                    throw new System.Exception($"File not found '{file}'");
            }
        }

        /// <summary>
        /// Save the settings to a stream.
        /// </summary>
        /// <param name="stream"></param>
        public static void Save(Stream stream)
        {
            serializer.Serialize(fields, stream);
        }

        /// <summary>
        /// Load the settings from a stream.
        /// </summary>
        /// <param name="stream"></param>
        public static void Load(Stream stream)
        {
            fields = serializer.Deserialize<Dictionary<string, object>>(stream);
            if (fields == null)
                fields = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create a file with the default settings.
        /// </summary>
        /// <param name="file">Path to file.</param>
        public static void CreateDefaultSettings(string file)
        {
            var v = typeof(T1).GetProperties();

            foreach (var pi in v)
                pi.GetValue(null);

            CreateDirIfNotExists(Path.GetDirectoryName(file));
            using (Stream stream = File.Open(file, FileMode.Create, FileAccess.Write))
                Save(stream);
        }


        /// <summary>
        /// Set the value of a property.
        /// </summary>
        /// <typeparam name="T2">Type of the property to set.</typeparam>
        /// <param name="value">Value of the property to set.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        protected static void SetPar<T2>(T2 value, [CallerMemberName] string propertyName = null)
        {
            lock (fields)
            {
                fields[propertyName] = value;
            }

        }



        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <typeparam name="T2">Type of the property to get.</typeparam>
        /// <param name="defVal">The default value to use if the property has no value.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns></returns>
        protected static T2 GetPar<T2>(T2 defVal = default(T2), [CallerMemberName] string propertyName = null)
        {
            lock (fields)
            {
                T2 val = defVal;
                try
                {
                    if (!fields.ContainsKey(propertyName))
                        fields[propertyName] = defVal;

                    if (val.GetType() == fields[propertyName].GetType())
                        val = (T2)fields[propertyName];
                    else
                        val = (T2)Convert.ChangeType(fields[propertyName], typeof(T2));



                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return val;
            }
        }


        private static void CreateDirIfNotExists(string dir)
        {
            if (dir != "")
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }


    }
}


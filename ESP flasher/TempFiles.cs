using System.IO;

namespace ESP_flasher
{
    public static class TempFiles
    {
        public static string PrepTempFolder(string folder)
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    if (Directory.Exists(folder))
                        Directory.Delete(folder, true);
                    Directory.CreateDirectory(folder);
                    done = true;
                }
                catch
                {

                }
            }
            return folder;
        }

        public static void RemoveTempFolder(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }
}

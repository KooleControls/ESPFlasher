using ESP_Flasher.Models;
using ESP_Flasher.Parsers;
using ESP_Flasher.UIBinders;
using System.IO.Compression;
using System.IO;
using System.Windows.Forms;

namespace ESP_Flasher.Services
{
    public class ArchiveService
    {
        private readonly string _archiveFilter = "Firmware archive|*.kczip";

        // Load the archive from the file dialog
        public FirmwareArchive? LoadArchive()
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = _archiveFilter;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return null;

            using FileStream fileStream = File.OpenRead(openFileDialog.FileName);
            FirmwareArchiveLoader archiveLoader = new FirmwareArchiveLoader();
            FirmwareArchive? archive = archiveLoader.Parse(fileStream);
            if (archive != null)
                archive.ZipFile = openFileDialog.FileName;
            return archive;
        }

        public async Task UseFileStream(FirmwareArchive archive, string fileName, Func<Stream, long, Task> streamAction)
        {
            using FileStream fileStream = File.OpenRead(archive.ZipFile);
            using ZipArchive archiveZip = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
            ZipArchiveEntry? entry = archiveZip.GetEntry(fileName) ?? throw new Exception("File not found");

            using Stream entryStream = entry.Open();
            using MemoryStream copyStream = new MemoryStream();
            entryStream.CopyTo(copyStream); 
            copyStream.Position = 0;
            // Invoke the action on the opened stream
            await streamAction(copyStream, entry.Length);
        }





    }

} 


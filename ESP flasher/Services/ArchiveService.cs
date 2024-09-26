using ESP_Flasher.Models;
using ESP_Flasher.Parsers;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Web;

namespace ESP_Flasher.Services
{
    public class ArchiveService
    {
        private readonly ILogger<ArchiveService> _logger;
        private readonly FirmwareArchiveLoader _zipLoader;
        private readonly BuildFolderArchiveLoader _buildLoader;

        public ArchiveService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ArchiveService>();
            _zipLoader = new FirmwareArchiveLoader(loggerFactory);
            _buildLoader = new BuildFolderArchiveLoader(loggerFactory);
        }

        // Load archive from ZIP
        public async Task<FirmwareArchive?> LoadFromZip(string zipFile, CancellationToken token = default)
        {
            return await _zipLoader.LoadFromZip(zipFile, token);
        }

        // Load archive from Build Directory
        public async Task<FirmwareArchive?> LoadFromBuildDirectory(string argsFilePath, CancellationToken token = default)
        {
            return await _buildLoader.LoadFromBuildDirectory(argsFilePath, token);
        }



        public async Task SaveArchive(Stream stream, FirmwareArchive archive)
        {
        
        }
        
        public async Task SaveArchiveHex(Stream stream, FirmwareArchive archive)
        {
        
        }


    }

}


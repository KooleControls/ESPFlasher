using ESP_Flasher.Models;
using ESP_Flasher.Parsers;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Web;

namespace ESP_Flasher.Services
{
    public class ArchiveService
    {
        private readonly ILogger<ArchiveService> _logger;
        private readonly ZipArchiveLoader _zipLoader;
        private readonly ZipArchiveSaver _zipSaver;
        private readonly BuildFolderLoader _buildLoader;
        private readonly IntelHexSaver _intelHexSaver;
        private readonly PartitionTableExtractor _partitionTableExtractor;
        private readonly AppHeaderExtractor _appHeaderExtractor;

        public ArchiveService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ArchiveService>();
            _zipLoader = new ZipArchiveLoader(loggerFactory);
            _zipSaver = new ZipArchiveSaver(loggerFactory);
            _buildLoader = new BuildFolderLoader(loggerFactory);
            _partitionTableExtractor = new PartitionTableExtractor(loggerFactory);
            _appHeaderExtractor = new AppHeaderExtractor(loggerFactory);
            _intelHexSaver = new IntelHexSaver(loggerFactory, _appHeaderExtractor);
        }

        public async Task<FirmwareArchive?> LoadFromZip(string zipFile, CancellationToken token = default)
        {
            return await _zipLoader.LoadArchiveAsync(zipFile, token);
        }

        public async Task<FirmwareArchive?> LoadFromBuildDirectory(string argsFilePath, CancellationToken token = default)
        {
            return await _buildLoader.LoadArchiveAsync(argsFilePath, token);
        }

        public async Task SaveArchive(FirmwareArchive archive, string zipFile, CancellationToken token = default)
        {
            await _zipSaver.SaveArchiveAsync(zipFile, archive, token);
        }

        public async Task SaveApplicationIntelHex(FirmwareArchive archive, string zipFile, CancellationToken token = default)
        {
            await _intelHexSaver.SaveApplicationAsync(zipFile, archive, token);
        }

        public async Task SaveArchiveIntelHex(FirmwareArchive archive, string zipFile, CancellationToken token = default)
        {
            await _intelHexSaver.SaveArchiveAsync(zipFile, archive, token);
        }

        public async Task<PartitionTable?> ExtractPartitionTable(FirmwareArchive archive, CancellationToken token = default)
        {
            return await _partitionTableExtractor.ExtractTableAsync(archive, token);
        }

        public async Task<AppHeader?> ExtractAppHeader(FirmwareArchive archive, CancellationToken token = default)
        {
            return await _appHeaderExtractor.ExtractHeaderAsync(archive, token);
        }

    }

}


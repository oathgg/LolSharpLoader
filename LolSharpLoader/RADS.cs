using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace LolSharpLoader
{
    public class RADS
    {
        private Settings _settings;

        public RADS(Settings settings)
        {
            _settings = settings;
        }

        public void DownloadClients()
        {
            string releaseList = DownloadReleaseList();

            using (StreamReader stream = new StreamReader(releaseList))
            {
                string[] versions = stream.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Parallel.ForEach(versions, DownloadAndDecompress);
            }
        }

        private void DownloadAndDecompress(string version)
        {
            string targetPath = $@"{_settings.ClientType.ToString()}\{version}";

            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            WebClient wc = new WebClient();

            try
            {
                string compressedFileName = $@"{targetPath}\LeagueOfLegends.compressed";
                wc.DownloadFile(new Uri(_settings.BaseUrl + version + _settings.ExePath), compressedFileName);
                DecompressFile(compressedFileName, ".exe");

                Console.WriteLine($"[V] Downloaded {compressedFileName}");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[X] Failed to download {version + _settings.ExePath}");
                Console.ResetColor();
            }

            wc.Dispose();
        }

        private void DecompressFile(string compressedFile, string extension)
        {
            byte[] fileInBytes = File.ReadAllBytes(compressedFile);
            if (fileInBytes[0] == 0x78 && fileInBytes[1] == 0x9C)
            {
                // We skip the first 2 bytes as these are only needed for the header.
                Stream byteStreamOriginal = new MemoryStream(fileInBytes, 2, fileInBytes.Length - 2);
                using (DeflateStream decompressionStream = new DeflateStream(byteStreamOriginal, CompressionMode.Decompress))
                {
                    string currentFileName = compressedFile;
                    string decompressedFile = currentFileName.Replace(".compressed", extension);
                    using (FileStream decompressedFileStream = File.Create(decompressedFile))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                    File.Delete(compressedFile);
                }
            }
        }

        private string DownloadReleaseList()
        {
            string targetPath = _settings.ClientType.ToString();
            string targetFile = $@"{targetPath}\{_settings.ListFileName}";

            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(new Uri(_settings.BaseUrl + _settings.ListFileName), targetFile);

                wc.Dispose();
            }
            catch
            {
                throw new FileNotFoundException(_settings.BaseUrl + _settings.ListFileName);
            }

            if (!File.Exists(targetFile))
                throw new FileNotFoundException();

            return targetFile;
        }
    }
}
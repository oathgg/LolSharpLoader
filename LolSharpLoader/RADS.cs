﻿using System;
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
            string compressedFile = DownloadCompressedFile(version, "leagueoflegends");
            DecompressFile(compressedFile, ".exe");

            compressedFile = DownloadCompressedFile(version, "stub");
            DecompressFile(compressedFile, ".dll");
        }

        private void DecompressFile(string compressedFile, string extension)
        {
            string decompressedFile = "";
            byte[] fileInBytes = File.ReadAllBytes(compressedFile);
            if (fileInBytes[0] == 0x78 && fileInBytes[1] == 0x9C)
            {
                // We skip the first 2 bytes as these are only needed for the header.
                Stream byteStreamOriginal = new MemoryStream(fileInBytes, 2, fileInBytes.Length - 2);
                using (DeflateStream decompressionStream = new DeflateStream(byteStreamOriginal, CompressionMode.Decompress))
                {
                    string currentFileName = compressedFile;
                    decompressedFile = currentFileName.Replace(".compressed", extension);
                    using (FileStream decompressedFileStream = File.Create(decompressedFile))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("-> {0}", decompressedFile);
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

        private string DownloadCompressedFile(string version, string fileName)
        {
            string targetPath = $@"{_settings.ClientType.ToString()}\{version}";

            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            string compressedFileName = $@"{targetPath}\{fileName}.compressed";
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(new Uri(_settings.BaseUrl + version + _settings.UrlPath), compressedFileName);

                wc.Dispose();
            }
            catch
            {
                // Delete the directory if we created it and any files in the directory.
                if (Directory.Exists(targetPath))
                    Directory.Delete(targetPath, true);
            }

            return compressedFileName;
        }
    }
}
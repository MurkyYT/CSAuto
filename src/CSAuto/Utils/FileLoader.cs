using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Murky.Utils
{
    public class FileLoader
    {
        struct FileData
        {
            public string Name;
            public long Size;
            public long Offset;
        }

        public const string FileMagic = "MPF\0";
        public string Path { get; private set; }

        readonly Dictionary<string, Stream> cachedFiles = new Dictionary<string, Stream>();
        readonly Dictionary<string, FileData> filesData = new Dictionary<string, FileData>();

        public FileLoader(string path)
        {
            Path = path;
            Stream fileData = ReadFileData(path);
            ParseFileData(fileData);
        }

        private void ParseFileData(Stream fileData)
        {
            byte[] buffer = new byte[4];
            fileData.Read(buffer, 0, 4);
            string magic = Encoding.UTF8.GetString(buffer);
            if (magic != FileMagic)
                return;

            while(fileData.Position != fileData.Length)
            {
                string name = "";
                byte read;
                do
                {
                    read = (byte)fileData.ReadByte();
                    if (read == 0) break;
                    name += (char)read;
                }
                while (read != 0);

                buffer = new byte[8];
                fileData.Read(buffer, 0, 8);
                long size = BitConverter.ToInt64(buffer, 0);
                filesData[name] = new FileData()
                {
                    Name = name,
                    Size = size,
                    Offset = fileData.Position
                };
                fileData.Position += size;
            }
        }

        private Stream ReadFileData(string path)
        {
            MemoryStream mso = new MemoryStream();
            using (FileStream msi = File.OpenRead(path))
            {
                using (GZipStream gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    byte[] bytes = new byte[4096];

                    int cnt;

                    while ((cnt = gs.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mso.Write(bytes, 0, cnt);
                    }
                }

                mso.Position = 0;

                return mso;
            }
        }

        public Stream LoadFile(string name)
        {
            if (!filesData.ContainsKey(name))
                return null;

            Stream fileData = ReadFileData(Path);

            FileData file = filesData[name];

            fileData.Position = file.Offset;

            if (cachedFiles.ContainsKey(name))
            {
                cachedFiles[name].Position = 0;
                return cachedFiles[name];
            }

            MemoryStream memoryStream = new MemoryStream();
            while(memoryStream.Length < file.Size)
            {
                byte[] buffer = new byte[4096];
                int read = fileData.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                memoryStream.Write(buffer, 0, read);
            }

            cachedFiles[name] = memoryStream;

            fileData.Dispose();

            return memoryStream;
        }

        public List<Stream> LoadFiles(List<string> names)
        {
            Stream fileData = ReadFileData(Path);
            List<Stream> files = new List<Stream>();
            foreach (string name in names)
            {
                if (!filesData.ContainsKey(name))
                {
                    files.Add(null);
                    continue;
                }

                FileData file = filesData[name];
                fileData.Position = file.Offset;

                if (cachedFiles.ContainsKey(name))
                {
                    cachedFiles[name].Position = 0;
                    files.Add(cachedFiles[name]);
                    continue;
                }

                MemoryStream memoryStream = new MemoryStream();
                while (memoryStream.Length < file.Size)
                {
                    byte[] buffer = new byte[4096];
                    int read = fileData.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;
                    memoryStream.Write(buffer, 0, read);
                }

                cachedFiles[name] = memoryStream;

                files.Add(memoryStream);
            }

            fileData.Dispose();

            return files;
        }
    }
}

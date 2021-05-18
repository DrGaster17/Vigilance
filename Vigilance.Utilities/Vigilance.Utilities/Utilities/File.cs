using System.Collections.Generic;
using System.IO;

namespace Vigilance.Utilities
{
    public class File
    {
        private string _path;

        public File(string path)
        {
            _path = path;
        }

        public void Delete() => System.IO.File.Delete(_path);
        public void Create() => System.IO.File.Create(_path).Close();
        public void Copy(string destinationPath) => System.IO.File.Copy(_path, destinationPath);
        public void Decrypt() => System.IO.File.Decrypt(_path);
        public void Encrypt() => System.IO.File.Encrypt(_path);

        public void Append(IEnumerable<string> lines) => System.IO.File.AppendAllLines(_path, lines);
        public void Append(object content) => System.IO.File.AppendAllText(_path, content.ToString());

        public FileStream CreateStream() => System.IO.File.Create(_path);
        public FileStream Read() => System.IO.File.OpenRead(_path);

        public void Write(object content) => System.IO.File.WriteAllText(_path, content.ToString());

        public bool Exists => System.IO.File.Exists(_path);
        public string Text => System.IO.File.ReadAllText(_path);

        public StreamWriter Writer => System.IO.File.AppendText(_path);
    }
}
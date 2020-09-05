using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordHookBotPlus
{
    internal class Storage<T, I>
    {
        public readonly string FileName;
        private Dictionary<I, T> _data = new Dictionary<I, T>();

        public Storage(string fileName)
        {
            if (!File.Exists(fileName))
                File.Create(fileName);
            FileName = fileName;
            ReadFromFile();
        }

        public void Upsert(T item, I id)
        {
            _data[id] = item;
            WriteToFile();
        }

        internal void WriteToFile()
        {
            File.WriteAllBytes(FileName, Compressed());
        }

        internal void ReadFromFile()
        {
            string decompressed = Decompressed();
            _data = decompressed.Length == 0
                ? new Dictionary<I, T>()
                : JsonConvert.DeserializeObject<Dictionary<I, T>>(decompressed);
        }

        public T GetObjectFromId(I id, T @default)
        {
            ReadFromFile();
            if (_data == null || _data.Count == 0) _data = new Dictionary<I, T>();
            if (_data.ContainsKey(id)) return _data[id];
            else return @default;
        }

        public List<T> GetAllObjects()
        {
            return _data.Values.ToList();
        }

        public byte[] Compressed()
        {
            string text = JsonConvert.SerializeObject(_data);
            byte[] raw = Encoding.Unicode.GetBytes(text);
            MemoryStream mem = new MemoryStream(raw);
            MemoryStream o = new MemoryStream();
            GZip.Compress(mem, o, true);
            return o.ToArray();
        }

        public string Decompressed()
        {
            MemoryStream mem = new MemoryStream();
            GZip.Decompress(File.OpenRead(FileName), mem, true);
            return Encoding.Unicode.GetString(mem.ToArray());
        }
    }

    public class SingleStorage<T> : IEnumerable<T>
    {
        public readonly string FileName;
        HashSet<T> _data = new HashSet<T>();

        public SingleStorage(string fileName)
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }

            FileName = fileName;
            ReadFromFile();
        }

        public void Upsert(T item)
        {
            _data.Add(item);
            WriteToFile();
        }

        internal bool WriteToFile()
        {
            File.WriteAllBytes(FileName, Compressed());
            return true;
        }

        internal void ReadFromFile()
        {
            string decompressed = Decompressed();
            _data = decompressed.Length == 0
                ? new HashSet<T>()
                : JsonConvert.DeserializeObject<HashSet<T>>(decompressed);
        }

        internal bool Remove(T item)
        {
            return _data.Remove(item) && WriteToFile();
        }

        public List<T> GetAllObjects()
        {
            return _data.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) _data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _data).GetEnumerator();
        }

        public bool Has(T t)
        {
            ReadFromFile();
            return _data?.Contains(t) ?? false;
        }

        public byte[] Compressed()
        {
            string text = JsonConvert.SerializeObject(_data);
            byte[] raw = Encoding.Unicode.GetBytes(text);
            MemoryStream mem = new MemoryStream(raw);
            MemoryStream o = new MemoryStream();
            GZip.Compress(mem, o, true);
            return o.ToArray();
        }

        public string Decompressed()
        {
            MemoryStream mem = new MemoryStream();
            GZip.Decompress(File.OpenRead(FileName), mem, true);
            return Encoding.Unicode.GetString(mem.ToArray());
        }
    }
}
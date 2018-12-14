using System.IO;

namespace HChan.Parser
{
    public class Image
    {
        public string Name { get; set; }

        public byte[] Bytes { get; private set; }

        public Image(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }

        public void SaveToFile(string filePath)
        {
            File.WriteAllBytes(filePath, this.Bytes);
        }

        public void SaveToDirectory(string directoryPath)
        {
            string filePath = Path.Combine(directoryPath, $"{this.Name}.jpg");
            File.WriteAllBytes(filePath, this.Bytes);
        }
    }
}

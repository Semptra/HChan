namespace HChan.Parser
{
    public class Manga
    {
        public string Name { get; set; }

        public string Url { get; private set; }

        public Manga(string name, string url)
        {
            this.Name = name;
            this.Url = url;
        }
    }
}

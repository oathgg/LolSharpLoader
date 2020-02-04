namespace LolSharpLoader
{
    public class Settings
    {
        public string BaseUrl { get; set; }
        public string ListFileName { get; set; }
        public string ExePath { get; set; }
        public string StubPath { get; set; }
        public ClientType ClientType { get; set; }
    }

    public enum ClientType
    {
        Mac,
        Windows
    }
}
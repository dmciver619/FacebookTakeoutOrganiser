public interface ILocatedFile
{
    public string URI { get; set; }
    public long CreateTimestamp { get; set; }

    public bool IsLocalFile()
    {
        return !URI.StartsWith("https");
    }
}
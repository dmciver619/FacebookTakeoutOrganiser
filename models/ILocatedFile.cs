public interface ILocatedFile
{
    public string URI { get; set; }
    public long CreateTimestamp { get; set; }
}
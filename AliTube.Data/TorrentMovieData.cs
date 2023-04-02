namespace AliTube.Data
{
    public class TorrentMovieData
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public byte[] TorrentData { get; set; }
        public long Size { get; set; }
        public string Quality { get; set; }
    }
}

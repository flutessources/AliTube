namespace AliTube.Data
{
    public class MovieData
    {
        public int Id { get; set; }
        public string Owner { get; set; } 
        public int OwnerId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }   
        public string Description { get; set; }
        public string Genres { get; set; }
        public string ImageUrl { get; set; }
        public float Rating { get; set; }
        
    }
}

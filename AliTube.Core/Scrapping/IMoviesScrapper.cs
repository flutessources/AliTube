using AliTube.Data;

namespace AliTube.Core.Scrapping
{
    public interface IMoviesScrapper
    {
        public void Scrapping(Action<int, int> progress);
        public Action<bool> OnFinish { get; set; }
        public void Stop();
    }
}

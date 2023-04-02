using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AliTube.Data;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;
using Newtonsoft.Json;

namespace AliTube.Core.Scrapping.YTS
{
    public class YTSMoviesScrapper : IMoviesScrapper
    {
        public Action<bool> OnFinish { get; set; }

        private bool m_isStarted;

        public void Scrapping(Action<int, int> progress)
        {
			Task.Factory.StartNew(async () =>
            {
                try
                {
                    m_isStarted = true;
                    int movieCounts = await GetMoviesCount();
                    int limit = 50;
                    int page = 1;
                    int maxPages = movieCounts / limit;

                    while (page < maxPages && m_isStarted)
                    {
                        var ytsMovies = await GetMovies($"?page={page}&limit={limit}");

                        if (ytsMovies == null)
                        {
                            return;
                        }

                        foreach (var m in ytsMovies)
                        {
                            MovieData movie = ParseMovie(m);
                            movie.Owner = "YTS";
                            movie.OwnerId = m.id;
                            var movieCreated = await Database.MovieDatabase.AddMovie(movie);
                            if (movieCreated == null)
                            {
                                Console.WriteLine("Fail on create movie");
                                continue;
                            }

                            foreach (var t in m.torrents)
                            {
                                TorrentMovieData? torrent = await ParseTorrent(m, t);

                                if (torrent == null)
                                {
                                    Console.WriteLine("Fail on parse torrent");
                                    continue;
                                }

                                torrent.MovieId = movieCreated.Id;
                                var torrentCreated = await Database.MovieDatabase.AddTorrent(torrent);

                                if (torrentCreated == null)
                                {
                                    Console.WriteLine("Fail on create torrent");
                                }
                            }
                        }

                        page++;
                        Console.WriteLine("Invoke on progress");
                        progress?.Invoke(page, maxPages);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error " + ex);
                }

                OnFinish?.Invoke(true);

            }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            m_isStarted = false;
        }
        private static async Task<YTSMovie?> GetMovie(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://yts.torrentbay.to/api/v2/movie_details.json?movie_id=" + id);
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var ytsMessage = JsonConvert.DeserializeObject<YTSMessage<YTSMovie>>(data);

                    return ytsMessage?.data;
                }
            }

            return null;
        }

        private static async Task<int> GetMoviesCount()
        {
            using (HttpClient client = new())
            {
                Console.WriteLine("Get movies count 1");

                List<YTSMovie> movies = new List<YTSMovie>();
                var data = await client.GetStringAsync("https://yts.torrentbay.to/api/v2/list_movies.json");

                if (data != null)
                {
					Console.WriteLine("Get movies count 2");

					var ytsMessage = JsonConvert.DeserializeObject<YTSMessage<YTSListMovies>>(data);

                    if (ytsMessage != null)
                    {
						Console.WriteLine("Get movies count 3 " + ytsMessage.data.movie_count);
						return ytsMessage.data.movie_count;
                    }
                }
            }

            return 0;
        }

        private static async Task<List<YTSMovie>?> GetMovies(string query)
        {
            using (HttpClient client = new())
            {
                List<YTSMovie> movies = new List<YTSMovie>();
                var data = await client.GetStringAsync("https://yts.torrentbay.to/api/v2/list_movies.json" + query);

                if (data != null)
                {
                    var ytsMessage = JsonConvert.DeserializeObject<YTSMessage<YTSListMovies>>(data);

                    return ytsMessage?.data.movies;
                }
            }

            return null;
        }

        private static async Task<TorrentMovieData?> ParseTorrent(YTSMovie movie, YTSTorrent ytsTorrent)
        {
            Console.WriteLine("Parse torrent");

            byte[]? torrentData = await GetTorrentData(ytsTorrent.url);
            if (torrentData == null)
                return null;

            var torrent = new TorrentMovieData()
            {
                Quality = ytsTorrent.quality,
                Size = ytsTorrent.size_bytes,
                TorrentData = torrentData//TorrentToMagnetLink(torrentData),
            };

            return torrent;
        }

		public static string TorrentToMagnetLink(byte[] torrentFileBytes)
		{
			var parser = new BencodeParser(Encoding.Default); // Default encoding is Encoding.UTF8, but you can specify another if you need to
			Torrent torrent = parser.Parse<Torrent>(torrentFileBytes);
			string magnetLink = torrent.GetMagnetLink();

            Console.WriteLine("magnetlink : " + magnetLink);

            return magnetLink;
		}
		private static async Task<byte[]?> GetTorrentData(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("GetTorrentData start");
                client.BaseAddress = new Uri(url);

                HttpResponseMessage response = await client.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
					Console.WriteLine("GetTorrentData end");
					return data;
                }

				Console.WriteLine("GetTorrentData end");
			}

            return null;
        }

        private static MovieData ParseMovie(YTSMovie ytsMovie)
        {
            var movieData = new MovieData()
            {
                Title = ytsMovie.title,
                Year = ytsMovie.year,
                Rating = ytsMovie.rating,
                Description = ytsMovie.description_full,
                ImageUrl = ytsMovie.large_cover_image,
                Genres = ytsMovie.genres == null ? "" : ytsMovie.genres.Aggregate((a, b) => a.ToLower() + "," + b.ToLower())
            };

            return movieData;
        }
    }
}

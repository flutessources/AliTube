using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AliTube.Data;
using Microsoft.Extensions.Configuration;
using MonoTorrent;

namespace AliTube.Core.Database
{
    public class MovieDatabase
    {
        //private IConfiguration m_configuration;

        public const string SERVER_URL = "https://localhost:5001";

        public static async Task<List<MovieData>?> GetMovies(int page, int limit, MovieOrder? order = null, List<MovieFilter>? filters = null)
        {
            var client = GetClient();
            return await GetMovies(client, page, limit, order, filters);
        }

        public static async Task<List<TorrentMovieData>?> GetTorrents(int movieId)
        {
            var client = GetClient();
            return await GetTorrents(client, movieId);
        }

        public static async Task<MovieData?> AddMovie(MovieData movie)
        {
            var client = GetClient();
            return await AddMovie(client, movie);
        }

        public static async Task<TorrentMovieData?> AddTorrent(TorrentMovieData torrent)
        {
            var client = GetClient();
            return await AddTorrent(client, torrent);
        }

        private static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();

            return client;
        }
        
        private static async Task<List<MovieData>?> GetMovies(HttpClient client, int page, int limit, MovieOrder? order = null, List<MovieFilter>? filters = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, SERVER_URL + $"/Movies");
            message.Content = JsonContent.Create(new GetMoviesMessage() { Page = page, Limit = limit, Order = order, Filters = filters });
            var response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            List<MovieData>? result = await response.Content.ReadFromJsonAsync<List<MovieData>?>();

            return result;
        }

        private static async Task<List<TorrentMovieData>?> GetTorrents(HttpClient client, int movieId)
        {
            var response = await client.GetAsync(SERVER_URL + $"/Torrents/Details?movie_id={movieId}");

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            List<TorrentMovieData>? result = await response.Content.ReadFromJsonAsync<List<TorrentMovieData>?>();

            return result;
        }

        private static async Task<MovieData?> AddMovie(HttpClient client, MovieData movie)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, SERVER_URL + "/Movies");
            message.Content = JsonContent.Create(movie);
            var response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            MovieData? result = await response.Content.ReadFromJsonAsync<MovieData?>();

            return result;
        }

        private static async Task<TorrentMovieData?> AddTorrent(HttpClient client, TorrentMovieData torrent)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, SERVER_URL + "/Torrents");
            message.Content = JsonContent.Create(torrent);
            var response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            TorrentMovieData? result = await response.Content.ReadFromJsonAsync<TorrentMovieData?>();

            return result;
        }
    }
}

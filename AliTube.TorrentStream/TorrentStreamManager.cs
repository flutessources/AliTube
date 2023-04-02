using MonoTorrent.Client;
using MonoTorrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace AliTube.TorrentStream
{
    public static class TorrentStreamManager
    {
        private static Dictionary<string, Stream> m_availableStreams = new();
        private static Dictionary<int, TorrentManager> m_managers = new();

        private static ClientEngine m_engine;

        public static void Init(ClientEngine engine)
        {
            m_engine = engine;
        }
		public static async Task StreamPartOfTorrent(HttpContext context, MagnetLink link, long startPos, long endPos)
		{
			var manager = await m_engine.AddStreamingAsync(link, "downloads");
            await StreamPartOfTorrent(context, manager, link.ToV1Uri().AbsoluteUri, startPos, endPos);
		}

		public static async Task StreamPartOfTorrent(HttpContext context, int movieId, string torrentStr, long startPos, long endPos)
        {
			byte[] torrentBytes = Encoding.UTF8.GetBytes(torrentStr);
            await StreamPartOfTorrent(context, movieId, torrentBytes, startPos, endPos);
		}


		public static async Task StreamPartOfTorrent(HttpContext context, int movieId, byte[] torrentBytes, long startPos, long endPos)
		{
            Torrent torrent;
            bool success = Torrent.TryLoad(torrentBytes, out torrent);

            if (success == false)
            {
                Console.WriteLine("Torrent load failed");
                return;    
            }

            Console.WriteLine("Stream part of torrent " + torrent.Name);

            TorrentManager manager = null;
            if (m_managers.ContainsKey(movieId))
            {
                manager = m_managers[movieId];
            }
            else
            {
                manager = await m_engine.AddStreamingAsync(torrent, "downloads");
                m_managers.Add(movieId, manager);
			}
            
			await StreamPartOfTorrent(context, manager, torrent.Name, startPos, endPos);
		}

		private static async Task StreamPartOfTorrent(HttpContext context, TorrentManager manager, string name, long startPos, long endPos)
        {
            long fileSize = 0;
            byte[] buffer = await StreamPartOfTorrent(manager, name, startPos, endPos, (s) => fileSize = s);

            context.Response.StatusCode = 206;
            context.Response.Headers["Accept-Ranges"] = "bytes";
            context.Response.Headers["Content-Range"] = string.Format($" bytes {startPos}-{endPos - 1}/{fileSize}");
            context.Response.ContentType = "application/octet-stream";
            
            Console.WriteLine("Before write");
            await context.Response.Body.WriteAsync(buffer);
            Console.WriteLine("After write");
        }

        private static async Task<byte[]> StreamPartOfTorrent(TorrentManager manager, string name, long startPos, long endPos, Action<long> fileSize)
        {
			Stream stream = null;
			if (m_availableStreams.ContainsKey(name))
			{
				stream = m_availableStreams[name];
				Console.WriteLine("stream already streamed file");
			}
            
			else
			{
				Console.WriteLine("stream new file");
				stream = await CreateBaseStream(manager);
				m_availableStreams.Add(name, stream);
			}

            return await StreamPartOfTorrent(stream, startPos, endPos, fileSize);
		}


		private static async Task<byte[]> StreamPartOfTorrent(Stream stream, long startPos, long endPos, Action<long> fileSize)
        {
            fileSize?.Invoke(stream.Length);

            long length = endPos - startPos;
            byte[] buffer = new byte[length];

            try
            {
                Console.WriteLine("begin read");
                stream.Seek(startPos, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, (int)length);
                Console.WriteLine("end read");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return buffer;
        }

        private static async Task<Stream> CreateBaseStream(TorrentManager manager)
        {
            var overall = Stopwatch.StartNew();
            var firstPeerFound = Stopwatch.StartNew();
            var firstPeerConnected = Stopwatch.StartNew();
            manager.PeerConnected += (o, e) =>
            {
                if (!firstPeerConnected.IsRunning)
                    return;

                firstPeerConnected.Stop();
                Console.WriteLine(("First peer connected. Time since torrent started: ", firstPeerConnected.Elapsed));
            };
            manager.PeersFound += (o, e) =>
            {
                if (!firstPeerFound.IsRunning)
                    return;

                firstPeerFound.Stop();
                Console.WriteLine(($"First peers found via {e.GetType().Name}. Time since torrent started: ", firstPeerFound.Elapsed));
            };
            manager.PieceHashed += (o, e) =>
            {
                if (manager.State != TorrentState.Downloading)
                    return;

                Console.WriteLine(($"Piece {e.PieceIndex} hashed. Time since torrent started: ", overall.Elapsed));
            };



            await manager.StartAsync();
            Console.WriteLine("Started");
            await manager.WaitForMetadataAsync();
            Console.WriteLine("Metadata founded");

            var largestFile = manager.Files.OrderByDescending(t => t.Length).First();
            await manager.SetFilePriorityAsync(largestFile, Priority.Highest);
            var stream = await manager.StreamProvider.CreateStreamAsync(largestFile, false);

            Console.WriteLine($"Stream created {largestFile.Path}");
            return stream;
        }
    }
}

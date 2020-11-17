using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Utility;
using YoutubeDownloader.Utility.CLI;
using YoutubeExplode;

namespace YoutubeDownloader.Utility.Download
{
    class Downloader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="video"></param>
        /// <param name="outputDirectory"></param>
        public static void DownloadThumbnails(Options options, YoutubeExplode.Videos.Video video, string outputDirectory)
        {
            (string resolution, string url)[] thumbnails = { ("Max Resolution", video.Thumbnails.MaxResUrl), ("High Resolution", video.Thumbnails.HighResUrl), ("Standard Resolution", video.Thumbnails.StandardResUrl), ("Medium Resolution", video.Thumbnails.MediumResUrl), ("Low Resolution", video.Thumbnails.LowResUrl) };

            System.IO.Directory.CreateDirectory(outputDirectory + @"\thumbnails");

            Helper.WriteLine("Downloading Thumbnails associated with " + video.Title, false, options);

            using System.Net.WebClient client = new System.Net.WebClient();
            foreach ((string resolution, string url) in thumbnails)
            {
                if (url == "" || url == null)
                    continue;
                Helper.WriteLine("\t, Downloading Thumbnail in " + resolution, false, options);
                client.DownloadFile(url, outputDirectory + @"\Thumbnails\" + resolution + ".jpg");
            }
        }

        public static async Task DownloadFullVideo(Options options, YoutubeClient youtube, YoutubeExplode.Videos.Video video, string fileName, string outputDirectory, CancellationToken token)
        {

            Helper.WriteLine("Downloading manifest associated with " + video.Id + ", " + video.Title, false, options);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

            Helper.WriteLine("-- Downloading video " + video.Title + ", " + video.Id + " --", true, options);

            await DownloadMuxed(options, youtube, video, fileName, outputDirectory, streamManifest, token);
            if (!options.Fast)
            {
                await DownloadVideo(options, youtube, video, fileName, outputDirectory, streamManifest, token);
                await DownloadAudio(options, youtube, video, fileName, outputDirectory, streamManifest, token);
            }
            Console.WriteLine("\n\n");

        }

        private static async Task DownloadAudio(Options options, YoutubeClient youtube, YoutubeExplode.Videos.Video video, string fileName, string outputDirectory, YoutubeExplode.Videos.Streams.StreamManifest streamManifest, CancellationToken token)
        {
            bool flag = false;
            (string bitrate, YoutubeExplode.Videos.Streams.AudioOnlyStreamInfo)[] audioStreams = { (streamManifest.GetAudioOnly().FirstOrDefault(s => s.Tag == 140).Bitrate.ToString(), streamManifest.GetAudioOnly().FirstOrDefault(s => s.Tag == 140)) };

            foreach ((string bitrate, YoutubeExplode.Videos.Streams.AudioOnlyStreamInfo info) in audioStreams)
            {
                if (info == null)
                {
                    Helper.WriteLine("\t\tCould not download audio stream in bitrate " + bitrate + ", downgrading...", false, options);
                    continue;
                }

                string localOutputDirectory = outputDirectory + @"\Audio " + bitrate;

                Helper.WriteLine("\tDownloading audio \"" + video.Title + "\" in bitrate " + bitrate, true, options);

                System.IO.Directory.CreateDirectory(localOutputDirectory);

                DateTime now = DateTime.Now;
                try
                {
                    await youtube.Videos.Streams.DownloadAsync(info, localOutputDirectory + @"\" + fileName + ".mp4", null, token);
                }
                catch (Exception e)
                {
                    Helper.WriteLine("Error downloading Audio " + bitrate + " - " + fileName + ".mp4, Error: " + e.Message, true, options);
                    Helper.Logger("Error downloading Audio " + bitrate + " - " + fileName + ".mp4, Error: " + e.Message);
                    continue;
                }
                Helper.WriteLine("\tDownload took " + (DateTime.Now - now).TotalMilliseconds.ToString() + "ms", false, options);
                flag = true;
                break;
            }
            if (!flag)
            {
                Helper.WriteLine("\tCould not download any video streams of " + video.Title + ", skipping.", true, options);
                if (options.Logging)
                    Helper.Logger("Could not download video video " + video.Title + ", " + video.Id);
            }
        }

        private static async Task DownloadVideo(Options options, YoutubeClient youtube, YoutubeExplode.Videos.Video video, string fileName, string outputDirectory, YoutubeExplode.Videos.Streams.StreamManifest streamManifest, CancellationToken token)
        {
            bool flag = false;
            (string resolution, YoutubeExplode.Videos.Streams.VideoOnlyStreamInfo)[] videoStreams = { ("1080p", streamManifest.GetVideoOnly().FirstOrDefault(s => s.VideoQualityLabel == "1080p")), ("720p", streamManifest.GetVideoOnly().FirstOrDefault(s => s.VideoQualityLabel == "720p")), ("480p", streamManifest.GetVideoOnly().FirstOrDefault(s => s.VideoQualityLabel == "480p")), ("360p", streamManifest.GetVideoOnly().FirstOrDefault(s => s.VideoQualityLabel == "360p")) };


            foreach ((string resolution, YoutubeExplode.Videos.Streams.VideoOnlyStreamInfo info) in videoStreams)
            {
                YoutubeExplode.Videos.Streams.IStreamInfo streamer = info;
                if (info == null)
                {
                    Helper.WriteLine("\t\tCould not download video stream in resolution " + resolution + ", downgrading...", false, options);
                    continue;
                }

                string localOutputDirectory = outputDirectory + @"\Video " + resolution;

                Helper.WriteLine("\tDownloading bideo \"" + video.Title + "\" in resolution " + resolution, true, options);

                System.IO.Directory.CreateDirectory(localOutputDirectory);

                DateTime now = DateTime.Now;

                try
                {
                    await youtube.Videos.Streams.DownloadAsync(info, localOutputDirectory + @"\" + fileName + ".mp4", null, token);
                }
                catch (Exception e)
                {
                    Helper.WriteLine("Error downloading Video " + resolution + " - " + fileName + ".mp4, Error: " + e.Message, true, options);
                    Helper.Logger("Error downloading Video " + resolution + " - " + fileName + ".mp4, Error: " + e.Message);
                    continue;
                }
                Helper.WriteLine("\tDownload took " + (DateTime.Now - now).TotalMilliseconds.ToString() + "ms", false, options);
                flag = true;
                break;
            }
            if (!flag)
            {
                Helper.WriteLine("\tCould not downloading any video streams of " + video.Title + ", skipping.", true, options);
                if (options.Logging)
                    Helper.Logger("Could not download video video " + video.Title + ", " + video.Id);
            }
        }

        private static async Task DownloadMuxed(Options options, YoutubeClient youtube, YoutubeExplode.Videos.Video video, string fileName, string outputDirectory, YoutubeExplode.Videos.Streams.StreamManifest streamManifest, CancellationToken token)
        {
            bool flag = false;
            (string resolution, YoutubeExplode.Videos.Streams.MuxedStreamInfo)[] muxedStreams = { ("720p", streamManifest.GetMuxed().FirstOrDefault(s => s.VideoQualityLabel == "720p")), ("360p", streamManifest.GetMuxed().FirstOrDefault(s => s.VideoQualityLabel == "360p")) };

            foreach ((string resolution, YoutubeExplode.Videos.Streams.MuxedStreamInfo info) in muxedStreams)
            {
                if (info == null)
                {
                    Helper.WriteLine("\t\tCould not download muxed stream in resolution " + resolution + ", downgrading...", false, options);
                    continue;
                }

                string localOutputDirectory = outputDirectory + @"\Muxed " + resolution;

                Helper.WriteLine("\tDownloading muxed video \"" + video.Title + "\" in resolution " + resolution, true, options);

                System.IO.Directory.CreateDirectory(localOutputDirectory);

                DateTime now = DateTime.Now;
                try
                {
                    await youtube.Videos.Streams.DownloadAsync(info, localOutputDirectory + @"\" + fileName + ".mp4", null, token);
                }
                catch (Exception e)
                {
                    Helper.WriteLine("Error downloading Muxed " + resolution + " - " + fileName + ".mp4, Error: " + e.Message, true, options);
                    Helper.Logger("Error downloading Muxed " + resolution + " - " + fileName + ".mp4, Error: " + e.Message);
                    continue;
                }
                Helper.WriteLine("\tDownload took " + (DateTime.Now - now).TotalMilliseconds.ToString() + "ms", false, options);
                flag = true;
                break;
            }
            if (!flag)
            {
                Helper.WriteLine("\t Could not download any muxed streams of " + video.Title + ", skipping.", true, options);
                if (options.Logging)
                    Helper.Logger("Could not download muxed " + video.Title + ", " + video.Id);
            }
        }
    }
}

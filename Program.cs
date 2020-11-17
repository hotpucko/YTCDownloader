using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

using YoutubeExplode;

using CommandLine;

using YoutubeDownloader.Utility;
using YoutubeDownloader.Utility.CLI;
using YoutubeDownloader.Utility.Download;


namespace YoutubeDownloader
{
    //entry point
    class Program
    {
        static void Main(string[] args)
        {

            CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => {
                try
                {
                    new Program().Run(o).Wait();
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                    return;
                }
            });
                
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run(Options options)
        {
            DateTime start = DateTime.Now;

            /*
             * https://stackoverflow.com/questions/34143202/get-all-videos-from-channel-youtube-api-v3-c-sharp
             * https://github.com/Tyrrrz/YoutubeDownloader
             * https://github.com/Tyrrrz/YoutubeExplode <------------
             * https://github.com/Tyrrrz/YoutubeExplode/issues/300
             */

            /*
             * TODO: 
             *      Swap the video id grabbing from youtube's api to scraping with Tyrrrz/YoutubeExplode || Tyrrrz/YoutubeDownloader to avoid including youtube api key [Done]
             *      
             * 
             */
            var youtube = new YoutubeClient();
            if (!options.LiveStream.Equals(""))
            {
                Console.WriteLine("Press a key to load the url for unus annus final stream.");
                Console.ReadKey();
                var streamURL = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(options.LiveStream);
                Console.WriteLine(streamURL);//await youtube.Videos.Streams.
                //YoutubeExplode.Videos.Video video = await youtube.Videos.
                return;
            }

            /*if (!options.LiveStream.Equals(""))
            {
                Console.WriteLine("Press a key to load the url for unus annus final stream.");
                Console.ReadKey();
                var streamURL = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(options.LiveStream);
                Console.WriteLine(streamURL);//await youtube.Videos.Streams.
                //YoutubeExplode.Videos.Video video = await youtube.Videos.
                return;
            }*/

            //gets all available videos associated with the specified channel, and puts it in oldest first order
            IEnumerable<YoutubeExplode.Videos.Video> videos = (await youtube.Channels.GetUploadsAsync(options.ChannelKey)).Reverse();

            if (videos.Count() <= 0)
            {
                Helper.WriteLine("Could not find any videos associated with the channel.", true, options);
                return;
            }

            string channelDirectory = "";

            //if the output is set to empty, haven't tested if you can actually do that but just in case, we want to create a base directory
            if (options.Output != "")
            {
                channelDirectory = options.Output + @"\" + videos.ElementAt(0).Author;
                if (!System.IO.Directory.Exists(options.Output))
                {
                    Helper.WriteLine("Created Directory " + options.Output, false, options);

                    System.IO.Directory.CreateDirectory(options.Output);
                }

            }
            if (!System.IO.Directory.Exists(channelDirectory))
            {
                Helper.WriteLine("Created Directory " + channelDirectory, false, options);

                System.IO.Directory.CreateDirectory(channelDirectory);
            }

            for (int i = 0; i < videos.Count(); i++)
            {
                YoutubeExplode.Videos.Video video = videos.ElementAt(i);

                string fileName = Helper.FileNameGenerator(i, video.Title, video.UploadDate.DateTime.ToString());
                string outputDirectory = channelDirectory + @"\" + fileName;
                string outputFileName = outputDirectory + @"\" + fileName;

                if (System.IO.Directory.Exists(outputDirectory))
                {
                    Helper.WriteLine("Video already downloaded", true, options);
                    continue;
                }
                try
                {
                    Helper.WriteLine("Created Directory " + outputDirectory, false, options);

                    System.IO.Directory.CreateDirectory(outputDirectory);

                    if (options.DownloadAll)
                    {
                        System.IO.File.WriteAllText(outputFileName + ".txt", Helper.GetVideoInformation(video));

                        Helper.WriteLine("Created and wrote to file " + outputFileName + ".txt", false, options);
                    }

                    if (options.Thumbnail)
                    {
                        Downloader.DownloadThumbnails(options, video, outputDirectory);
                    }

                    CancellationToken token = default;
                    if (options.Subtitles)
                    {
                        Helper.WriteLine("Attempting to download subtitle files for " + fileName, false, options);
                        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id);
                        foreach (var track in trackManifest.Tracks)
                        {
                            Helper.WriteLine("\tDownloading subtitle file in language " + track.Language + " for video " + fileName, false, options);
                            await youtube.Videos.ClosedCaptions.DownloadAsync(track, outputDirectory + @"\" + track.Language.Name + ".srt", null, token);
                        }
                    }

                    await Downloader.DownloadFullVideo(options, youtube, video, fileName, outputDirectory, token);
                }
                catch (Exception ex)
                {
                    Helper.WriteLine("Failed to download video, Error: " + ex.Message, true, options);
                    Helper.Logger(fileName + @"\" + fileName + ".mp4:  " + "Failed to download video, Error: " + ex.Message);
                    continue;
                }
            }

            string x = "Finished download at " + DateTime.Now.ToString() + ", Took " + (DateTime.Now - start).TotalMilliseconds.ToString() + "ms";
            Helper.WriteLine(x, true, options);
            Helper.Logger(x);
        }
    }
}

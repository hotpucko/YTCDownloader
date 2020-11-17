using CommandLine;

namespace YoutubeDownloader.Utility.CLI
{
    public class Options
    {
        //https://github.com/gsscoder/commandline/wiki
        public const bool debug = true;
        [Option('v', "verbose", Default = debug, HelpText = "Prints all messages to the standard output.")]
        public bool Verbose { get; set; }

        [Option('o', "output", Default = "output2", HelpText = "output folder.")]
        public string Output { get; set; }

        [Option('s', "subtitles", Default = debug, HelpText = "Downloads all subtitles associated with each video.")]
        public bool Subtitles { get; set; }

        [Option('c', "channel", Default = "UCIcgBZ9hEJxHv6r_jDYOMqg", HelpText = "The key to the channel, example: UCIcgBZ9hEJxHv6r_jDYOMqg.")]
        public string ChannelKey { get; set; }

        [Option('l', "logging", Default = debug, HelpText = "Log issues in a log file.")]
        public bool Logging { get; set; }

        [Option('a', "all", Default = debug, HelpText = "Download all associated data with a video, e.g. title, description, upload date etc.")]
        public bool DownloadAll { get; set; }

        [Option('t', "thumbnail", Default = debug, HelpText = "Download highest resolution thumbnail associated with the videos.")]
        public bool Thumbnail { get; set; }

        [Option('f', "fast", Default = false, HelpText = "Downloads only Muxed streams.")]
        public bool Fast { get; set; }
        [Option('v', "videoStream", HelpText = "", Default = "aDQ3nfBbPWM")]
        public string LiveStream { get; set; }
    }
}

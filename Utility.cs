using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Utility.CLI;
using YoutubeExplode;

namespace YoutubeDownloader.Utility
{
    /// <summary>
    /// A class containing helpful functions
    /// </summary>
    class Helper
    {
        /// <summary>
        /// Generates a file name based on id, title and date. Filters the indata for characters that are not allowed and replaces them with ~similar enough~ characters.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FileNameGenerator(int id, string title, string date)
        {
            date = ReplaceCharacters(date, new (char, char)[] { (':', 'ː') });
            date = date.Replace(':', 'ː');

            title = ReplaceCharacters(title, new (char, char)[] {
                ('\\', '＼'),
                ('/', '／'),
                ('*', '※'),
                ('?', '¿'),
                ('"', '”'),
                ('<', '≤'),
                ('>', '≥'),
                ('|', '∣'),
                (':', 'ː') });

            /*title = title.Replace('\\', '＼');
            title = title.Replace('/', '／');
            title = title.Replace('*', '※');
            title = title.Replace('?', '¿');
            title = title.Replace('"', '”');
            title = title.Replace('<', '≤');
            title = title.Replace('>', '≥');
            title = title.Replace('|', '∣');
            title = title.Replace(':', 'ː');*/
            return id + " - " + title + " - " + date;
        }

        public static string ReplaceCharacters(string input, params (char from, char to)[] from_to)
        {
            foreach ((char from, char to) in from_to)
            {
                input = input.Replace(from, to);
            }

            return input;
        }

        /// <summary>
        /// constructs and returns a string covering all information captured in a video object.
        /// </summary>
        /// <param name="video">a reference video</param>
        /// <returns>a string newline seperated by autho, id, url, channel, duration, keywords, views, likes, dislikes, upload date and description</returns>
        public static string GetVideoInformation(YoutubeExplode.Videos.Video video)
        {
            string keywords = "";
            foreach (var keyword in video.Keywords)
            {
                keywords += keyword + "\n\t";
            }

            string videoInformation = "" + video.Author.ToString() +
                                "\nAuthor: " + video.Title.ToString() +
                                "\nVideo ID: " + video.Id.ToString() +
                                "\nVideo URL: " + video.Url.ToString() +
                                "\nChannel ID: " + video.ChannelId.ToString() +
                                "\nVideo Duration: " + video.Duration.ToString() +
                                "\nKeywords: " + keywords +
                                "\nView Count: " + video.Engagement.ViewCount.ToString() +
                                "\nLikes: " + video.Engagement.LikeCount.ToString() +
                                "\nDislikes: " + video.Engagement.DislikeCount.ToString() +
                                "\nAverage Rating: " + video.Engagement.AverageRating.ToString() +
                                "\nUpload Date: " + video.UploadDate.DateTime.ToString() +
                                "\nDescription: " + video.Description.ToString();
            return videoInformation;
        }

        

        public static void Logger(string str)
        {
            if (!System.IO.Directory.Exists("log2"))
                System.IO.Directory.CreateDirectory("log2");
            System.IO.File.AppendAllText(@"log2\logfile.txt", str + "\n");
        }

        /// <summary>
        /// only Console.writeLines if it's either important, or if the option verbose is enabled.
        /// </summary>
        /// <param name="message">The Message to write to console</param>
        /// <param name="important">whether the message will display with or without verbose being set</param>
        /// <param name="o">options object</param>
        public static void WriteLine(string message, bool important, Options o)
        {
            if (o.Verbose || important)
                Console.WriteLine(message);
        }
    }
}

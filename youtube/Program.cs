using CommandLine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace youtube
{
    //Command line options
    class Options
    {
        [Option('l', "link", Required = true, HelpText = "Link(s) to be downloaded")]
        public IEnumerable<string> Links { get; set; }

        [Option('a', "audio-only", Required = false, HelpText = "Download audio only")]
        public bool OnlyAudio { get; set; }
    }

    class Program
    {
        static YoutubeClient YoutubeClient = new YoutubeClient();

        static async Task<int> Main(string[] args)
        {
            var returnCode = Parser.Default.ParseArguments<Options>(args).MapResult(
                (options) => Run(options),
                errors => HandleParseError(errors)
            );

            return await returnCode;
        }
            
        static async Task<int> Run(Options options)
        {
            var links = options.Links;

            foreach (var link in links)
                await Download(link, options.OnlyAudio);

            return 0;
        }
        static async Task<int> HandleParseError(IEnumerable<Error> errors) => 1;

        private static async Task Download(string link, bool onlyAudio)
        {
            IStreamInfo streamInfo;
            Console.WriteLine($"Downloading {link}");

            var video = await YoutubeClient.Videos.GetAsync(link);
            var streamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(link);
            
            if (onlyAudio)
                streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();
            else
                streamInfo = streamManifest.GetMuxed().WithHighestVideoQuality();

            var file = $"{Directory.GetCurrentDirectory()}\\{HandleText(video.Author)} - {HandleText(video.Title)}.{HandleText(streamInfo.Container.ToString())}";

            var stream = await YoutubeClient.Videos.Streams.GetAsync(streamInfo);
            await YoutubeClient.Videos.Streams.DownloadAsync(streamInfo, file);

            Console.WriteLine($"Saved to: {file}");
        }

        private static string HandleText(string text)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                text = text.Replace(c.ToString(), "");
            }

            return text;
        }
    }
}

namespace musicTag;

public enum MusicFileExtension
{
        Flac,
        Mp3,
        Wav,
        Wma,
        Ogg,
        Alac,
        M4a,
        Aac,
}

public static class MusicFileExtensionHelper
{
        public static readonly Dictionary<MusicFileExtension, string> ExtensionMap = new()
        {
                { MusicFileExtension.Flac, ".flac" },
                { MusicFileExtension.Mp3, ".mp3" },
                { MusicFileExtension.Wav, ".wav" },
                { MusicFileExtension.Wma, ".wma" },
                { MusicFileExtension.Ogg, ".ogg" },
                { MusicFileExtension.Alac, ".alac" },
                { MusicFileExtension.M4a, ".m4a" },
                { MusicFileExtension.Aac, ".aac" },

                
        };

        public static string ToFileExtension(this MusicFileExtension musicFileExtension)
        {
                return ExtensionMap[musicFileExtension];
                
        }
        
}

using System.Text.RegularExpressions;

namespace jammer
{
    public class URL
    {
        public static bool IsValidSoundcloudSong(string uri)
        {
            Regex regex = new Regex(Utils.scSongPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool isValidSoundCloudPlaylist(string uri)
        {
            Regex regex = new Regex(Utils.scPlaylistPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool IsValidYoutubeSong(string uri)
        {
            Regex regex = new Regex(Utils.ytSongPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool IsUrl(string uri)
        {
            return IsValidSoundcloudSong(uri) || IsValidYoutubeSong(uri);
        }
    }
}
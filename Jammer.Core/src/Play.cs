using ManagedBass;
using ManagedBass.Aac;
using Spectre.Console;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TagLib;


namespace Jammer
{
    public class Play
    {
        public static string[] songExtensions = {   ".mp3", ".ogg", ".wav", ".mp2", ".mp1", ".aiff", ".m2a", 
                                                    ".mpa", ".m1a", ".mpg", ".mpeg", ".aif", ".mp3pro", ".bwf", 
                                                    ".mus", ".mod", ".mo3", ".s3m", ".xm", ".it", ".mtm", ".umx", 
                                                    ".mdz", ".s3z", ".itz", ".xmz"};
        public static string[] aacExtensions = { ".aac", ".m4a", ".adts", ".m4b" };
        public static string[] mp4Extensions = { ".mp4" };

        public static bool isValidExtension(string extension)
        {
            foreach (string ext in songExtensions)
            {
                if (extension == ext)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isValidAACExtension(string extension)
        {
            foreach (string ext in aacExtensions)
            {
                if (extension == ext)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isValidMP4Extension(string extension)
        {
            foreach (string ext in mp4Extensions)
            {
                if (extension == ext)
                {
                    return true;
                }
            }
            return false;
        }
        
        // playsong function will play the song at the index of the array and get the path of the song
        public static void PlaySong(string[] songs, int Currentindex)
        {

            if (songs.Length == 0)
            {
                #if CLI_UI
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.NoSongsInPlaylist}[/]");
                #endif
                #if AVALONIA_UI
                // TODO Add no songs in playlist message
                #endif
                Currentindex = 0;
                Start.Run(new string[] {});
                return;
            }
            while(Currentindex > songs.Length){
                Currentindex--;
            }
            Debug.dprint("Play song");
            var path = "";

            string song = songs[Currentindex];
            // if song has a title, remove it
            if (song.Contains("½"))
            {
                song = song.Split("½")[0];
            }
            Utils.songs[Currentindex] = song;

            // check if file is a local
            if (System.IO.File.Exists(song))
            {
                // id related to local file path, convert to absolute path
                path = Path.GetFullPath(song);
            }
            // if folder
            else if (Directory.Exists(song))
            {
                int originalLengthMinusFolder = Utils.songs.Length - 1;
                // add all files in folder to Utils.songs
                string[] files = Directory.GetFiles(song);
                foreach (string file in files)
                {
                    AddSong(file);
                }
                #if CLI_UI
                AnsiConsole.MarkupLine("[bold]" + Currentindex + "[/] : " + Utils.songs.Length + " : " + Utils.currentSongIndex + " : " + originalLengthMinusFolder);
                #endif
                #if AVALONIA_UI
                // TODO Add folder message
                #endif

                if (Utils.songs.Length == originalLengthMinusFolder) {
                    path = Utils.songs[originalLengthMinusFolder - 1];
                }
                else {
                    path = song;
                }
                
                
            }
            else if (URL.isValidSoundCloudPlaylist(song)) {
                // id related to url, download and convert to absolute path
                Debug.dprint("Soundcloud playlist.");
                path = Download.GetSongsFromPlaylist(song, "soundcloud");
            }
            else if (URL.IsValidSoundcloudSong(song))
            {
                // id related to url, download and convert to absolute path
                path = Download.DownloadSong(song);
            }
            else if (URL.IsValidYoutubePlaylist(song))
            {
                // id related to url, download and convert to absolute path
                path = Download.GetSongsFromPlaylist(song, "youtube");
            }
            else if (URL.IsValidYoutubeSong(song))
            {
                // id related to url, download and convert to absolute path
                path = Download.DownloadSong(song);
            }
            else
            {
                #if CLI_UI
                AnsiConsole.MarkupLine($"[red] {Locale.OutsideItems.SongNotFound}[/]");
                #endif
                #if AVALONIA_UI
                // TODO Add song not found message
                #endif
                return;
            }

            Start.prevMusicTimePlayed = -1;
            Start.lastSeconds = -1;
            Utils.currentSong = path;
            Utils.currentSongIndex = Currentindex;
            Utils.currentPlaylistSongIndex = Currentindex;
            Utils.songs[Currentindex] = Utils.songs[Currentindex];

            // taglib get title to display
            TagLib.File? tagFile;
            string title = "";

            try {
                tagFile = TagLib.File.Create(path);
                title = tagFile.Tag.Title;

                if (title == null || title == "")
                    title = "";
                else
                    title = "½" + title;

            } catch (Exception) {
                tagFile = null;
            }
            

            if (title == null || title == "")
            {
                title = "";
            }

            if (Utils.songs[Currentindex].Contains("½"))
            {
                title = "";
            }


            Utils.songs[Currentindex] = song + title;

            Playlists.AutoSave();

            try
            {
                string extension = Path.GetExtension(path).ToLower();

                if (isValidExtension(extension) || isValidAACExtension(extension) || isValidMP4Extension(extension))
                {
                    Debug.dprint("Audiofile");
                    StartPlaying();
                }
                else if (extension == ".Jammer") {
                    Debug.dprint("Jammer");
                    // read playlist

                    string[] playlist = System.IO.File.ReadAllLines(path);
                    // add all songs in playlist to Utils.songs
                    foreach (string s in playlist) {
                        AddSong(s);
                    }
                    // remove playlist from Utils.songs
                    Utils.songs = Utils.songs.Where((source, i) => i != Currentindex).ToArray();
                    if (Currentindex == Utils.songs.Length)
                    {
                        Utils.currentSongIndex = Utils.songs.Length - 1;
                    }
                    Start.state = MainStates.playing;
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
                else
                {
                    #if CLI_UI
                    Console.WriteLine(Locale.OutsideItems.UnsupportedFileFormat);
                    #endif
                    #if AVALONIA_UI
                    // TODO Add unsupported file format message
                    #endif
                    Debug.dprint("Unsupported file format");
                    // remove song from current Utils.songs
                    Utils.songs = Utils.songs.Where((source, i) => i != Currentindex).ToArray();
                    if (Currentindex == Utils.songs.Length)
                    {
                        Utils.currentSongIndex = Utils.songs.Length - 1;
                    }
                    // Start.state = MainStates.playing;
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
            }
            catch (Exception e)
            {
                #if CLI_UI
                Console.WriteLine($"{Locale.OutsideItems.Error}: " + e);
                #endif
                #if AVALONIA_UI
                // TODO Add error message
                #endif
                Debug.dprint("Error: " + e);
                return;
            }
            Debug.dprint("End of PlaySong");
        }

        public static void PauseSong()
        {
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void ResumeSong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void PlaySong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPlay(Utils.currentMusic);
        }

        public static void StopSong()
        {
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void ResetMusic()
        {
            if (Utils.currentMusic != 0)
            {
                Bass.StreamFree(Utils.currentMusic);    
            }
        }
        public static void NextSong()
        {
            if(Utils.queueSongs.Count > 0){
                PlayDrawReset();
                PlaySong(Utils.queueSongs.ToArray(), 0);
                PlaySong();
                int index = Array.IndexOf(Utils.songs, Utils.queueSongs[0]);
                Utils.previousSongIndex = Utils.currentSongIndex;
                Utils.currentSongIndex = index;
                Utils.queueSongs.RemoveAt(0);
            } else {
                if (Utils.songs.Length >= 1) // no next song if only one song or less
                {
                    Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
                    PlayDrawReset();
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
            }
        }

        public static void PlayDrawReset() // play, draw, reset lastSeconds
        {
            // Start.state = MainStates.playing;
            Start.drawOnce = true;
            Start.prevMusicTimePlayed = 0;
        }

        public static void RandomSong()
        {
            int lastSongIndex = Utils.currentSongIndex;
            Random rnd = new Random();
            Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            // make sure we dont play the same song twice in a row
            while (Utils.currentSongIndex == lastSongIndex)
            {
                Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            }
            PlayDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }
        public static void PrevSong()
        {
            Utils.currentSongIndex = (Utils.currentSongIndex - 1) % Utils.songs.Length;
            if (Utils.currentSongIndex < 0)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            PlayDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }
        public static void SeekSong(float seconds, bool relative)
        {
            // Calculate the seek position based on the requested seconds
            var pos = Bass.ChannelGetPosition(Utils.currentMusic);
            
            // If seeking relative to the current position, adjust the seek position
            if (relative)
            {
                // if negative, move backwards
                if (seconds < 0)
                {
                    pos = pos - Bass.ChannelSeconds2Bytes(Utils.currentMusic, Preferences.GetRewindSeconds());
                }
                else
                {
                    pos = pos + Bass.ChannelSeconds2Bytes(Utils.currentMusic, Preferences.GetForwardSeconds());
                }
                // Clamp again to ensure it's within the valid range
                //pos = Math.Max(0, Math.Min(pos, Utils.audioStream.Length));
                Start.drawOnce = true;
            }
            else
            {
                // Clamp the seek position to be within the valid range [0, Utils.audioStream.Length]
                pos = Bass.ChannelSeconds2Bytes(Utils.currentMusic, seconds);
            }

            // Update the audio stream's position
            //if (Utils.audioStream.Length == pos)
            if (pos < 0)
            {
                Bass.ChannelSetPosition(Utils.currentMusic, 0);
            }
            else if (pos >= Bass.ChannelGetLength(Utils.currentMusic))
            {
                Bass.ChannelSetPosition(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic) - 1);
            }
            else
            {
                Bass.ChannelSetPosition(Utils.currentMusic, pos);
            }

            if (Bass.ChannelGetPosition(Utils.currentMusic) >= Bass.ChannelGetLength(Utils.currentMusic))
            {
                MaybeNextSong();
            }
            PlayDrawReset();
            return;
        }

        public static void ModifyVolume(float volume)
        {
            Preferences.volume += volume;
            if (Preferences.volume > 1)
            {
                Preferences.volume = 1;
            }
            else if (Preferences.volume < 0)
            {
                Preferences.volume = 0;
            }

            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.volume);

        }

        public static void ToggleMute()
        {
            if (Preferences.isMuted)
            {
                Preferences.isMuted = false;
                //Utils.currentMusic.Volume = Preferences.oldVolume;
                Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.oldVolume);
                Preferences.volume = Preferences.oldVolume;
            }
            else
            {
                Preferences.isMuted = true;
                Preferences.oldVolume = Preferences.volume;
                Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, 0);
                Preferences.volume = 0;
            }
        }

        public static void MaybeNextSong()
        {
            Start.drawOnce = true;
            
            if (Preferences.isLoop)
            {
                Bass.ChannelSetPosition(Utils.currentMusic, 0);
                Bass.ChannelPlay(Utils.currentMusic);
            }
            else if (Utils.songs.Length == 1 && !Preferences.isLoop){
                //Utils.audioStream.Position = Utils.audioStream.Length;
                Bass.ChannelSetPosition(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic));
                Start.state = MainStates.pause;
                //Utils.currentMusic.Pause();
                Bass.ChannelPause(Utils.currentMusic);
            }
            else if (Preferences.isShuffle)
            {
                RandomSong();
            }
            else
            {
                NextSong();
            }
        }

        public static void ReDownloadSong()
        {
            if (Utils.songs[Utils.currentSongIndex].Contains("https://") || Utils.songs[Utils.currentSongIndex].Contains("http://"))
            {
                System.IO.File.Delete(Utils.currentSong);
                PlaySong(Utils.songs, Utils.currentSongIndex);
                SeekSong(0, false);
                return;
            }

            string path = Utils.currentSong;
            string fileName = Path.GetFileName(path);
            string temporary_filename = Utils.songs[Utils.currentSongIndex].Split("½")[0];
            System.IO.File.Delete(path);
            Utils.songs[Utils.currentSongIndex] = Utils.songs[Utils.currentSongIndex].Split("½")[0];

            if(fileName.Contains("www.youtube.com") && !URL.IsUrl(temporary_filename)){
                // reconstruct url
                int space = fileName.IndexOf(" ");
                if(space != -1){
                    fileName = fileName.Remove(space, 1).Insert(space, "/");
                    space = fileName.IndexOf(" ");
                    if(space != -1){
                        fileName = fileName.Remove(space, 1).Insert(space, "?");
                    }
                    // remove .mp4
                    int dotPos = temporary_filename.LastIndexOf(".mp4");
                    if(dotPos != -1){
                        fileName = fileName.Remove(fileName.Length - 4, 4);
                    }
                }
                string new_url = "https://" + fileName;
                Utils.songs[Utils.currentSongIndex] = new_url;
            } else if(fileName.Contains("soundcloud.com") && !URL.IsUrl(temporary_filename)){
                // reconstruct url
                int space = fileName.IndexOf(" ");
                if(space != -1){
                    fileName = fileName.Remove(space, 1).Insert(space, "/");
                    space = fileName.IndexOf(" ");
                    if(space != -1){
                        fileName = fileName.Remove(space, 1).Insert(space, "/");
                    }
                    // remove .mp3
                    int dotPos = temporary_filename.LastIndexOf(".mp3");
                    if(dotPos != -1){
                        fileName = fileName.Remove(fileName.Length - 4, 4);
                    }
                }
                string new_url = "https://" + fileName;
                
                Utils.songs[Utils.currentSongIndex] = new_url;
            } else {
                Utils.songs[Utils.currentSongIndex] = temporary_filename;
            }

            
            PlaySong(Utils.songs, Utils.currentSongIndex);
            SeekSong(0, false);

        }

        public static void AddSong(string song)
        {
            // check if song is already in playlist
            foreach (string s in Utils.songs)
            {
                if (s == song)
                {
                    #if CLI_UI
                    Console.WriteLine(Locale.OutsideItems.SongInPlaylist);
                    #endif
                    #if AVALONIA_UI
                    // TODO Add song in playlist message
                    #endif
                    return;
                }
            }
            // add song to current Utils.songs
            Array.Resize(ref Utils.songs, Utils.songs.Length + 1);
            Utils.songs[Utils.songs.Length - 1] = song;

            if (Utils.songs.Length == 1)
            {
                Utils.currentSongIndex = 0;
                PlaySong(Utils.songs, Utils.currentSongIndex);
            }
        }
        public static void  DeleteSong(int index, bool isQueue)
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            // check if index is in range
            if (index < 0 || index > Utils.songs.Length)
            {
                #if CLI_UI
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.IndexOoR}[/]");
                #endif
                #if AVALONIA_UI
                // TODO Add index out of range message
                #endif
                return;
            }
            // remove song from current Utils.songs
            Utils.songs = Utils.songs.Where((source, i) => i != index).ToArray();
            Utils.currentSongIndex--;
            if(Utils.currentSongIndex == -1){
                Utils.currentSongIndex = 0;
            }
            // PREV RESET
            // Console.WriteLine((index < Utils.currentSongIndex   ) + " " + Utils.currentPlaylistSongIndex);
            if (index == Utils.songs.Length){
                if (Utils.songs.Length == 0) {
                    Utils.songs = new string[] { "" };
                    ResetMusic();
                    Start.state = MainStates.pause;
                }
                else if(index >= Utils.currentSongIndex){
                    _ = Utils.currentSongIndex;
                }
                else {
                    Utils.currentSongIndex = Utils.songs.Length - 1;
                    // Start.state = MainStates.playing;
                }
            } else {
                if(index < Utils.currentPlaylistSongIndex && index != Utils.currentPlaylistSongIndex){
                    if(Utils.currentPlaylistSongIndex == Utils.songs.Length){
                        Utils.currentPlaylistSongIndex--;
                    } else {
                        Utils.currentPlaylistSongIndex++;
                    }
                } else {
                    Utils.currentPlaylistSongIndex++;
                }
            }
            
            Playlists.AutoSave();
            // if no songs left, add "" to Utils.songs
            if (!isQueue)
            {
                PlaySong(Utils.songs, Utils.currentSongIndex);
            }
            else
            {
                if (Utils.currentSongIndex == index)
                {
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
            }
        }

        public static void Shuffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.songs = Utils.songs.OrderBy(x => rnd.Next()).ToArray();
        }

        public static string Title(string title, string getOrNot)
        {
            if (title.Contains("½"))
            {
                string[] titleSplit = title.Split("½");
                if (getOrNot == "get")
                {
                    string a = titleSplit[1];
                    int posdot = a.LastIndexOf(".");
                    string ext = "";
                    if(posdot != -1){
                        ext = a[posdot..];
                    }
                    // Console.WriteLine(a + " " + posdot);
                    // Console.ReadKey();

                    if(Play.isValidAACExtension(ext) || Play.isValidExtension(ext) || Play.isValidMP4Extension(ext)){
                        
                        return a[..posdot];
                    } else {
                        return a;
                    }
                }
                else if (getOrNot == "not")
                {
                    return titleSplit[0];
                }
            }
            return title;
        }

        static public void StartPlaying()
        {

            ResetMusic();
            // if extension is aac, use aac decoder
            if (isValidAACExtension(Path.GetExtension(Utils.currentSong)))
            {
                BassAac.PlayAudioFromMp4 = false;
                BassAac.AacSupportMp4 = false;
                Utils.currentMusic = BassAac.CreateStream(Utils.currentSong, 0, 0, BassFlags.Default);
            }
            else if (isValidMP4Extension(Path.GetExtension(Utils.currentSong)))
            {
                // flags
                BassAac.PlayAudioFromMp4 = true;
                BassAac.AacSupportMp4 = true;
                Utils.currentMusic = BassAac.CreateMp4Stream(Utils.currentSong, 0, 0, BassFlags.Default);
            }
            else
            {
                // create stream
                Utils.currentMusic = Bass.CreateStream(Utils.currentSong, 0, 0, BassFlags.Default);
            }

            // create stream
            if (Utils.currentMusic == 0)
            {
                #if CLI_UI
                Jammer.Message.Data(Locale.OutsideItems.StartPlayingMessage1, $"{Locale.OutsideItems.StartPlayingMessage2}: " + Utils.currentSong);
                #endif
                #if AVALONIA_UI
                // TODO Add start playing message
                #endif

                DeleteSong(Utils.currentSongIndex, false);
                
            }

            // set volume
            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.GetVolume());

            Start.state = MainStates.playing;
            // play stream
            Start.prevMusicTimePlayed = 0;
            PlayDrawReset();
            Bass.ChannelPlay(Utils.currentMusic);
            #if CLI_UI
            TUI.RefreshCurrentView();
            #endif
            // TODO AVALONIA_UI
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.Xna.Framework.Media;
#endif
using System;

namespace VirusX
{
    class AudioManager
    {
#region singleton
        private static readonly AudioManager instance = new AudioManager();
        public static AudioManager Instance { get { return instance; } }
        private AudioManager() { }
#endregion

        ContentManager content;
        Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();

        bool isPlaying = false;
#if WINDOWS_UWP
        Dictionary<string, Windows.Storage.StorageFile> songs = new Dictionary<string, Windows.Storage.StorageFile>();
        MediaElement musicPlayer = new MediaElement();
#else
        Dictionary<string, Song> songs = new Dictionary<string, Song>();
#endif

        public void LoadContent(ContentManager content)
        {
            this.content = content;

#if WINDOWS_UWP
            musicPlayer = new MediaElement();
            musicPlayer.Volume = 0.5f;
            musicPlayer.IsLooping = true;
#else
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.IsRepeating = true;
#endif

            soundEffects.Add("click", content.Load<SoundEffect>("sound/room__snare-switchy"));
            //soundEffects.Add("click", content.Load<SoundEffect>("sound/tick2"));
            soundEffects.Add("capture", content.Load<SoundEffect>("sound/capture"));
            //soundEffects.Add("capture", content.Load<SoundEffect>("sound/soundFx8"));
            soundEffects.Add("switch", content.Load<SoundEffect>("sound/switch"));
            soundEffects.Add("explosion", content.Load<SoundEffect>("sound/explosion"));
            soundEffects.Add("danger", content.Load<SoundEffect>("sound/danger_zone"));
            soundEffects.Add("wipeout", content.Load<SoundEffect>("sound/andromadax24__woosh-01"));
            soundEffects.Add("collect", content.Load<SoundEffect>("sound/cosmicd__light-switch-of-doom"));
            soundEffects.Add("swoosh", content.Load<SoundEffect>("sound/chripei__whoosh-1"));
            soundEffects.Add("death", content.Load<SoundEffect>("sound/lg__electric09"));

#if WINDOWS_UWP
            Windows.Storage.StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var contentFolder = installFolder.GetFolderAsync("Content").GetAwaiter().GetResult();
            var soundFolder = contentFolder.GetFolderAsync("sound").GetAwaiter().GetResult();
            var musicFile = soundFolder.GetFileAsync("09 Beach.mp3").GetAwaiter().GetResult();
            songs.Add("background", musicFile);
#else
            songs.Add("background", content.Load<Song>("sound/09 Beach"));
#endif

            PlaySong("background");
        }

        /// <summary>
        /// Plays a preloaded sound effect by name
        /// </summary>
        /// <param name="sound">click, capture, switch, explosion, danger, wipeout, collect</param>
        public void PlaySoundeffect(string sound)
        {
            System.Diagnostics.Debug.Assert(soundEffects.ContainsKey(sound));

            try
            {
                if (Settings.Instance.Sound)
                    soundEffects[sound].Play();
            }
            catch { }
        }

        /// <summary>
        /// Plays a preloaded song effect by name
        /// </summary>
        /// <param name="song"></param>
        public async void PlaySong(string song)
        {
            if (Settings.Instance.Music)
            {
#if WINDOWS_UWP
                var file = songs[song];
                musicPlayer.SetSource(await file.OpenAsync(Windows.Storage.FileAccessMode.Read), file.ContentType);
                musicPlayer.Play();
#else
                MediaPlayer.Play(songs[song]);
#endif
                isPlaying = true;
            }
        }

        public void PlaySongsRandom()
        {
            // if song is playing without permission -> stop it!
            if (!Settings.Instance.Music && isPlaying)
            {
#if WINDOWS_UWP
                musicPlayer.Stop();
#else
                MediaPlayer.Stop();
#endif
                isPlaying = false;
            }

            // if song has ended search for a new one
            if (Settings.Instance.Music && !isPlaying)
            {
                PlaySong(songs.Keys.ElementAt(Random.Next(songs.Keys.Count)));
            }
        }
    }
}
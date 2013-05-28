using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

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
        Dictionary<string, Song> songs = new Dictionary<string, Song>();

        public void Initialize(ContentManager content)
        {
            this.content = content;

            MediaPlayer.Volume = 0.5f;
            MediaPlayer.IsRepeating = true;

            soundEffects.Add("click", content.Load<SoundEffect>("sound/room__snare-switchy"));
            soundEffects.Add("capture", content.Load<SoundEffect>("sound/capture"));
            soundEffects.Add("switch", content.Load<SoundEffect>("sound/switch"));
            soundEffects.Add("explosion", content.Load<SoundEffect>("sound/explosion"));
            soundEffects.Add("danger", content.Load<SoundEffect>("sound/danger_zone"));
            soundEffects.Add("wipeout", content.Load<SoundEffect>("sound/andromadax24__woosh-01"));
            soundEffects.Add("collect", content.Load<SoundEffect>("sound/cosmicd__light-switch-of-doom"));

            songs.Add("beach", content.Load<Song>("sound/09 Beach"));
        }

        /// <summary>
        /// Plays a preloaded sound effect by name
        /// </summary>
        /// <param name="sound">click, capture, switch, explosion, danger, wipeout, collect</param>
        public void PlaySoundeffect(string sound)
        {
            System.Diagnostics.Debug.Assert(soundEffects.ContainsKey(sound));

            if (Settings.Instance.Sound)
                soundEffects[sound].Play();
        }

        /// <summary>
        /// Plays a preloaded song effect by name
        /// </summary>
        /// <param name="song"></param>
        public void PlaySong(string song)
        {
            if (Settings.Instance.Music)
                MediaPlayer.Play(songs[song]);
        }

        public void PlaySongsRandom()
        {
            // if song is playing without permission -> stop it!
            if (!Settings.Instance.Music && MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            // if song has ended search for a new one
            if (Settings.Instance.Music && MediaPlayer.State == MediaState.Stopped)
            {
                PlaySong(songs.Keys.ElementAt(Random.Next(songs.Keys.Count)));
            }
        }
    }
}
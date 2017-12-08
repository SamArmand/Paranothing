using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Paranothing
{
    internal sealed class SoundManager
    {
        private static SoundManager _instance;

        public Dictionary<string, SoundEffect> SoundEffects { get; }

        public static SoundManager getInstance()
        {
            return _instance ?? (_instance = new SoundManager());
        }

        private SoundManager() 
        {
            SoundEffects = new Dictionary<string, SoundEffect>();
        }

        public void playSound(string soundName)
        {
            SoundEffects[soundName].Play();
        }
    }
}

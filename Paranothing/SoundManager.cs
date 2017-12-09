using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Paranothing
{
    internal sealed class SoundManager
    {
        private static SoundManager _instance;

        public Dictionary<string, SoundEffect> SoundEffects { get; }

        public static SoundManager GetInstance()
        {
            return _instance ?? (_instance = new SoundManager());
        }

        private SoundManager()
        {
            SoundEffects = new Dictionary<string, SoundEffect>();
        }

        public void PlaySound(string soundName)
        {
            if (GameTitle.ToggleSound)
                SoundEffects[soundName].Play();
        }
    }
}

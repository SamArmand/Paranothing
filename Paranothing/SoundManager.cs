using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Paranothing
{
    sealed class SoundManager
    {
        static SoundManager _instance;

        public Dictionary<string, SoundEffect> SoundEffects { get; } = new Dictionary<string, SoundEffect>();

        public Dictionary<string, SoundEffectInstance> SoundEffectInstances { get; } = new Dictionary<string, SoundEffectInstance>();
        public static SoundManager Instance() => _instance ?? (_instance = new SoundManager());

        public void PlaySound(string soundName, bool isLooped = false)
        {
            if (GameTitle.ToggleSound)
            {
                var soundEffectInstance = SoundEffectInstances[soundName];

                if (SoundEffectInstances.ContainsKey(soundName) && soundEffectInstance.State == SoundState.Playing)
                    return;

                soundEffectInstance = SoundEffects[soundName].CreateInstance();
                soundEffectInstance.IsLooped = isLooped;
                soundEffectInstance.Play();
            }
        }

        public void StopSound(string soundName)
        {
            var soundEffectInstance = SoundEffectInstances[soundName];

            if (GameTitle.ToggleSound && SoundEffectInstances.ContainsKey(soundName) && soundEffectInstance.State == SoundState.Playing)
                soundEffectInstance.Stop();
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Paranothing
{
	sealed class SoundManager
	{
		static SoundManager _instance;

		internal Dictionary<string, SoundEffect> SoundEffects { get; } = new Dictionary<string, SoundEffect>();

		Dictionary<string, SoundEffectInstance> SoundEffectInstances { get; } =
			new Dictionary<string, SoundEffectInstance>();

		internal static SoundManager Instance() => _instance ??= new SoundManager();

		internal void PlaySound(string soundName, bool isLooped = false, bool force = false)
		{
			if (!GameTitle.ToggleSound) return;

			if (!SoundEffectInstances.ContainsKey(soundName))
				SoundEffectInstances[soundName] = SoundEffects[soundName].CreateInstance();

			var soundEffectInstance = SoundEffectInstances[soundName];

			if (soundEffectInstance.State == SoundState.Playing)
			{
				if (force)
					StopSound(soundName);
				else
					return;
			}

			soundEffectInstance.IsLooped = isLooped;
			soundEffectInstance.Play();
		}

		internal void StopSound(string soundName)
		{
			if (GameTitle.ToggleSound && SoundEffectInstances.ContainsKey(soundName) &&
				SoundEffectInstances[soundName].State == SoundState.Playing)
				SoundEffectInstances[soundName].Stop();
		}
	}
}
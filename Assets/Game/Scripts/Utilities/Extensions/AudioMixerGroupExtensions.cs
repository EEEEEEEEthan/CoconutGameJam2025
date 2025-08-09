using System;
using Game.Utilities.Pools;
using Game.Utilities.UnityTools;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		static readonly ObjectPool<AudioSource> audioSourceThreadedPool = new(
			create: () =>
			{
				var gameObject = new GameObject();
				var audioSource = gameObject.AddComponent<AudioSource>();
				gameObject.name = nameof(AudioSource);
				return audioSource;
			},
			onReturn: obj =>
			{
				if (!obj) return false;
				if (!audioPoolTransform)
				{
					audioPoolTransform = new GameObject("AudioPool").transform;
					Object.DontDestroyOnLoad(audioPoolTransform.gameObject);
				}
				obj.Stop();
				obj.clip = null;
				obj.outputAudioMixerGroup = null;
				obj.transform.parent = audioPoolTransform;
				return true;
			});
		static Transform audioPoolTransform;
		public static Pooled GenerateAudioSource(this AudioMixerGroup @this, out AudioSource source)
		{
			var disposable = audioSourceThreadedPool.Rent(out source);
			source.outputAudioMixerGroup = @this;
			source.volume = 1;
			return disposable;
		}
		public static async void PlayAsync(this AudioMixerGroup @this, AudioClip clip)
		{
			try
			{
				if (!clip) return;
				using (@this.GenerateAudioSource(out var source))
				{
					source.clip = clip;
					source.volume = 1;
					source.Play();
					await MainThreadTimerManager.Await(clip.length);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}

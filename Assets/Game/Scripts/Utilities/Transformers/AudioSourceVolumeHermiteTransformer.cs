using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	public static partial class Extensions
	{
		[RequireComponent(typeof(AudioSource))]
		sealed class AudioSourceVolumeHermiteTransformer : HermiteSingleTransformer
		{
			AudioSource cachedAudioSource;
			protected override float Value
			{
				get => AudioSource.volume;
				set => AudioSource.volume = value;
			}
			AudioSource AudioSource => cachedAudioSource ? cachedAudioSource : cachedAudioSource = GetComponent<AudioSource>();
		}
		public static void HermiteSetVolume(this AudioSource @this, float volume, float smoothTime, Action callback)
		{
			var transformer = @this.gameObject.GetOrAddComponent<AudioSourceVolumeHermiteTransformer>();
			transformer.SetValue(volume, smoothTime, callback);
		}
		public static Awaitable HermiteSetVolume(this AudioSource @this, float volume, float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			@this.HermiteSetVolume(volume, smoothTime, handle.Set);
			return awaitable;
		}
	}
}

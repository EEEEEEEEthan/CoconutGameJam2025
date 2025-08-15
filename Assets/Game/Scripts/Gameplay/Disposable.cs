using System;
namespace Game.Gameplay
{
	public readonly struct Disposable : IDisposable
	{
		readonly Action onDispose;
		public Disposable(Action onDispose) => this.onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
		void IDisposable.Dispose() => onDispose?.Invoke();
	}
}

using System;
using Game.Utilities.Time;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Utilities.UnityTools
{
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public static class MainThreadTaskQueue
	{
		[DefaultExecutionOrder(int.MinValue)]
		sealed class MainThreadTaskQueueUpdater : MonoBehaviour
		{
			void Update() => OnEarlyUpdate();
		}
		[DefaultExecutionOrder(int.MaxValue)]
		sealed class MainThreadTaskQueueLateUpdater : MonoBehaviour
		{
			void LateUpdate() => OnLateUpdate();
		}
		static readonly TaskQueue[] taskQueues = new TaskQueue[2];
		static readonly TaskQueuePriorityCode[] priorityValues =
			(TaskQueuePriorityCode[])Enum.GetValues(typeof(TaskQueuePriorityCode));
		static readonly ThreadConverter threadConverter = new();
		static DateTime frameBegin;
		static MainThreadTaskQueue()
		{
			foreach (var priority in priorityValues)
				taskQueues[(int)priority] = new()
				{
					FrameTime = priority switch
					{
						TaskQueuePriorityCode.UserInterface => TimeSpan.FromSeconds(0.05f),
						TaskQueuePriorityCode.Background => TimeSpan.FromSeconds(0.01f),
						_ => throw new ArgumentOutOfRangeException(),
					},
					Threshold = priority switch
					{
						TaskQueuePriorityCode.UserInterface => 0.1f,
						TaskQueuePriorityCode.Background => 0.01f,
						_ => throw new ArgumentOutOfRangeException(),
					},
				};
#if UNITY_EDITOR
			ApplicationEventListener.OnEditorUpdate += update;
			static void update()
			{
				if (Application.isPlaying) return;
				OnEarlyUpdate();
				OnLateUpdate();
			}
#endif
		}
		public static void Enqueue(TaskQueuePriorityCode taskQueuePriority, Action action)
		{
			var queue = taskQueues[(int)taskQueuePriority];
			lock (queue)
			{
				queue.Enqueue(action);
			}
		}
		public static Awaitable Await(TaskQueuePriorityCode taskQueuePriority)
		{
			var awaitable = Awaitable.Create(out var handle);
			Enqueue(taskQueuePriority, handle.Set);
			return awaitable;
		}
		static void OnEarlyUpdate()
		{
			frameBegin = DateTime.Now;
			threadConverter.Update();
		}
		static void OnLateUpdate()
		{
			var count = taskQueues.Length;
			var endTime = frameBegin + TimeSpan.FromSeconds(1f / Application.targetFrameRate);
			for (var i = 0; i < count; ++i)
			{
				var now = DateTime.Now;
				var queue = taskQueues[i];
				var remaining = endTime - now;
				if (remaining.Ticks < 0) remaining = new(1);
				queue.FrameTime = remaining;
				queue.Update();
			}
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Initialize()
		{
			var gameObject = new GameObject($"{nameof(MainThreadTaskQueue)}.{nameof(MainThreadTaskQueueLateUpdater)}");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<MainThreadTaskQueueUpdater>();
			gameObject.AddComponent<MainThreadTaskQueueLateUpdater>();
		}
	}
}

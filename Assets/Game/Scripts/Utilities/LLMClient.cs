using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
namespace Game.Utilities
{
	public static class LlmClient
	{
#if UNITY_EDITOR
		public static class EditorDefinitions
		{
			sealed class PersonalEditorConfigs : ScriptableObject
			{
				public Config defaultConfig;
				public bool overrideAutoBuildConfig;
				public Config autoBuildConfig;
				public bool overrideGameObjectNamerConfig;
				public Config gameObjectNamerConfig;
			}
			static PersonalEditorConfigs cachedInstance;
			public static Config DefaultConfig => Instance.defaultConfig;
			public static Config AutoBuildConfig => Instance.overrideAutoBuildConfig ? Instance.autoBuildConfig : Instance.defaultConfig;
			public static Config GameObjectNamerConfig => Instance.overrideAutoBuildConfig ? Instance.gameObjectNamerConfig : Instance.defaultConfig;
			static PersonalEditorConfigs Instance
			{
				get
				{
					if (cachedInstance) return cachedInstance;
					cachedInstance = ScriptableObject.CreateInstance<PersonalEditorConfigs>();
					var json = UnityEditor.EditorUserSettings.GetConfigValue(typeof(PersonalEditorConfigs).FullName);
					UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, cachedInstance);
					return cachedInstance;
				}
			}
			[UnityEditor.SettingsProvider]
			static UnityEditor.SettingsProvider CreateMyCustomSettingsProvider()
			{
				var serializedObject = new UnityEditor.SerializedObject(Instance);
				var provider = new UnityEditor.SettingsProvider("Preferences/*Game*", UnityEditor.SettingsScope.User)
				{
					label = "*Game*",
					guiHandler = _ =>
					{
						UnityEditor.EditorGUILayout.LabelField("LLM Settings", UnityEditor.EditorStyles.boldLabel);
						using (new UnityEditor.EditorGUI.IndentLevelScope())
						{
							UnityEditor.EditorGUI.BeginChangeCheck();
							var defaultConfig = serializedObject.FindProperty(nameof(PersonalEditorConfigs.defaultConfig));
							var overrideAutoBuildConfig = serializedObject.FindProperty(nameof(PersonalEditorConfigs.overrideAutoBuildConfig));
							var autoBuildConfig = serializedObject.FindProperty(nameof(PersonalEditorConfigs.autoBuildConfig));
							var overrideGameObjectNamerConfig = serializedObject.FindProperty(nameof(PersonalEditorConfigs.overrideGameObjectNamerConfig));
							var gameObjectNamerConfig = serializedObject.FindProperty(nameof(PersonalEditorConfigs.gameObjectNamerConfig));
							UnityEditor.EditorGUILayout.PropertyField(defaultConfig);
							UnityEditor.EditorGUILayout.PropertyField(overrideAutoBuildConfig);
							if (overrideAutoBuildConfig.boolValue) UnityEditor.EditorGUILayout.PropertyField(autoBuildConfig);
							UnityEditor.EditorGUILayout.PropertyField(overrideGameObjectNamerConfig);
							if (overrideGameObjectNamerConfig.boolValue) UnityEditor.EditorGUILayout.PropertyField(gameObjectNamerConfig);
							if (UnityEditor.EditorGUI.EndChangeCheck())
							{
								serializedObject.ApplyModifiedProperties();
								var json = UnityEditor.EditorJsonUtility.ToJson(Instance, true);
								UnityEditor.EditorUserSettings.SetConfigValue(typeof(PersonalEditorConfigs).FullName, json);
							}
						}
					},
					keywords = new[] { "Game", "AI", "API", "Key", "LLM", },
				};
				return provider;
			}
		}
#endif
		[Serializable]
		public struct Config : IEquatable<Config>
		{
			public static bool operator ==(Config left, Config right) => left.Equals(right);
			public static bool operator !=(Config left, Config right) => !left.Equals(right);
			public string url;
			public string model;
			public string apiKey;
			public override bool Equals(object obj) => obj is Config other && Equals(other);
			public override int GetHashCode()
			{
				unchecked
				{
					var hash = url != null ? url.GetHashCode() : 0;
					hash = (hash * 397) ^ (model != null ? model.GetHashCode() : 0);
					hash = (hash * 397) ^ (apiKey != null ? apiKey.GetHashCode() : 0);
					return hash;
				}
			}
			public bool Equals(Config other) => url == other.url && model == other.model && apiKey == other.apiKey;
		}
		/// <summary>
		///     详情见https://www.volcengine.com/docs/82379/1298454
		/// </summary>
		[Serializable]
		public struct Result
		{
			[Serializable]
			public struct Message
			{
				// ReSharper disable once InconsistentNaming
				public string content;
			}
			[Serializable]
			public struct Choice
			{
				// ReSharper disable once InconsistentNaming
				public Message message;
			}
			// ReSharper disable once InconsistentNaming
			public Choice[] choices;
		}
		[Serializable]
		struct Request
		{
			// ReSharper disable NotAccessedField.Local
			[Serializable]
			public struct MessageParam
			{
				// ReSharper disable once InconsistentNaming
				public string role;
				// ReSharper disable once InconsistentNaming
				public string content;
			}

			// ReSharper disable once InconsistentNaming
			public string model;
			// ReSharper disable once InconsistentNaming
			public MessageParam[] messages;
			// ReSharper restore NotAccessedField.Local
		}
		public static void SendRequest(Config config, string systemContent, string userContent, Action<(UnityWebRequest request, Result result)> callback)
		{
			var msg = new Request
			{
				model = config.model,
				messages = new[]
				{
					new Request.MessageParam { role = "system", content = systemContent, }, new Request.MessageParam { role = "user", content = userContent, },
				},
			};
			var json = JsonConvert.SerializeObject(msg);
			var request = new UnityWebRequest(config.url, "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
			var operation = request.SendWebRequest();
			operation.completed += onRequestDone;
			return;
			void onRequestDone(AsyncOperation _)
			{
				if (!operation.isDone) return;
				if (request.result == UnityWebRequest.Result.Success)
				{
					Result result = default;
					try
					{
						result = JsonConvert.DeserializeObject<Result>(request.downloadHandler.text);
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Failed to deserialize LLM response: {e.Message}\nResponse text: {request.downloadHandler.text}");
					}
					callback?.Invoke((request, result));
					return;
				}
				callback?.Invoke((request, default));
			}
		}
		public static Awaitable<(UnityWebRequest request, Result result)> SendRequest(Config config, string systemContent, string userContent)
		{
			var awaitable = Awaitable<(UnityWebRequest, Result)>.Create(out var handle);
			SendRequest(config, systemContent, userContent, handle.Set);
			return awaitable;
		}
	}
}

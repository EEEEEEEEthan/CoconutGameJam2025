using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace Game.Utilities
{
	public static class FeishuRobot
	{
		struct Msg
		{
			// ReSharper disable once InconsistentNaming
			public string msg_type;
			public Content content;
			public string sign;
			public string timestamp;
			public override string ToString()
			{
				if (!string.IsNullOrEmpty(sign))
					return
						$"{{\"msg_type\":\"{msg_type}\",\"content\":{content},\"timestamp\":\"{timestamp}\",\"sign\":\"{sign}\"}}";
				return $"{{\"msg_type\":\"{msg_type}\",\"content\":{content}}}";
			}
		}
		struct Content
		{
			public string text;
			public readonly override string ToString() => $"{{\"text\":\"{text}\"}}";
		}
		sealed class Hmacsha256Final : HMACSHA256
		{
			public Hmacsha256Final(byte[] bytes) : base(bytes) { }
			public new byte[] HashFinal() => base.HashFinal();
		}
		public static void Send(string url, string content, string secret = null)
		{
			try
			{
				Debug.Log("Send to feishu, content:" + content + "\nurl:" + url);
				var msg = new Msg { msg_type = "text", content = new() { text = content, }, };
				if (!string.IsNullOrEmpty(secret))
				{
					var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
					var sign = GenSign(timestamp, secret);
					msg.sign = sign;
					msg.timestamp = timestamp.ToString();
				}
				var paramString = Encoding.UTF8.GetBytes(msg.ToString());
				var request = new UnityWebRequest(url, "POST") { uploadHandler = new UploadHandlerRaw(paramString), downloadHandler = new DownloadHandlerBuffer(), };
				request.SendWebRequest();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		static string GenSign(long timestamp, string secret)
		{
			var stringToSign = $"{timestamp}\n{secret}";
			var contentByte = Encoding.UTF8.GetBytes(stringToSign);
			using var hmacsha256 = new Hmacsha256Final(contentByte);
			return Convert.ToBase64String(hmacsha256.HashFinal());
		}
	}
}

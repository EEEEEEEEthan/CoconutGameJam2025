using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.ResourceManagement
{
	public class Loader
	{
		internal readonly string guid;
		internal readonly string pathWithoutExtension;
		Object[] resources;
		public IReadOnlyList<Object> Resources
		{
			get
			{
				if (resources is null)
					try
					{
						resources = UnityEngine.Resources.LoadAll(pathWithoutExtension);
					}
					catch (Exception e)
					{
						Debug.LogError($"Failed to load resources from path: {pathWithoutExtension} with guid: {guid}");
						Debug.LogException(e);
						resources = Array.Empty<Object>();
					}
				return resources;
			}
		}
		internal Loader(string pathWithoutExtension, string guid)
		{
			this.pathWithoutExtension = pathWithoutExtension;
			this.guid = guid;
		}
		public override string ToString() => $"{nameof(Loader)}({nameof(pathWithoutExtension)}:{pathWithoutExtension})";
	}
	public class Loader<T> : Loader
	{
		public T Main
		{
			get
			{
				if (Resources[0] is T main) return main;
				return default;
			}
		}
		internal Loader(string pathWithoutExtension, string guid) : base(pathWithoutExtension, guid) { }
	}
}

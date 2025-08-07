using System.Collections.Generic;
using UnityEngine;
namespace Game.ResourceManagement
{
	public class ResourceLoader
	{
		readonly string path;
		readonly string guid;
		Object[] resources;
		public IReadOnlyList<Object> Resources => resources ??= UnityEngine.Resources.LoadAll(path);
		protected ResourceLoader(string path, string guid) { }
	}
	public class ResourceLoader<T> : ResourceLoader
	{
		public T Main
		{
			get
			{
				if (Resources[0] is T main) return main;
				return default;
			}
		}
		public ResourceLoader(string path, string guid) : base(path, guid) { }
	}
}

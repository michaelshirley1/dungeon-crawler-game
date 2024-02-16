using System;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public abstract class AbstractMeshCombiner : MonoBehaviour {
		public abstract void Include(AbstractCombinable combinable);
		public abstract void Exclude(AbstractCombinable combinable);

		public abstract void Init();
		
		public abstract void Clear();
		
		public abstract Renderer[] GetRenderers();

		[Tooltip("List of key to accept combinable. Allow any if empty")]
		public string[] keys = Array.Empty<string>();
	}
}
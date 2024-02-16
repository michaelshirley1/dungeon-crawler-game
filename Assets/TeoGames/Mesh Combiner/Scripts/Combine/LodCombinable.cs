using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	[RequireComponent(typeof(Renderer))]
	[AddComponentMenu("Mesh Combiner/MC LOD Combinable")]
	public class LodCombinable : Combinable {
		private Mesh _StoredMesh;

		public override bool IsActive => cache.renderer.isVisible && base.IsActive;

		public override void ClearCache(bool force = false) {
			base.ClearCache(force);

			if (_StoredMesh) cache.mesh = _StoredMesh;
		}

		private void OnBecameVisible() {
			if (cache.isSkinnedMesh) {
				_StoredMesh = cache.skinnedMeshRenderer.sharedMesh;
				cache.skinnedMeshRenderer.sharedMesh = new Mesh { bounds = _StoredMesh.bounds };
			} else {
				_StoredMesh = cache.meshFilter.sharedMesh;
				cache.meshFilter.sharedMesh = new Mesh { bounds = _StoredMesh.bounds };
			}

			UpdateStatus();
		}

		private void OnBecameInvisible() {
			if (cache.isSkinnedMesh) {
				cache.skinnedMeshRenderer.sharedMesh = _StoredMesh;
				_StoredMesh = null;
			} else {
				cache.meshFilter.sharedMesh = _StoredMesh;
				_StoredMesh = null;
			}

			UpdateStatus();
		}

		public override void OnExclude() { }
		public override void OnInclude() { }
	}
}
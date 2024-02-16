using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Combine.MeshRendererManager;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	public abstract class AbstractChunkContainer : MonoBehaviour, IAsyncCombiner, IPostBakeAction {
		public bool SeparateBlendShapes { get; protected set; }

		public bool BakeMaterials { get; protected set; }

		public int MaxBuildTime { get; protected set; }

		public TargetRendererType RendererTypes { get; protected set; }

		public string[] Keys { get; protected set; }

		public abstract float Compability(AbstractCombinable obj);
		public abstract float Distance(Vector3 position);
		public abstract void Clear();

		public abstract Renderer[] GetRenderers();

		public abstract string GetKey(AbstractCombinable combinable);
		
		public abstract void Include(AbstractCombinable combinable, string key);

		public abstract void Exclude(AbstractCombinable combinable, string key);

		public abstract void PostBakeAction();

		public abstract Task UpdateTask { get; }

		public virtual void Init(
			string[] keys,
			TargetRendererType rendererTypes,
			int maxBuildTime,
			bool bakeMaterials,
			bool separateBlendShapes
		) {
			Keys = keys;
			RendererTypes = rendererTypes;
			MaxBuildTime = maxBuildTime;
			BakeMaterials = bakeMaterials;
			SeparateBlendShapes = separateBlendShapes;
		}

		protected MeshCombiner CreateMeshCombiner(string combinerName, Transform parent = null) {
			var obj = new GameObject { name = combinerName, transform = { parent = parent ? parent : transform } };

			obj.SetActive(false);
			var combiner = obj.AddComponent<MeshCombiner>();
			combiner.keys = Keys;
			combiner.asyncMode = false;
			combiner.rendererTypes = RendererTypes;
			combiner.maxBuildTime = MaxBuildTime;
			combiner.bakeMaterials = BakeMaterials;
			combiner.separateBlendShapes = SeparateBlendShapes;
			combiner.Init();
			obj.SetActive(true);

			return combiner;
		}

		protected Vector3 GetPosition(AbstractCombinable combinable) {
			return combinable.GetCache().renderer.bounds.center;
		}
	}
}
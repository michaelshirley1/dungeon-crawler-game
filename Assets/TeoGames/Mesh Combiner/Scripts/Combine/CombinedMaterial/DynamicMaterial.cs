using System.Collections.Generic;
using TeoGames.Mesh_Combiner.Scripts.BlendShape;
using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Rendering;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial {
	public class DynamicMaterial : BasicMaterial {
		public static readonly McProfiler Profiler = new McProfiler("DynamicMaterial > SetMesh");

		private static readonly Matrix4x4 DefaultMatrix = Matrix4x4.identity;

		public DynamicMaterial(Material mat, ShadowCastingMode shadow) : base(mat, shadow) {
			isStatic = false;
		}

		public override void SetMesh(
			int cID,
			MeshParser parser,
			Transform[] bones,
			int subMeshIndex,
			Mesh mesh,
			bool isStaticMesh,
			Matrix4x4 rootMat,
			Transform renTransform,
			BlendShapeConfiguration blendShapeConf
		) {
			using (Profiler.Auto()) {
				if (parser != null) mesh = parser.GetParsedMesh(mesh);

				var obj = new AdvancedCombineInstance {
					Combine = new CombineInstance {
						transform = isStaticMesh
							? rootMat * renTransform.localToWorldMatrix
							: DefaultMatrix,
						subMeshIndex = subMeshIndex,
						mesh = mesh,
					},
					Bones = bones
				};

				if (blendShapeConf.enabled) {
					obj.BlendShape = mesh
						.GetBlendShape(subMeshIndex, blendShapeConf)
						.Export(blendShapeConf, renTransform, cID);
					hasBlendShapes = true;
				}

				Meshes[cID * 100 + subMeshIndex] = obj;

				Updated();
			}
		}

		private readonly List<Transform> _Bones = new List<Transform>();

		public override void Build() {
			base.Build();

			_Bones.Clear();
			blendShape.Clear();

			foreach (var m in Meshes.Values) {
				if (m.Bones != null) _Bones.AddRange(m.Bones);
				if (m.BlendShape != null) blendShape.Extend(m.BlendShape);
				else if (hasBlendShapes) blendShape.length += m.Combine.mesh.GetSubMesh(m.Combine.subMeshIndex).vertexCount;
			}

			Bones = _Bones.ToArray();
		}
	}
}
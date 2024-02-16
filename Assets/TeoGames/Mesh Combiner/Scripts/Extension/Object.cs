using System;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Combine;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeoGames.Mesh_Combiner.Scripts.Extension {
	public static class ObjectExtension {
		private static Transform CONTAINER;
		private static Transform CURRENT_GROUP;
		private static Task CURRENT_TASK;
		private static int GROUP_ID;

#if UNITY_EDITOR
		[InitializeOnEnterPlayMode, InitializeOnLoadMethod]
		private static void OnEnterPlaymodeInEditor() {
			if (CONTAINER) Object.DestroyImmediate(CONTAINER);
			if (CURRENT_GROUP) Object.DestroyImmediate(CURRENT_GROUP);
			GROUP_ID = 0;
		}
#endif

		public static void ScheduleGroupDeletion(Transform group) {
			CURRENT_TASK = Task.CompletedTask
				.WaitForSeconds(2)
				.ContinueWith(
					() => {
						CURRENT_GROUP = null;
						CURRENT_TASK = null;
					}
				)
				.WaitForSeconds(3);

			CURRENT_TASK.WaitForSeconds(5).ContinueWith(
				() => {
					if (group) Object.Destroy(group.gameObject);
				}
			);
		}

		public static void CreateNewGroup() {
			CURRENT_GROUP = new GameObject {
				name = $"#{GROUP_ID++:0000} Group",
				transform = {
					parent = CONTAINER,
					localPosition = Vector3Extensions.Zero,
					localEulerAngles = Vector3Extensions.Zero,
					localScale = Vector3Extensions.Zero,
				}
			}.transform;
		}

		public static void LazyDestroy(this GameObject obj, Action onDestroy) {
			obj.LazyDestroy().ContinueWith(onDestroy);
		}

		public static Task LazyDestroy(this GameObject obj) {
			if (!obj) return Task.CompletedTask;

			if (!CONTAINER) {
				CONTAINER = new GameObject("[DM] Lazy Destroy Queue").transform;
				CONTAINER.gameObject.SetActive(false);

				CreateNewGroup();
				ScheduleGroupDeletion(CURRENT_GROUP);
			} else if (!CURRENT_GROUP) {
				CreateNewGroup();
				ScheduleGroupDeletion(CURRENT_GROUP);
			}

			var trans = obj.transform;
			trans.GetComponentsInChildren<AbstractCombinable>().ForEach(
				c => {
					c.gameObject.SetActive(false);
					c.enabled = false;
				}
			);
			trans.localScale = Vector3Extensions.Zero;
			trans.parent = CURRENT_GROUP;

			return CURRENT_TASK;
		}
	}
}
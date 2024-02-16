using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Extension {
	public static class TaskExtension {
		public static async Task WaitUntil(this Task task, Func<bool> action) {
			await task;

			while (!action()) {
				await Task.Yield();
			}
		}

		public static async Task WaitForUpdate(this Task task) {
			if (task.Status < TaskStatus.RanToCompletion) await task;

			await Task.Yield();
		}

		public static Task WaitForSeconds(this Task task, float seconds) {
			return task.ContinueWith(() => Task.Delay(Mathf.CeilToInt(seconds * 1000)));
		}

		public static Task ContinueWith(this Task task, Action action) {
			return task.ContinueWith(
				t => {
					if (t.Exception != null) {
						Debug.LogException(t.Exception);
						return t;
					}

					try {
						action();

						return Task.CompletedTask;
					} catch (Exception e) {
						Debug.LogException(e);
						return Task.FromException(e);
					}
				},
				TaskScheduler.FromCurrentSynchronizationContext()
			);
		}

		public static Task ContinueWith(this Task task, Func<Task> action) {
			var promise = new TaskCompletionSource<bool>();

			task.ContinueWith(
				async t => {
					if (t.Exception != null) {
						promise.SetException(t.Exception);
						return;
					}

					try {
						await action();
						promise.SetResult(true);
					} catch (Exception e) {
						Debug.LogException(e);
						promise.SetException(e);
					}
				},
				TaskScheduler.FromCurrentSynchronizationContext()
			);

			return promise.Task;
		}

		public static void Forget(this Task task) { }
	}
}
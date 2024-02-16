using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Extension;

namespace TeoGames.Mesh_Combiner.Scripts.Util {
	public class ThreadsPool {
		private readonly List<Task> _Queue = new List<Task>();

		public bool HasTasks => _Queue.Count > 0;

		public Task<Action> Schedule() {
			var promise = new TaskCompletionSource<Action>();
			var last = _Queue.Count > 0 ? _Queue.Last() : null;
			_Queue.Add(promise.Task);

			(last ?? Task.CompletedTask)
				.WaitForUpdate()
				.ContinueWith(() => { promise.SetResult(() => { _Queue.Remove(promise.Task); }); })
				.Forget();

			return promise.Task;
		}
	}
}
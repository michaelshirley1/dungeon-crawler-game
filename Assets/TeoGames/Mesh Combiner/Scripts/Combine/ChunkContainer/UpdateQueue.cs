﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Serialization;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	[Serializable]
	public class UpdateQueue {
		[FormerlySerializedAs("maxTimePerFrame")]
		[Tooltip("Maximum time that Update Queue can take in each frame for updating mesh position")]
		public float maxUpdateTimePerFrame = .5f;

		[Tooltip("Maximum time that Update Queue can take in each frame for processing new meshes")]
		public float maxInitTimePerFrame = 5f;

		[Tooltip("Distance between last update position and current position to trigger container check")]
		public float updateDistance = .5f;

		[Tooltip("Max amount of ticks before forcing container check")]
		public int forceUpdateAfter = 100;

		private readonly Dictionary<AbstractCombinable, UpdateQueueItem> _List =
			new Dictionary<AbstractCombinable, UpdateQueueItem>();

		private readonly Dictionary<AbstractCombinable, UpdateType> _Updates =
			new Dictionary<AbstractCombinable, UpdateType>();

		public bool IsStarted { get; protected set; }
		public int Count => _List.Count;
		public long Ticks { get; protected set; }
		private ChunkMeshCombiner _Owner;
		private Timer _UpdateTimer = new Timer();
		private Timer _InitTimer = new Timer();

		public void Clear() {
			_Updates.Clear();
			_List.Clear();
			IsStarted = false;
			Ticks = 0;
		}

		public void Start(ChunkMeshCombiner owner) {
			_UpdateTimer.MaxExecTime = maxUpdateTimePerFrame;
			_UpdateTimer.AsyncMode = true;

			_InitTimer.MaxExecTime = maxInitTimePerFrame;
			_InitTimer.AsyncMode = true;

			_Owner = owner;
			IsStarted = true;

			UpdateList().Forget();
		}

		public void Stop() {
			IsStarted = false;
		}

		public void Schedule(AbstractCombinable combinable) => _Updates[combinable] = UpdateType.Include;

		public void Remove(AbstractCombinable combinable) => _Updates[combinable] = UpdateType.Exclude;

		private async Task UpdateList() {
			Ticks = 0;
			await Task.Yield();

			while (IsStarted && _Owner) {
				await ApplyUpdates();

				_UpdateTimer.Start();
				var i = 0;
				foreach (var (combinable, item) in _List) {
					if (i++ % 10 == 0 && _UpdateTimer.IsTimeoutRequired) {
						await _UpdateTimer.Wait();
						if (!IsStarted) return;
					}

					ProcessCombinable(combinable, item);
				}

				_UpdateTimer.Stop();
				Ticks++;

				await Task.Yield();
			}
		}

		private async Task ApplyUpdates() {
			if (!_Updates.Any()) return;

			_InitTimer.Start();
			var i = 0;
			var cpy = new Dictionary<AbstractCombinable, UpdateType>(_Updates);
			_Updates.Clear();

			foreach (var pair in cpy) {
				if (i++ % 10 == 0 && _InitTimer.IsTimeoutRequired) {
					await _InitTimer.Wait();
					if (!IsStarted) return;
				}

				if (pair.Value == UpdateType.Include) {
					var item = _Owner.InternalInclude(pair.Key);
					if (pair.Key.IsStatic) continue;

					item.transform = pair.Key.transform;
					item.position = item.transform.position;
					_List[pair.Key] = item;
				} else _List.Remove(pair.Key);
			}

			_InitTimer.Stop();
		}

		private void ProcessCombinable(AbstractCombinable combinable, UpdateQueueItem item) {
			if (!combinable) {
				Remove(combinable);
				return;
			}

			var pos = item.transform.position;
			if (item.ticks < forceUpdateAfter && Vector3.Distance(item.position, pos) < updateDistance) {
				item.ticks++;
				return;
			}

			try {
				_Owner.InternalInclude(combinable, item);
				item.ticks = 0;
				item.position = pos;
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
	}
}
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TeoGames.Mesh_Combiner.Scripts.Profile {
	public class Timer {
		public long Diff => NanoTime() - _Start;
		public long DiffMs => Diff / 100000;

		private float _MaxExecTime = 50;

		public float MaxExecTime {
			get => _MaxExecTime / 10;
			set => _MaxExecTime = value * 10;
		}

		public bool AsyncMode = false;

		public bool IsTimeoutRequired => AsyncMode && DiffMs > _MaxExecTime;

		private long _Start;

		public async Task Wait() {
			Stop();
			await Task.Yield();
			Start();
		}

		public void Start() => _Start = NanoTime();

		public void Stop() => ProfilerModule.BakeTime.Value += Diff;

		public static int MS() => (int)(NanoTime() / 1000000);
		
		private static long NanoTime() {
			var nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}
	}
}
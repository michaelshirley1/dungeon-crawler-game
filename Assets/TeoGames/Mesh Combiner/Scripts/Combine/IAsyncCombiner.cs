using System.Threading.Tasks;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public interface IAsyncCombiner {
		public Task UpdateTask { get; }
	}
}
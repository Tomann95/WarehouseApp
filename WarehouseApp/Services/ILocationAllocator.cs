using System.Threading.Tasks;

namespace WarehouseApp.Services
{
    public interface ILocationAllocator
    {
        /// <summary>
        /// Zwraca Id lokalizacji docelowej dla danego produktu.
        /// </summary>
        /// <param name="productId">Id towaru</param>
        /// <returns>Id lokalizacji lub null, jeśli żadnej nie znaleziono</returns>
        Task<int?> GetBestTargetLocationAsync(int productId);
    }
}


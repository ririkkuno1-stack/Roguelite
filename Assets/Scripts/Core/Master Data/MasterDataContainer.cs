using System.Collections.Generic;
namespace Core.MasterData
{
    public interface MasterDataContainer<T> where T : IMsterData
    {
        
        List<T> Records { get; }
    }
   
}

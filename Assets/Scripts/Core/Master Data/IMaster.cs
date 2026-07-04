using UnityEngine;
namespace Core.MasterData
{
    /// <summary>
    /// 一行のデータが必ずIDを持つことを保証する
    /// </summary>

    public interface IMsterData
    {
        public ulong Id { get; }

    }
}
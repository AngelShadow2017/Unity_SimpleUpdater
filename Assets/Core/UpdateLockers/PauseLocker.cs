using UnityEngine;
// ReSharper disable once CheckNamespace
namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{
    /// <summary>
    /// 基于游戏状态的锁（不用于帧同步，用于游戏暂停）
    /// </summary>
    public class PauseLock : IFrameLock
    {
        public FrameLockManager.FrameLockType Id => FrameLockManager.FrameLockType.Pause;

        public bool IsOpen { get; private set; } = true;
        public void Open()
        {
            IsOpen = true;
        }
        
        public void Close()
        {
            IsOpen = false;
        }
    }
}
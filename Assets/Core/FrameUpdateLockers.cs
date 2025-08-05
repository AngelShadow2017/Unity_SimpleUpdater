using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{
    
    /// <summary>
    /// 管理所有帧同步锁的类
    /// </summary>
    public class FrameLockManager
    {
        public enum FrameLockType
        {
            Default,
            Pause
        }
        
        private Dictionary<FrameLockManager.FrameLockType, IFrameLock> _locks = new Dictionary<FrameLockManager.FrameLockType, IFrameLock>();
        
        /// <summary>
        /// 是否所有锁都已打开，允许帧更新
        /// </summary>
        public bool AllLocksOpen
        {
            get
            {
                bool all = true;
                foreach (var l in _locks.Values)
                {
                    if (!l.IsOpen)
                    {
                        all = false;
#if UNITY_EDITOR
                        //Debug.Log("Locking By "+l.Id);
#endif
                        break;
                    }
                }

                return _locks.Count == 0 || all;
            }
        }
        /// <summary>
        /// 添加一个锁
        /// </summary>
        /// <param name="frameLock">要添加的锁</param>
        public void AddLock(IFrameLock frameLock)
        {
            if (!_locks.ContainsKey(frameLock.Id))
            {
                _locks.Add(frameLock.Id, frameLock);
            }
            else
            {
                Debug.LogWarning($"锁 {frameLock.Id} 已存在，无法重复添加");
            }
        }
        
        /// <summary>
        /// 移除一个锁
        /// </summary>
        /// <param name="lockId">要移除的锁的ID</param>
        public void RemoveLock(FrameLockManager.FrameLockType lockId)
        {
            if (_locks.ContainsKey(lockId))
            {
                _locks.Remove(lockId);
            }
        }
        
        /// <summary>
        /// 获取一个锁
        /// </summary>
        /// <param name="lockId">锁的ID</param>
        /// <returns>对应的锁，如果不存在则返回null</returns>
        public IFrameLock GetLock(FrameLockManager.FrameLockType lockId)
        {
            return _locks.TryGetValue(lockId, out var frameLock) ? frameLock : null;
        }
        
        /// <summary>
        /// 打开特定的锁
        /// </summary>
        /// <param name="lockId">要打开的锁的ID</param>
        public void OpenLock(FrameLockManager.FrameLockType lockId)
        {
            var frameLock = GetLock(lockId);
            frameLock?.Open();
        }
        
        /// <summary>
        /// 关闭特定的锁
        /// </summary>
        /// <param name="lockId">要关闭的锁的ID</param>
        public void CloseLock(FrameLockManager.FrameLockType lockId)
        {
            var frameLock = GetLock(lockId);
            frameLock?.Close();
        }
        
        /// <summary>
        /// 清除所有锁
        /// </summary>
        public void ClearAllLocks()
        {
            _locks.Clear();
        }

        // 其他现有方法...
    }

    /// <summary>
    /// 帧同步锁接口，用于控制游戏更新流程
    /// </summary>
    public interface IFrameLock
    {
        /// <summary>
        /// 锁的唯一标识符
        /// </summary>
        FrameLockManager.FrameLockType Id { get; }
        /// <summary>
        /// 锁的状态，true表示锁已打开（允许更新），false表示锁已关闭（阻止更新）
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// 打开锁，允许帧更新
        /// </summary>
        void Open();
        
        /// <summary>
        /// 关闭锁，阻止帧更新
        /// </summary>
        void Close();
    }
}
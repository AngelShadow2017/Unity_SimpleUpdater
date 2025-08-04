using System;
using UnityEngine;

namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{
    // 标记类需要手动 Update 注册
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ManualUpdaterAttribute : Attribute
    {
    }

    // 两个空接口，表明类支持 ManualUpdate 或 ManualFixedUpdate
    public interface IManualUpdater
    {
    }

    public interface IFixedManualUpdater
    {
    }

    public class ManualUpdateManager
    {
        public enum UpdateType
        {
            Update,
            FixedUpdate
        }
        public static void Register(MonoBehaviour script,UpdateType type,Action function)
        {
            Debug.Log("I am fine thank you");
        }
        public static void Unregister(MonoBehaviour script,UpdateType type)
        {
            Debug.Log("I am fine thank you, leave me alone");
        }
    }
}
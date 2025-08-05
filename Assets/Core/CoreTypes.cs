using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{

    // 标记类需要手动 Update 注册
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ManualUpdaterAttribute : Attribute
    {
        public ManualUpdaterAttribute(ManualUpdateManager.UpdateOrder order = ManualUpdateManager.UpdateOrder.Normal)
        {
            Order = order;
        }
        public ManualUpdateManager.UpdateOrder Order { get; }
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
        public enum UpdateOrder
        {
            FakePosition = 0,
            Normal = 1
        }

        private static readonly Dictionary<UpdateOrder, int> shunts = new();
        private static readonly List<(Type,UpdateOrder[])> shuntConfigs = new()
        {
            (typeof(MonobehaviourManagers),new UpdateOrder[] { UpdateOrder.FakePosition ,UpdateOrder.Normal})
        };
        private static readonly List<MonobehaviourManagers> managers = new List<MonobehaviourManagers>();
        private static bool inited = false;
        private static void Init()
        {
            if (inited)
            {
                return;
            }

            inited = true;
            var shuntCount = shuntConfigs.Count;
            for (var index = 0; index < shuntCount; index++)
            {
                var config = shuntConfigs[index];
                if (!config.Item1.IsAssignableFrom(typeof(MonobehaviourManagers))) continue;
                managers.Add(new GameObject().AddComponent(config.Item1) as MonobehaviourManagers);
                managers[^1].handles.AddRange(config.Item2);// 添加所有的 UpdateOrder
                managers[^1].Init();
                foreach (var order in config.Item2)
                {
                    shunts.TryAdd(order, index);
                }
            }
        }

        static ManualUpdateManager()
        {
            Init();
        }
        public static void Register(MonoBehaviour script,UpdateType type,Action function,UpdateOrder order=UpdateOrder.Normal)
        {
            Debug.Log("I am fine thank you"+order);
            managers[shunts[order]].Register(script,type,function,order);
        }
        public static void Unregister(MonoBehaviour script,UpdateType type,UpdateOrder order=UpdateOrder.Normal)
        {
            Init();
            Debug.Log("I am fine thank you, leave me alone");
            managers[shunts[order]].Unregister(script,type,order);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
// ReSharper disable once CheckNamespace
namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{
    

    class BehaviourComponents<T> where T : class
    {
        public int count;
        public Dictionary<int,T>[] behaviourUpdates;
        public Dictionary<int,T>[] readyToUpdate;
        public List<int>[] readyToRemove;
        private int deltaSub=0;//标记变化减少，和增加的最大值
        private const int trimThreshold = 15000; // 当元素数量超过这个阈值时，才进行TrimExcess
        public BehaviourComponents(int count)
        {
            this.count = count;
            
            behaviourUpdates = new Dictionary<int, T>[count];
            readyToUpdate = new Dictionary<int, T>[count];
            readyToRemove = new List<int>[count];
            for (int i = 0; i < count; i++)
            {
                behaviourUpdates[i] = new Dictionary<int, T>();
                readyToUpdate[i] = new Dictionary<int, T>();
                readyToRemove[i] = new List<int>();
            }
        }

        public void Add(int behaviour, T action,int index)
        {
            readyToUpdate[index][behaviour] = action;
            readyToRemove[index].Remove(behaviour);
        }
        public void Remove(int behaviour, int index)
        {
            //Debug.Log("readyRemoveID: "+behaviour);
            readyToUpdate[index].Remove(behaviour);
            readyToRemove[index].Add(behaviour);
            deltaSub++;
        }
        public Enumerator GetEnumerator() => new (behaviourUpdates);
        public void Appends()
        {
            for(int index = 0; index < count; index++)
            {
                foreach (var monoBehaviour in readyToUpdate[index])
                {
                    //Debug.Log("append "+monoBehaviour.Value);
                    if (!behaviourUpdates[index].TryAdd(monoBehaviour.Key, monoBehaviour.Value))
                    {
                        Debug.LogError("你没有删成功。");
                        throw new Exception($"Failed to add {monoBehaviour.Key} to behaviourUpdates at index {index}. It may already exist.");
                    }
                }
                readyToUpdate[index].Clear();
            }
        }

        public void FlushRemove()
        {
            bool needToTrim = deltaSub > trimThreshold;
            for (int index = 0; index < count; index++)
            {
                foreach (var monoBehaviour in readyToRemove[index])
                {
                    if (behaviourUpdates[index].ContainsKey(monoBehaviour))
                    {
                        behaviourUpdates[index][monoBehaviour] = null;
                        behaviourUpdates[index].Remove(monoBehaviour);
                    }
                    readyToUpdate[index].Remove(monoBehaviour);
                }
                readyToRemove[index].Clear();
                if (needToTrim)
                {
                    behaviourUpdates[index].TrimExcess();
                    readyToUpdate[index].TrimExcess();
                }
            }
            deltaSub = 0;
        }
        public struct Enumerator
        {
            private readonly Dictionary<int, T>[] _arrays;  // 引用外部的 behaviourUpdates
            private int _outerIndex;                        // 外层字典数组的下标
            private Dictionary<int, T>.Enumerator _inner;   // 内层字典的 struct 枚举器

            public Enumerator(Dictionary<int, T>[] arrays)
            {
                _arrays     = arrays;
                _outerIndex = -1;
                _inner      = default;  
            }

            // 由 foreach 自动调用
            public bool MoveNext()
            {
                // 尝试从当前 inner 枚举器里拉下一个元素
                if (_outerIndex>=0&&_inner.MoveNext())

                    return true;

                // inner 用尽后，切换到下一个非空字典并重置 inner
                while (++_outerIndex < _arrays.Length)
                {
                    _inner = _arrays[_outerIndex].GetEnumerator();
                    if (_inner.MoveNext())
                        return true;
                }

                return false;
            }

            // 由 foreach 自动访问
            public T Current => _inner.Current.Value;
        }
    }
    //实现和unity一样的更新逻辑，在所有的Update，FixedUpdate之前,Update，FixedUpdate之后都会批量执行Start
    [DefaultExecutionOrder(int.MinValue)]
    public class StarterManager:MonoBehaviour
    {
        Dictionary<ManualUpdateManager.UpdateOrder, int> orderDict = new Dictionary<ManualUpdateManager.UpdateOrder, int>();
        private HashSet<IManualStarter>[] runningTemps;
        private HashSet<IManualStarter>[] starters;
        private int addCounter = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterStarter<T>(T starter,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal) where T:MonoBehaviour, IManualStarter
        {
            addCounter++;
            starters[orderDict[order]].Add(starter);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Awake()
        {
            List<ManualUpdateManager.UpdateOrder> orders = ((ManualUpdateManager.UpdateOrder[])Enum.GetValues(
                typeof(ManualUpdateManager.UpdateOrder))).ToList();
            orders.Sort();
            runningTemps = new HashSet<IManualStarter>[orders.Count];
            starters = new HashSet<IManualStarter>[orders.Count];
            for (var i = 0; i < orders.Count; i++)
            {
                orderDict[orders[i]] = i;
                runningTemps[i] = new HashSet<IManualStarter>();
                starters[i] = new HashSet<IManualStarter>();
            }
            DontDestroyOnLoad(gameObject);
        }
        public void Flush(int maxLoop = 1)
        {
            if (!ManualUpdateManager.frameLockManager.AllLocksOpen)
            {
                return;
            }
            while (addCounter>0&&maxLoop > 0)
            {
                addCounter = 0;
                (starters,runningTemps)= (runningTemps, starters);
                foreach (var manualStarterse in runningTemps)
                {
                    foreach (var manualStarter in manualStarterse)
                    {
                        if (manualStarter==null||manualStarter.Equals(null))
                        {
                            continue;
                        }
                        manualStarter.__F_ManualStart_AutoGenerated__();
                    }
                    manualStarterse.Clear();
                }
                maxLoop--;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update()
        {
            Flush(2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FixedUpdate()
        {
            Flush(2);
        }
    }
    [DefaultExecutionOrder(int.MaxValue)]
    public class LateStarterInjector : MonoBehaviour
    {
        public StarterManager starterManager;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LateUpdate()
        {
            starterManager.Flush();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FixedUpdate()
        {
            starterManager.Flush();
        }
    }
    [DefaultExecutionOrder(-1)]
    public class MonobehaviourManagers : MonoBehaviour
    {
        public List<ManualUpdateManager.UpdateOrder> handles = new List<ManualUpdateManager.UpdateOrder>();
        Dictionary<ManualUpdateManager.UpdateOrder, int> orderDict = new Dictionary<ManualUpdateManager.UpdateOrder, int>();
        private BehaviourComponents<IManualUpdater> updates;
        private BehaviourComponents<IFixedManualUpdater> fixedUpdates;
        private bool inited = false;
        public List<MonobehaviourManagers> allManagers;
        public bool isFirstUpdateGroup = false;//是否是掌管第一个UpdateOrder，只有掌管第一个UpdateOrder的才可以flush全体
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Init();
#if UNITY_EDITOR
            gameObject.name = string.Join("_", handles.Select(h => h.ToString())) + "_UpdateManager";
#endif
        }

        public void Init()
        {
            if (inited)
            {
                return;
            }

            inited = true;
            handles.Sort();
            int len = handles.Count;
            for (var i = 0; i < len; i++)
            {
                orderDict.Add(handles[i],i);
            }
            updates = new(len);
            fixedUpdates = new (len);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterUpdate<T>(T script,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal) where T:MonoBehaviour,IManualUpdater
        {
            updates.Add(script.GetInstanceID(),script,orderDict[order]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterFixed<T>(T script,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal) where T:MonoBehaviour,IFixedManualUpdater
        {
            fixedUpdates.Add(script.GetInstanceID(),script,orderDict[order]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterUpdate(MonoBehaviour script,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal)
        {
            updates.Remove(script.GetInstanceID(),orderDict[order]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterFixed(MonoBehaviour script,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal)
        {
            fixedUpdates.Remove(script.GetInstanceID(),orderDict[order]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlushUpdate()
        {
            updates.FlushRemove();
            updates.Appends();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlushFixedUpdate()
        {
            fixedUpdates.FlushRemove();
            fixedUpdates.Appends();
        }
        //批量刷新所有的Update和FixedUpdate，由第一个UpdateOrder的MonobehaviourManager调用
        //如果不是第一个UpdateOrder的MonobehaviourManager调用会被忽略
        //避免在Update和FixedUpdate中生成其他调用顺序的物体，然后导致调用顺序混乱
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BatchFlushUpdate()
        {
            foreach (var monobehaviourManagerse in allManagers)
            {
                monobehaviourManagerse.FlushUpdate();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BatchFlushFixedUpdate()
        {
            foreach (var monobehaviourManagerse in allManagers)
            {
                monobehaviourManagerse.FlushFixedUpdate();
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (!ManualUpdateManager.frameLockManager.AllLocksOpen)
            {
                return;
            }

            if (isFirstUpdateGroup)
            {
                BatchFlushUpdate();
            }
            foreach (var manualUpdater in updates)
            {
                manualUpdater.__F_ManualUpdate_AutoGenerated__();
            }
        }
        void FixedUpdate()
        {
            if (!ManualUpdateManager.frameLockManager.AllLocksOpen)
            {
                return;
            }
            if (isFirstUpdateGroup)
            {
                BatchFlushFixedUpdate();
            }
            foreach (var manualUpdater in fixedUpdates)
            {
                manualUpdater.__F_ManualFixedUpdate_AutoGenerated__();
            }
        }
    }
    [DefaultExecutionOrder(-1001)]
    public class BeforeUpdateMonobehaviourManager:MonobehaviourManagers{}
}
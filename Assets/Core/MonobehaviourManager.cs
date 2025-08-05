using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
// ReSharper disable once CheckNamespace
namespace ZeroAs.ZeroAs_Core.ManualUpdaters
{
    class BehaviourComponents
    {
        public int count;
        public Dictionary<MonoBehaviour,Action>[] behaviourUpdates;
        public Dictionary<MonoBehaviour,Action>[] readyToUpdate;
        public List<MonoBehaviour>[] readyToRemove;
        public BehaviourComponents(int count)
        {
            this.count = count;
            
            behaviourUpdates = new Dictionary<MonoBehaviour, Action>[count];
            readyToUpdate = new Dictionary<MonoBehaviour, Action>[count];
            readyToRemove = new List<MonoBehaviour>[count];
            for (int i = 0; i < count; i++)
            {
                behaviourUpdates[i] = new Dictionary<MonoBehaviour, Action>();
                readyToUpdate[i] = new Dictionary<MonoBehaviour, Action>();
                readyToRemove[i] = new List<MonoBehaviour>();
            }
        }

        public void Add(MonoBehaviour behaviour, Action action,int index)
        {
            readyToUpdate[index][behaviour] = action;
            readyToRemove[index].Remove(behaviour);
        }
        public void Remove(MonoBehaviour behaviour, int index)
        {
            readyToUpdate[index].Remove(behaviour);
            readyToRemove[index].Add(behaviour);
        }
        public void Traverse()
        {
            for (int index = 0; index < count; index++)
            {
                foreach (var update in behaviourUpdates[index])
                {
                    if (update.Key)
                    {
                        update.Value();
                    }
                    else
                    {
                        Remove(update.Key,index);
                    }
                }
            }
        }
        public void Appends()
        {
            for(int index = 0; index < count; index++)
            {
                foreach (var monoBehaviour in readyToUpdate[index])
                {
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
            for (int index = 0; index < count; index++)
            {
                foreach (var monoBehaviour in readyToRemove[index])
                {
                    behaviourUpdates[index].Remove(monoBehaviour);
                    readyToUpdate[index].Remove(monoBehaviour);
                }
                readyToRemove[index].Clear();
            }
        }
    }
    public class MonobehaviourManagers : MonoBehaviour
    {
        public List<ManualUpdateManager.UpdateOrder> handles = new List<ManualUpdateManager.UpdateOrder>();
        Dictionary<ManualUpdateManager.UpdateOrder, int> orderDict = new Dictionary<ManualUpdateManager.UpdateOrder, int>();
        BehaviourComponents updates,fixedUpdates;
        private bool inited = false;

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
            updates = new BehaviourComponents(len);
            fixedUpdates = new BehaviourComponents(len);
        }
        public void Register(MonoBehaviour script,ManualUpdateManager.UpdateType type,Action function,ManualUpdateManager.UpdateOrder order)
        {
            switch (type)
            {
                case ManualUpdateManager.UpdateType.FixedUpdate:
                    fixedUpdates.Add(script,function,orderDict[order]);
                    break;
                case ManualUpdateManager.UpdateType.Update:
                    updates.Add(script,function,orderDict[order]);
                    break;
            }
        }
        public void Unregister(MonoBehaviour script,ManualUpdateManager.UpdateType type,ManualUpdateManager.UpdateOrder order=ManualUpdateManager.UpdateOrder.Normal)
        {
            switch (type)
            {
                case ManualUpdateManager.UpdateType.FixedUpdate:
                    fixedUpdates.Remove(script,orderDict[order]);
                    break;
                case ManualUpdateManager.UpdateType.Update:
                    updates.Remove(script,orderDict[order]);
                    break;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (!ManualUpdateManager.frameLockManager.AllLocksOpen)
            {
                return;
            }
            updates.FlushRemove();
            updates.Appends();
            updates.Traverse();
        }
        void FixedUpdate()
        {
            if (!ManualUpdateManager.frameLockManager.AllLocksOpen)
            {
                return;
            }
            fixedUpdates.FlushRemove();
            fixedUpdates.Appends();
            fixedUpdates.Traverse();
        }
    }
}
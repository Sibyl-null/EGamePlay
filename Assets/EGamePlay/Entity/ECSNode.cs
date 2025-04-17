using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public sealed class ECSNode : Entity
    {
        public static ECSNode Instance { get; private set; }
        
        public Dictionary<Type, List<Entity>> Entities { get; private set; } = new();
        public List<Component> AllComponents { get; private set; } = new();
        public List<UpdateComponent> UpdateComponents { get; private set; } = new();

        private ECSNode()
        {
        }

        public static ECSNode Create()
        {
            if (Instance != null)
                return Instance;
            
            Instance = new ECSNode();
            Instance.AddComponent<GameObjectComponent>();
            UnityEngine.Object.DontDestroyOnLoad(Instance.GetComponent<GameObjectComponent>().GameObject);
            return Instance;
        }

        public static void Destroy()
        {
            Destroy(Instance);
            Instance = null;
        }

        public override void Update()
        {
            if (AllComponents.Count == 0)
                return;
            
            for (int i = AllComponents.Count - 1; i >= 0; i--)
            {
                Component item = AllComponents[i];
                if (item.IsDisposed)
                {
                    AllComponents.RemoveAt(i);
                    continue;
                }
                
                if (item.Disable)
                    continue;
                
                item.Update();
            }
        }

        public override void FixedUpdate()
        {
            if (AllComponents.Count == 0)
                return;
            
            for (int i = AllComponents.Count - 1; i >= 0; i--)
            {
                Component item = AllComponents[i];
                if (item.IsDisposed)
                {
                    AllComponents.RemoveAt(i);
                    continue;
                }
                
                if (item.Disable)
                    continue;
                
                item.FixedUpdate();
            }
        }
    }
}
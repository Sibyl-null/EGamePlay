using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public class Component
    {
        private bool _enable = false;
        
        public Entity Entity { get; set; }
        public bool IsDisposed { get; set; }
        public Dictionary<long, Entity> Id2Children { get; private set; } = new Dictionary<long, Entity>();
        
        public virtual bool DefaultEnable { get; set; } = true;
        
        public bool Enable
        {
            get => _enable;
            set
            {
                if (_enable == value)
                    return;
                
                _enable = value;
                if (_enable) OnEnable();
                else OnDisable();
            }
        }

        public T GetEntity<T>() where T : Entity
        {
            return Entity as T;
        }

        public virtual void Awake()
        {
        }

        public virtual void Awake(object initData)
        {
        }

        public virtual void Setup()
        {
        }

        public virtual void Setup(object initData)
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void OnDestroy()
        {
        }

        private void Dispose()
        {
            Enable = false;
            IsDisposed = true;
        }

        public static void Destroy(Component component)
        {
            try
            {
                component.OnDestroy();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            component.Dispose();
        }

        public T Publish<T>(T tEvent) where T : class
        {
            Entity.Publish(tEvent);
            return tEvent;
        }

        public void Subscribe<T>(Action<T> action) where T : class
        {
            Entity.Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            Entity.UnSubscribe(action);
        }
    }
}
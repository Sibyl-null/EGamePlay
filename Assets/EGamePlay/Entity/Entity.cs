using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public abstract partial class Entity
    {
        private string _name;
        private Entity _parent;
        
        public long Id { get; private set; }
        public long InstanceId { get; private set; }
        public List<Entity> Children { get; } = new();
        public Dictionary<long, Entity> Id2Children { get; } = new();
        public Dictionary<Type, List<Entity>> Type2Children { get; } = new();
        public Dictionary<Type, Component> Components { get; } = new();
        
        public Entity Parent => _parent;
        public bool IsDisposed => InstanceId == 0;
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                GetComponent<GameObjectComponent>().OnNameChanged(_name);
            }
        }

        protected Entity()
        {
            if (this is ECSNode)
                return;

            if (!GetType().Name.Contains("OnceWaitTimer"))
                AddComponent<GameObjectComponent>();
        }

        
        // ----------------------------------------------------------------
        // 生命周期
        // ----------------------------------------------------------------
        
        public virtual void Awake()
        {
        }

        public virtual void Awake(object initData)
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Start(object initData)
        {
        }

        public virtual void OnSetParent(Entity preParent, Entity nowParent)
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
            if (Children.Count > 0)
            {
                for (int i = Children.Count - 1; i >= 0; i--)
                    Destroy(Children[i]);
                
                Children.Clear();
                Type2Children.Clear();
            }

            Parent?.RemoveChild(this);
            foreach (Component component in Components.Values)
            {
                component.Enable = false;
                Component.Destroy(component);
            }
            
            Components.Clear();
            InstanceId = 0;
            if (ECSNode.Entities.ContainsKey(GetType()))
            {
                ECSNode.Entities[GetType()].Remove(this);
            }
        }
        
        
        // ----------------------------------------------------------------
        // 组件
        // ----------------------------------------------------------------

        public T GetParent<T>() where T : Entity
        {
            return _parent as T;
        }

        public T As<T>() where T : class
        {
            return this as T;
        }

        public bool As<T>(out T entity) where T : Entity
        {
            entity = this as T;
            return entity != null;
        }

        public T AddComponent<T>() where T : Component
        {
            T component = Activator.CreateInstance<T>();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            ECSNode.AllComponents.Add(component);
            
            component.Awake();
            component.Setup();
            
            GetComponent<GameObjectComponent>().OnAddComponent(component);
            component.Enable = component.DefaultEnable;
            return component;
        }

        public T AddComponent<T>(object initData) where T : Component
        {
            T component = Activator.CreateInstance<T>();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            ECSNode.AllComponents.Add(component);
            
            component.Awake(initData);
            component.Setup(initData);
            
            GetComponent<GameObjectComponent>().OnAddComponent(component);
            component.Enable = component.DefaultEnable;
            return component;
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component component = Components[typeof(T)];
            if (component.Enable)
                component.Enable = false;
            
            Component.Destroy(component);
            Components.Remove(typeof(T));
            GetComponent<GameObjectComponent>().OnRemoveComponent(component);
        }

        public T GetComponent<T>() where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
        }

        public bool HasComponent<T>() where T : Component
        {
            return Components.ContainsKey(typeof(T));
        }

        public bool TryGet<T>(out T component) where T : Component
        {
            if (Components.TryGetValue(typeof(T), out Component c))
            {
                component = c as T;
                return true;
            }
            
            component = null;
            return false;
        }


        // ----------------------------------------------------------------
        // 子实体
        // ----------------------------------------------------------------
        
        private void SetParent(Entity parent)
        {
            var preParent = Parent;
            preParent?.RemoveChild(this);
            _parent = parent;
            parent.GetComponent<GameObjectComponent>().OnAddChild(this);
            OnSetParent(preParent, parent);
        }

        public void SetChild(Entity child)
        {
            Children.Add(child);
            Id2Children.Add(child.Id, child);
            
            if (!Type2Children.ContainsKey(child.GetType()))
                Type2Children.Add(child.GetType(), new List<Entity>());
            
            Type2Children[child.GetType()].Add(child);
            child.SetParent(this);
        }

        public void RemoveChild(Entity child)
        {
            Children.Remove(child);
            Id2Children.Remove(child.Id);
            
            if (Type2Children.ContainsKey(child.GetType()))
                Type2Children[child.GetType()].Remove(child);
        }

        public Entity AddChild(Type entityType)
        {
            Entity entity = NewEntity(entityType);
            SetupEntity(entity, this);
            return entity;
        }

        public Entity AddChild(Type entityType, object initData)
        {
            Entity entity = NewEntity(entityType);
            SetupEntity(entity, this, initData);
            return entity;
        }

        public T AddChild<T>() where T : Entity
        {
            return AddChild(typeof(T)) as T;
        }

        public T AddIdChild<T>(long id) where T : Entity
        {
            Entity entity = NewEntity(typeof(T), id);
            SetupEntity(entity, this);
            return entity as T;
        }

        public T AddChild<T>(object initData) where T : Entity
        {
            return AddChild(typeof(T), initData) as T;
        }

        public Entity GetIdChild(long id)
        {
            Id2Children.TryGetValue(id, out Entity entity);
            return entity;
        }

        public T GetIdChild<T>(long id) where T : Entity
        {
            Id2Children.TryGetValue(id, out Entity entity);
            return entity as T;
        }

        public T GetChild<T>(int index = 0) where T : Entity
        {
            if (Type2Children.ContainsKey(typeof(T)) == false)
                return null;
            
            if (Type2Children[typeof(T)].Count <= index)
                return null;
            
            return Type2Children[typeof(T)][index] as T;
        }

        public Entity[] GetChildren()
        {
            return Children.ToArray();
        }

        public T[] GetTypeChildren<T>() where T : Entity
        {
            return Type2Children[typeof(T)].ConvertAll(x => x.As<T>()).ToArray();
        }

        public Entity Find(string name)
        {
            foreach (var item in Children)
                if (item._name == name) return item;
            
            return null;
        }

        public T Find<T>(string name) where T : Entity
        {
            if (Type2Children.TryGetValue(typeof(T), out var chidren))
            {
                foreach (var item in chidren)
                {
                    if (item._name == name)
                        return item as T;
                }
            }
            return null;
        }

        
        // ----------------------------------------------------------------
        // 事件
        // ----------------------------------------------------------------

        public T Publish<T>(T tEvent) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                return tEvent;
            }
            eventComponent.Publish(tEvent);
            return tEvent;
        }

        public SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            return eventComponent.Subscribe(action);
        }

        public SubscribeSubject Subscribe<T>(Action<T> action, Entity disposeWith) where T : class
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            return eventComponent.Subscribe(action).DisposeWith(disposeWith);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.UnSubscribe(action);
        }

        public void FireEvent(string eventType)
        {
            FireEvent(eventType, this);
        }

        public void FireEvent(string eventType, Entity entity)
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.FireEvent(eventType, entity);
        }

        public void OnEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            eventComponent.OnEvent(eventType, action);
        }

        public void OffEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.OffEvent(eventType, action);
        }
    }
}
using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public abstract partial class Entity
    {
        public static ECSNode ECSNode => ECSNode.Instance;

        public static T Create<T>() where T : Entity
        {
            return Create(typeof(T)) as T;
        }

        public static T Create<T>(object initData) where T : Entity
        {
            return Create(typeof(T), initData) as T;
        }

        private static Entity Create(Type entityType)
        {
            Entity entity = NewEntity(entityType);
            SetupEntity(entity, ECSNode);
            return entity;
        }

        private static Entity Create(Type entityType, object initData)
        {
            Entity entity = NewEntity(entityType);
            SetupEntity(entity, ECSNode, initData);
            return entity;
        }
        
        private static Entity NewEntity(Type entityType, long id = 0)
        {
            Entity entity = Activator.CreateInstance(entityType) as Entity;
            entity!.InstanceId = IdFactory.NewInstanceId();
            entity.Id = id == 0 ? entity.InstanceId : id;

            ECSNode.Entities.TryAdd(entityType, new List<Entity>());
            ECSNode.Entities[entityType].Add(entity);
            return entity;
        }
        
        private static void SetupEntity(Entity entity, Entity parent)
        {
            parent.SetChild(entity);
            entity.Awake();
            entity.Start();
        }

        private static void SetupEntity(Entity entity, Entity parent, object initData)
        {
            parent.SetChild(entity);
            entity.Awake(initData);
            entity.Start(initData);
        }

        public static void Destroy(Entity entity)
        {
            if (entity == null || entity.IsDisposed)
                return;
            
            try
            {
                entity.OnDestroy();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            entity.Dispose();
        }
    }
}
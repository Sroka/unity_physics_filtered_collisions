using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Arc.FilteredCollisions
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ExportPhysicsWorld))]
    public class FilteredEntityCollisionsSystem : SystemBase
    {
        private  BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();
        private  StepPhysicsWorld stepPhysicsWorld => World.GetOrCreateSystem<StepPhysicsWorld>();
        private  EndFramePhysicsSystem endFramePhysicsSystem => World.GetOrCreateSystem<EndFramePhysicsSystem>();
        internal NativeHashMap<EntityPair, CollisionEvent> Collisions;

        internal JobHandle EventsCollectedHandle;

        protected override void OnCreate()
        {
            Collisions = new NativeHashMap<EntityPair, CollisionEvent>(10, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            Collisions.Dispose();
        }

        protected override void OnUpdate()
        {
            var collisions = Collisions;
            Job.WithCode(() => { collisions.Clear(); })
               .Schedule();
            Dependency = new CollectCollisionsJob {Collisions = collisions,}
               .Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);
            endFramePhysicsSystem.AddInputDependency(Dependency);
            EventsCollectedHandle = Dependency;
        }

        private struct CollectCollisionsJob : ICollisionEventsJob
        {
            public NativeHashMap<EntityPair, CollisionEvent> Collisions;

            public void Execute(CollisionEvent collisionEvent)
            {
                Collisions[new EntityPair {EntityA = collisionEvent.EntityA, EntityB = collisionEvent.EntityB,}] =
                    collisionEvent;
            }
        }
    }
}
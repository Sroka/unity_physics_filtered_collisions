# Unity Physics Filtered Collisions
Simple system for filtering Unity Physics collisions so that only one collision is detected per body pair. Unity's implementation can raise multiple collision events per body pair according to:
https://forum.unity.com/threads/icollisioneventsjob-raises-events-many-times-at-once.840736/#post-6104946

This system raises only one collision event per body pair and discards the rest
Usage:
```
    [UpdateAfter(typeof(FilteredEntityCollisionsSystem))]
    public class SomeUserSystem : SystemBase
    {
        private FilteredEntityCollisionsSystem filteredEntityCollisionsSystem =>
            World.GetOrCreateSystem<FilteredEntityCollisionsSystem>();

        protected override void OnUpdate()
        {
            Dependency = new DetectCollisionsJob { }.Schedule(filteredEntityCollisionsSystem, Dependency);
        }

        [BurstCompile]
        private struct DetectCollisionsJob : IFilteredCollisionsJob
        {

            public void Execute(CollisionEvent collisionEvent)
            {
                // Do something
            }
        }
    }
```

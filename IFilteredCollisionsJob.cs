using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Physics;

namespace Arc.FilteredCollisions
{
    [JobProducerType(typeof(IFilteredCollisionEventJobExtensions.FilteredEntityCollisionsJobStruct<>))]
    public interface IFilteredCollisionsJob
    {
        void Execute(CollisionEvent collisionEvent);
    }

    public static class IFilteredCollisionEventJobExtensions
    {
        public struct FilteredCollisionsJobData<T>
        {
            public T UserJobData;

            [NativeDisableContainerSafetyRestriction]
            public NativeHashMap<EntityPair, CollisionEvent> CollisionEvents;
        }

        public struct FilteredEntityCollisionsJobStruct<T> where T : struct, IFilteredCollisionsJob
        {
            static IntPtr jobReflectionData;

            public static IntPtr Initialize()
            {
                if (jobReflectionData == IntPtr.Zero)
                {
                    jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(FilteredCollisionsJobData<T>),
                                                                            typeof(T), (ExecuteJobFunction) Execute);
                }

                return jobReflectionData;
            }

            // The Execute delegate and function have the same signature as IJob
            public delegate void ExecuteJobFunction(ref FilteredCollisionsJobData<T> jobData,
                                                    IntPtr additionalData,
                                                    IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            public static void Execute(ref FilteredCollisionsJobData<T> jobData, IntPtr additionalData,
                                       IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                var collisionEvents = jobData.CollisionEvents.GetValueArray(Allocator.Temp);
                for (var index = 0; index < collisionEvents.Length; index++)
                {
                    jobData.UserJobData.Execute(collisionEvents[index]);
                }

                collisionEvents.Dispose();
            }
        }

        public static unsafe JobHandle Schedule<T>(this T                         filteredEntityCollisionsJob,
                                                   FilteredEntityCollisionsSystem filteredEntityCollisionsSystem,
                                                   JobHandle                      inputDeps)
            where T : struct, IFilteredCollisionsJob
        {
            var data = new FilteredCollisionsJobData<T>
            {
                UserJobData     = filteredEntityCollisionsJob,
                CollisionEvents = filteredEntityCollisionsSystem.Collisions,
            };
            inputDeps = JobHandle.CombineDependencies(filteredEntityCollisionsSystem.EventsCollectedHandle, inputDeps);
            var parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref data),
                                                                   FilteredEntityCollisionsJobStruct<T>.Initialize(),
                                                                   inputDeps, ScheduleMode.Single);
            return JobsUtility.Schedule(ref parameters);
        }
    }
}
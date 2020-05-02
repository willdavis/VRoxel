using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Collections;
using Unity.Jobs;

using VRoxel.Navigation;

namespace NavigationJobSpecs
{
    public class ActivateAgentSpec
    {
        [Test]
        public void CanSetAllAgentStatus()
        {
            int count = 10;
            NativeArray<bool> agents = new NativeArray<bool>(count, Allocator.Persistent);
            
            ActivateAgents job = new ActivateAgents()
            {
                status = true,
                agents = agents
            };
            job.Schedule(count,1).Complete();

            for (int i = 0; i < count; i++)
                Assert.AreEqual(true, agents[i]);

            agents.Dispose();
        }

        [Test]
        public void CanSetASliceOfAgentStatus()
        {
            int count = 4;
            NativeArray<bool> agents = new NativeArray<bool>(count, Allocator.Persistent);
            NativeSlice<bool> slice = agents.Slice(1, 2);
            
            ActivateAgents job = new ActivateAgents()
            {
                status = true,
                agents = slice
            };
            job.Schedule(slice.Length,1).Complete();

            Assert.AreEqual(false, agents[0]);
            Assert.AreEqual(true,  agents[1]);
            Assert.AreEqual(true,  agents[2]);
            Assert.AreEqual(false, agents[3]);

            agents.Dispose();
        }
    }
}

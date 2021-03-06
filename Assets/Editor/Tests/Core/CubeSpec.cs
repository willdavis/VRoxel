﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.TestTools;

using VRoxel.Core;

namespace CoreSpecs
{
    public class CubeSpec
    {
        // Direction order is important for accessing Cube.Faces
        [Test]
        public void HasDirections()
        {
            Assert.AreEqual(0, (int)Cube.Direction.Up);
            Assert.AreEqual(1, (int)Cube.Direction.Down);
            Assert.AreEqual(2, (int)Cube.Direction.North);
            Assert.AreEqual(3, (int)Cube.Direction.East);
            Assert.AreEqual(4, (int)Cube.Direction.South);
            Assert.AreEqual(5, (int)Cube.Direction.West);
        }

        // unit vector order is important for accessing Cube.Faces
        [Test]
        public void HasUnitVectors()
        {
            Assert.AreEqual(new float3(-1, 1, 1), Cube.Vectors[0]);
            Assert.AreEqual(new float3( 1, 1, 1), Cube.Vectors[1]);
            Assert.AreEqual(new float3( 1, 1,-1), Cube.Vectors[2]);
            Assert.AreEqual(new float3(-1, 1,-1), Cube.Vectors[3]);
            Assert.AreEqual(new float3(-1,-1, 1), Cube.Vectors[4]);
            Assert.AreEqual(new float3( 1,-1, 1), Cube.Vectors[5]);
            Assert.AreEqual(new float3( 1,-1,-1), Cube.Vectors[6]);
            Assert.AreEqual(new float3(-1,-1,-1), Cube.Vectors[7]);
        }

        [Test]
        public void CanCalculateFace()
        {
            Vector3[] face = new Vector3[4];
            float3 position = new float3(0,0,0);
            float scale = 1.0f;

            Cube.Face(0, position, scale, ref face); // Top

            Assert.AreEqual(new Vector3(-1, 1, 1), face[0]);
            Assert.AreEqual(new Vector3( 1, 1, 1), face[1]);
            Assert.AreEqual(new Vector3( 1, 1,-1), face[2]);
            Assert.AreEqual(new Vector3(-1, 1,-1), face[3]);

            Cube.Face(1, position, scale, ref face); // Bottom

            Assert.AreEqual(new Vector3(-1,-1,-1), face[0]);
            Assert.AreEqual(new Vector3( 1,-1,-1), face[1]);
            Assert.AreEqual(new Vector3( 1,-1, 1), face[2]);
            Assert.AreEqual(new Vector3(-1,-1, 1), face[3]);
        }

        [Test]
        public void CanCalculateTransform()
        {
            float3[] cube = new float3[8];
            float3 position = new float3(0,0,0);
            Quaternion rotation = Quaternion.identity;
            float scale = 1.0f;

            Cube.Transform(position, scale, rotation, ref cube);

            Assert.AreEqual(new float3(-1, 1, 1), cube[0]);
            Assert.AreEqual(new float3( 1, 1, 1), cube[1]);
            Assert.AreEqual(new float3( 1, 1,-1), cube[2]);
            Assert.AreEqual(new float3(-1, 1,-1), cube[3]);
            Assert.AreEqual(new float3(-1,-1, 1), cube[4]);
            Assert.AreEqual(new float3( 1,-1, 1), cube[5]);
            Assert.AreEqual(new float3( 1,-1,-1), cube[6]);
            Assert.AreEqual(new float3(-1,-1,-1), cube[7]);
        }

        [Test]
        public void CanCalculateTransformRectangle()
        {
            float3[] rectangle = new float3[8];
            float3 start = new float3(-1,-1,-1);
            float3 end = new float3(1,1,1);
            float3 scale = new float3(1,1,1);
            Quaternion rotation = Quaternion.identity;

            Cube.TransformRectangle(start, end, scale, rotation, ref rectangle);

            Assert.AreEqual(new float3(-1, 1, 1), rectangle[0]);
            Assert.AreEqual(new float3( 1, 1, 1), rectangle[1]);
            Assert.AreEqual(new float3( 1, 1,-1), rectangle[2]);
            Assert.AreEqual(new float3(-1, 1,-1), rectangle[3]);
            Assert.AreEqual(new float3(-1,-1, 1), rectangle[4]);
            Assert.AreEqual(new float3( 1,-1, 1), rectangle[5]);
            Assert.AreEqual(new float3( 1,-1,-1), rectangle[6]);
            Assert.AreEqual(new float3(-1,-1,-1), rectangle[7]);
        }
    }
}

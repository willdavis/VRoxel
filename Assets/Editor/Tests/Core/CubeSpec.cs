using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
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
            Assert.AreEqual(0, (int)Cube.Direction.Top);
            Assert.AreEqual(1, (int)Cube.Direction.Bottom);
            Assert.AreEqual(2, (int)Cube.Direction.North);
            Assert.AreEqual(3, (int)Cube.Direction.East);
            Assert.AreEqual(4, (int)Cube.Direction.South);
            Assert.AreEqual(5, (int)Cube.Direction.West);
        }

        // unit vector order is important for accessing Cube.Faces
        [Test]
        public void HasUnitVectors()
        {
            Assert.AreEqual(new Vector3(-1, 1, 1), Cube.Vectors[0]);
            Assert.AreEqual(new Vector3( 1, 1, 1), Cube.Vectors[1]);
            Assert.AreEqual(new Vector3( 1, 1,-1), Cube.Vectors[2]);
            Assert.AreEqual(new Vector3(-1, 1,-1), Cube.Vectors[3]);
            Assert.AreEqual(new Vector3(-1,-1, 1), Cube.Vectors[4]);
            Assert.AreEqual(new Vector3( 1,-1, 1), Cube.Vectors[5]);
            Assert.AreEqual(new Vector3( 1,-1,-1), Cube.Vectors[6]);
            Assert.AreEqual(new Vector3(-1,-1,-1), Cube.Vectors[7]);
        }

        // unit vectors for a face must be returned in a clockwise order
        // this is important so that the mesh normals are oriented correctly
        [Test]
        public void HasFaces()
        {
            Assert.AreEqual(new int[4] { 0,1,2,3 }, Cube.Faces[(int)Cube.Direction.Top]);
            Assert.AreEqual(new int[4] { 7,6,5,4 }, Cube.Faces[(int)Cube.Direction.Bottom]);
            Assert.AreEqual(new int[4] { 1,0,4,5 }, Cube.Faces[(int)Cube.Direction.North]);
            Assert.AreEqual(new int[4] { 2,1,5,6 }, Cube.Faces[(int)Cube.Direction.East]);
            Assert.AreEqual(new int[4] { 3,2,6,7 }, Cube.Faces[(int)Cube.Direction.South]);
            Assert.AreEqual(new int[4] { 0,3,7,4 }, Cube.Faces[(int)Cube.Direction.West]);
        }

        [Test]
        public void CanCalculateFace()
        {
            Vector3[] face = new Vector3[4];
            Vector3 position = new Vector3(0,0,0);
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
            Vector3[] cube = new Vector3[8];
            Vector3 position = new Vector3(0,0,0);
            Quaternion rotation = Quaternion.identity;
            float scale = 1.0f;

            Cube.Transform(position, scale, rotation, ref cube);

            Assert.AreEqual(new Vector3(-1, 1, 1), cube[0]);
            Assert.AreEqual(new Vector3( 1, 1, 1), cube[1]);
            Assert.AreEqual(new Vector3( 1, 1,-1), cube[2]);
            Assert.AreEqual(new Vector3(-1, 1,-1), cube[3]);
            Assert.AreEqual(new Vector3(-1,-1, 1), cube[4]);
            Assert.AreEqual(new Vector3( 1,-1, 1), cube[5]);
            Assert.AreEqual(new Vector3( 1,-1,-1), cube[6]);
            Assert.AreEqual(new Vector3(-1,-1,-1), cube[7]);
        }

        [Test]
        public void CanCalculateTransformRectangle()
        {
            Vector3[] rectangle = new Vector3[8];
            Vector3 start = new Vector3(-1,-1,-1);
            Vector3 end = new Vector3(1,1,1);
            Vector3 scale = new Vector3(1,1,1);
            Quaternion rotation = Quaternion.identity;

            Cube.TransformRectangle(start, end, scale, rotation, ref rectangle);

            Assert.AreEqual(new Vector3(-1, 1, 1), rectangle[0]);
            Assert.AreEqual(new Vector3( 1, 1, 1), rectangle[1]);
            Assert.AreEqual(new Vector3( 1, 1,-1), rectangle[2]);
            Assert.AreEqual(new Vector3(-1, 1,-1), rectangle[3]);
            Assert.AreEqual(new Vector3(-1,-1, 1), rectangle[4]);
            Assert.AreEqual(new Vector3( 1,-1, 1), rectangle[5]);
            Assert.AreEqual(new Vector3( 1,-1,-1), rectangle[6]);
            Assert.AreEqual(new Vector3(-1,-1,-1), rectangle[7]);
        }
    }
}

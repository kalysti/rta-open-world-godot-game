using System;
using MeshDecimator;
using MeshDecimator.Math;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MeshDecimator
{
    public class MeshDecimatorTool
    {
        /**
         Start auto loding
        */
        public static ArrayMesh generateMesh(float quality, MeshInstance origMeshInst)
        {
            var arr_mesh = new ArrayMesh();
            var arrayMesh = origMeshInst.Mesh as ArrayMesh;
            quality = MathHelper.Clamp01(quality);

            for (int i = 0; i < origMeshInst.Mesh.GetSurfaceCount(); i++)
            {

                var arrays = generateMeshFromSurface(quality, origMeshInst, i);
                arr_mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
            }

            return arr_mesh;
        }

        public static Godot.Collections.Array generateMeshFromSurface(float quality, MeshInstance origMeshInst, int surface = 0)
        {
            var arr = origMeshInst.Mesh.SurfaceGetArrays(surface);

            var sourceNormals = (Godot.Vector3[])arr[(int)ArrayMesh.ArrayType.Normal];
            var sourceVertices = (Godot.Vector3[])arr[(int)ArrayMesh.ArrayType.Vertex];
            var sourceTxUv = (Godot.Vector2[])arr[(int)ArrayMesh.ArrayType.TexUv];
            var sourceTxUv2 = (Godot.Vector2[])arr[(int)ArrayMesh.ArrayType.TexUv2];
            var indexed_arr = (int[])arr[(int)ArrayMesh.ArrayType.Index];

            var sourceTangets = (float[])arr[(int)ArrayMesh.ArrayType.Tangent]; // chunck to 4
            var sourceBones = (int[])arr[(int)ArrayMesh.ArrayType.Bones]; //chunck to 4
            var sourceBonesWeights = (float[])arr[(int)ArrayMesh.ArrayType.Weights]; //chunck to 4

            var tangentList = new List<Vector4>();
            var weights = new List<Vector4>();
            var boneWeights = new List<BoneWeight>();

            var sourceMesh = new Mesh(sourceVertices.Select(d => new Vector3d(d.x, d.y, d.z)).ToArray(), indexed_arr);


            if (sourceTangets != null)
            {
                foreach (var c in sourceTangets.Split(4))
                {
                    var l = c.ToArray();
                    var vec = new Vector4(l[0], l[1], l[2], l[3]);

                    tangentList.Add(vec);
                }
            }

            if (sourceBonesWeights != null && sourceBones != null)
            {
                foreach (var c in sourceBonesWeights.Split(4))
                {
                    var l = c.ToArray();
                    var vec = new Vector4(l[0], l[1], l[2], l[3]);

                    weights.Add(vec);
                }

                int i = 0;
                foreach (var c in sourceBones.Split(4))
                {
                    var l = c.ToArray();
                    var bvc = weights[i];
                    var vec = new BoneWeight(l[0], l[1], l[2], l[3], bvc[0], bvc[1], bvc[2], bvc[3]);

                    boneWeights.Add(vec);
                    i++;
                }


                if (boneWeights.Count() > 0)
                    sourceMesh.BoneWeights = boneWeights.ToArray();
            }

            sourceMesh.Normals = sourceNormals.Select(d => new MeshDecimator.Math.Vector3(d.x, d.y, d.z)).ToArray();
            sourceMesh.Tangents = tangentList.ToArray();

            if (sourceTxUv != null)
                sourceMesh.UV1 = sourceTxUv.Select(d => new MeshDecimator.Math.Vector2(d.x, d.y)).ToArray();
            if (sourceTxUv2 != null)
                sourceMesh.UV2 = sourceTxUv2.Select(d => new MeshDecimator.Math.Vector2(d.x, d.y)).ToArray();


            int currentTriangleCount = 0;
            var subMeshIndicies = sourceMesh.GetSubMeshIndices();
            for (int i = 0; i < sourceMesh.GetSubMeshIndices().Length; i++)
            {
                currentTriangleCount += (subMeshIndicies[i].Length / 3);
            }

            //new triangle amount
            int targetTriangleCount = (int)Godot.Mathf.Ceil(currentTriangleCount * quality);

            GD.Print("Input: " + sourceVertices.Length + " vertices, " + currentTriangleCount + " triangles => " + targetTriangleCount);
            var algorithm = MeshDecimation.CreateAlgorithm(Algorithm.Default);
            var destMesh = MeshDecimation.DecimateMesh(algorithm, sourceMesh, targetTriangleCount);

            //create new mesh from arrays
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)ArrayMesh.ArrayType.Max);

            if (destMesh.Vertices != null)
            {
                arrays[(int)ArrayMesh.ArrayType.Vertex] = destMesh.Vertices.Select(tf => new Godot.Vector3((float)tf.x, (float)tf.y, (float)tf.z)).ToArray();
            }

            if (destMesh.Normals != null)
            {
                arrays[(int)ArrayMesh.ArrayType.Normal] = destMesh.Normals.Select(tf => new Godot.Vector3((float)tf.x, (float)tf.y, (float)tf.z)).ToArray();
            }

            if (destMesh.Indices != null)
            {
                arrays[(int)ArrayMesh.ArrayType.Index] = destMesh.Indices.ToArray();
            }

            if (destMesh.Tangents != null)
            {
                List<float> tangArr = new List<float>();
                foreach (var x in destMesh.Tangents)
                {
                    tangArr.Add(x.x);
                    tangArr.Add(x.y);
                    tangArr.Add(x.z);
                    tangArr.Add(x.w);
                }

                arrays[(int)ArrayMesh.ArrayType.Tangent] = tangArr.ToArray();
            }

            if (destMesh.BoneWeights != null)
            {
                List<float> newBoneWeights = new List<float>();
                List<int> newBoneIndexes = new List<int>();

                foreach (var x in destMesh.BoneWeights)
                {
                    newBoneIndexes.Add(x.boneIndex0);
                    newBoneIndexes.Add(x.boneIndex1);
                    newBoneIndexes.Add(x.boneIndex2);
                    newBoneIndexes.Add(x.boneIndex3);

                    newBoneWeights.Add(x.boneWeight0);
                    newBoneWeights.Add(x.boneWeight1);
                    newBoneWeights.Add(x.boneWeight2);
                    newBoneWeights.Add(x.boneWeight3);
                }

                arrays[(int)ArrayMesh.ArrayType.Bones] = newBoneIndexes.ToArray();
                arrays[(int)ArrayMesh.ArrayType.Weights] = newBoneWeights.ToArray();
            }

            if (destMesh.UV1 != null)
                arrays[(int)ArrayMesh.ArrayType.TexUv] = destMesh.UV1.Select(tf => new Godot.Vector2(tf.x, tf.y)).ToArray();
            if (destMesh.UV2 != null)
                arrays[(int)ArrayMesh.ArrayType.TexUv2] = destMesh.UV2.Select(tf => new Godot.Vector2(tf.x, tf.y)).ToArray();

            return arrays;
        }
    }
}
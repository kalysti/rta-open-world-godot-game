using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static MoreLinq.Extensions.DistinctByExtension;

namespace UMA
{
    public class UMAMeshGenerator
    {
        private float lowestOverlayZ = 0f;
        private float lastOverlayZ = 0f;
        private const float diff = 0.01f;
        private const float boneExtra = 0.01f;
        List<Vector3> vertices = new List<Vector3>();
        List<UMABone> bones = new List<UMABone>();

        private int surfaceTotal = 0;

        private int changes = 0;

        public ArrayMesh generate(ArrayMesh oldMesh, Skin skin, UMAReciepe reciepe, List<string> boneNames)
        {
            //reset changes value
            changes = 0;

            //clean up slots
            oldMesh = cleanOldSlots(oldMesh, reciepe);

            //clean up overlay
            oldMesh = cleanOldOverlays(oldMesh, reciepe);

            //create new slots
            oldMesh = createNewSlots(oldMesh, reciepe);

            //create new overlay
            oldMesh = createOverlays(oldMesh, reciepe);

            //assign binds
            if (changes > 0)
            {
                var binds = getBinds(reciepe);
                binds = binds.DistinctBy(dt => dt.getSelector()).ToList();
                skin.ClearBinds();

                int i = 0;
                foreach (var bind in binds)
                {
                    if (!boneNames.Contains(bind.boneName))
                        continue;

                    skin.AddBind(0, bind.transform);
                    skin.SetBindName(i, bind.boneName);
                    i++;
                }
            }

            //set main color
            return oldMesh;
        }


        /**
        get all binds 
        */
        private List<UMAReciepeBindPose> getBinds(UMAReciepe reciepe)
        {
            var res = new List<UMAReciepeBindPose>();

            foreach (var slot in reciepe.slots)
            {
                var slotPath = System.IO.Path.Combine(reciepe.slotPath, slot.Key + ".tscn");
                res.AddRange(extractBinds(slotPath));

                /*
                    foreach (var overlay in slot.Value)
                    {
                        var overlayPath = System.IO.Path.Combine(overlay.Value.overlayPath, overlay.Key + ".tscn");
                        res.AddRange(extractBinds(overlayPath));
                    }
                */
            }

            return res;
        }

        /* extract binds */
        private List<UMAReciepeBindPose> extractBinds(string urlPath)
        {
            var res = new List<UMAReciepeBindPose>();
            var file = ResourceLoader.Load<PackedScene>(urlPath);
            var instance = file.Instance();
            var meshes = new List<MeshInstance>();

            if (instance is Spatial)
            {
                foreach (var child in (instance as Spatial).GetChildren())
                {
                    if (child is MeshInstance)
                        meshes.Add(child as MeshInstance);
                }
            }

            if (instance is MeshInstance)
            {
                meshes.Add(instance as MeshInstance);
            }

            foreach (var mesh in meshes)
            {
                if (mesh.Skin == null)
                    continue;

                for (int bind = 0; bind < mesh.Skin.GetBindCount(); bind++)
                {
                    var bp = new UMAReciepeBindPose();
                    bp.boneName = mesh.Skin.GetBindName(bind);
                    bp.transform = mesh.Skin.GetBindPose(bind);

                    res.Add(bp);
                }

            }

            return res;
        }

        private ArrayMesh cleanOldOverlays(ArrayMesh oldMesh, UMAReciepe reciepe)
        {
            List<string> oldSurfaces = new List<string>();
            for (int surface = 0; surface < oldMesh.GetSurfaceCount(); surface++)
            {
                oldSurfaces.Add(oldMesh.SurfaceGetName(surface));
            }

            foreach (var oldSurface in oldSurfaces)
            {
                bool exist = false;

                if (!oldSurface.Contains("Overlay_"))
                    continue;

                foreach (var slot in reciepe.slots)
                {
                    var slotName = slot.Key;

                    foreach (var overlay in slot.Value)
                    {
                        var overlayName = slotName + "/Overlay_" + overlay.Key;

                        if (oldSurface.Contains(overlayName))
                            exist = true;
                    }
                }

                if (!exist)
                {
                    Console.WriteLine("[UMA] Delete overlay " + oldSurface);
                    oldMesh.SurfaceRemove(oldMesh.SurfaceFindByName(oldSurface));

                    changes++;
                }

            }

            return oldMesh;
        }
        /** 
        create slots 
        */
        private ArrayMesh createNewSlots(ArrayMesh oldMesh, UMAReciepe reciepe)
        {
            Dictionary<int, string> oldSurfaces = new Dictionary<int, string>();
            for (int surface = 0; surface < oldMesh.GetSurfaceCount(); surface++)
            {
                var name = oldMesh.SurfaceGetName(surface);
                oldSurfaces.Add(surface, name);
            }

            foreach (var slot in reciepe.slots)
            {
                if (!oldSurfaces.ContainsValue(slot.Key))
                {
                    var path = System.IO.Path.Combine(reciepe.slotPath, slot.Key + ".tscn");
                    //create slot
                    Console.WriteLine("[UMA] Create slot: " + path);
                    oldMesh = createSurface(oldMesh, path, slot.Key);
                }
            }

            return oldMesh;
        }
        /**
            create overlay
        */
        private ArrayMesh createOverlays(ArrayMesh oldMesh, UMAReciepe reciepe)
        {
            Dictionary<int, string> oldSurfaces = new Dictionary<int, string>();
            for (int surface = 0; surface < oldMesh.GetSurfaceCount(); surface++)
            {
                var name = oldMesh.SurfaceGetName(surface);
                oldSurfaces.Add(surface, name);
            }

            foreach (var slot in reciepe.slots)
            {
                var slotName = slot.Key;

                var path = System.IO.Path.Combine(reciepe.slotPath, slot.Key + ".tscn");
                var restBones = extractBinds(path);

                foreach (var overlay in slot.Value)
                {
                    var overlayName = slotName + "/Overlay_" + overlay.Key;
                    if (!oldSurfaces.ContainsValue(overlayName))
                    {
                        var overlayPath = System.IO.Path.Combine(overlay.Value.overlayPath, overlay.Key + ".tscn");
                        oldMesh = createSurfaceOverlay(oldMesh, slotName, overlayPath, overlayName, restBones);
                    }
                }
            }

            return oldMesh;
        }
        /**
            Create surface for overlay
        */
        public ArrayMesh createSurfaceOverlay(ArrayMesh mesh, string slotName, string urlPath, string overlayName, List<UMAReciepeBindPose> origBindPoses)
        {

            var file = ResourceLoader.Load<PackedScene>(urlPath);
            var instance = file.Instance();
            var meshes = new List<MeshInstance>();
            if (instance is Spatial)
            {
                foreach (var child in (instance as Spatial).GetChildren())
                {
                    if (child is MeshInstance)
                        meshes.Add(child as MeshInstance);
                }
            }

            if (instance is MeshInstance)
            {
                meshes.Add(instance as MeshInstance);
            }

            var mdt = new MeshDataTool();
            var idx = mesh.SurfaceFindByName(slotName);
            mdt.CreateFromSurface(mesh, idx);


            int totalSurfaces = 0;
            foreach (var mi in meshes)
            {
                var newMesh = (mi as MeshInstance).Mesh;
                var newSkin = (mi as MeshInstance).Skin;

                for (int surface = 0; surface < newMesh.GetSurfaceCount(); surface++)
                {

                    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, createSurfaceByBones(newMesh as ArrayMesh, surface, newSkin, origBindPoses));
                    var lastId = mesh.GetSurfaceCount() - 1;

                    var mat = newMesh.SurfaceGetMaterial(surface);
                    mat.ResourceLocalToScene = true;

                    if (mat is SpatialMaterial)
                    {
                        (mat as SpatialMaterial).ParamsGrow = true;
                        (mat as SpatialMaterial).ParamsGrowAmount = 0.005f;
                    }

                    mesh.SurfaceSetMaterial(lastId, mat);
                    mesh.SurfaceSetName(lastId, (totalSurfaces == 0) ? overlayName : overlayName + "/" + totalSurfaces);
                    totalSurfaces++;
                }

                changes++;
            }


            return mesh;
        }

        public class VectorTransform
        {

            public Vector3 vertex;
            public float[] boneWeights;
        }

        private Godot.Collections.Array createSurfaceByBones(ArrayMesh mesh, int surface, Skin newSkin, List<UMAReciepeBindPose> origBindPoses)
        {
            var mdt = new MeshDataTool();
            mdt.CreateFromSurface(mesh, surface);

            var st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);

            var newBindPoses = new List<UMAReciepeBindPose>();
            if (newSkin != null)
            {
                for (int i = 0; i < newSkin.GetBindCount(); i++)
                {
                    newBindPoses.Add(new UMAReciepeBindPose
                    {
                        boneName = newSkin.GetBindName(i),
                        transform = newSkin.GetBindPose(i),
                        boneIndex = newSkin.GetBindBone(i)
                    });
                }
            }

            var boneAmount = 0;
            for (int i = 0; i < mdt.GetVertexCount(); i++)
            {
                var oldVer = mdt.GetVertex(i);
                var oldNorm = mdt.GetVertexNormal(i);

                var newVer = new Vector3();
                var newNorm = new Vector3();

                var indexes = mdt.GetVertexBones(i);

                //  st.AddTangent(mdt.GetVertexTangent(i));
                st.AddBones(mdt.GetVertexBones(i));
                st.AddWeights(mdt.GetVertexWeights(i));

                int boneId = 0;
                foreach (var weight in mdt.GetVertexWeights(i))
                {
                    if (newBindPoses.Count >= indexes[boneId] && origBindPoses.Count >= indexes[boneId])
                    {
                        var restBoneNew = newBindPoses[indexes[boneId]];
                        var restBoneTemplate = origBindPoses[indexes[boneId]];

                        var dataup = restBoneNew.transform.Xform(Vector3.Up);
                        var dataright = restBoneNew.transform.Xform(Vector3.Right);

                        var templateup = restBoneTemplate.transform.Xform(Vector3.Up);
                        var templateright = restBoneTemplate.transform.Xform(Vector3.Right);

                        if (Mathf.Abs(dataup.AngleTo(templateup)) > 1 || Mathf.Abs(dataright.AngleTo(templateright)) > 1)
                        {
                            Transform convertMatrix = restBoneTemplate.transform.Inverse() * restBoneNew.transform;

                            newVer += convertMatrix.Xform(oldVer) * weight;
                            newNorm += convertMatrix.basis.Xform(oldNorm) * weight;
                        }
                        else
                        {
                            newVer += oldVer * weight;
                            newNorm += oldNorm * weight;
                        }


                    }
                    else
                    {
                        newVer += oldVer * weight;
                        newNorm += oldNorm * weight;
                    }

                    boneId++;
                }


                st.AddUv(mdt.GetVertexUv(i));

                if (mdt.GetVertexColor(i) != null)
                    st.AddColor(mdt.GetVertexColor(i));

                if (mdt.GetVertexUv2(i) != null)
                    st.AddUv2(mdt.GetVertexUv2(i));

                st.AddNormal(newNorm);
                st.AddVertex(newVer);

                boneAmount += mdt.GetVertexBones(i).Length;
            }

            //creating indexes
            for (int face = 0; face < mdt.GetFaceCount(); face++)
            {
                for (int faceI = 0; faceI < 3; faceI++)
                {
                    var ind = mdt.GetFaceVertex(face, faceI);
                    st.AddIndex(ind);
                }
            }

            st.GenerateTangents();
            return st.CommitToArrays();
        }

        /**
            Create surface overlay
        */
        public ArrayMesh createSurface(ArrayMesh mesh, string urlPath, string overlayName)
        {

            var file = ResourceLoader.Load<PackedScene>(urlPath);
            var instance = file.Instance();
            var meshes = new List<MeshInstance>();
            if (instance is Spatial)
            {
                foreach (var child in (instance as Spatial).GetChildren())
                {
                    if (child is MeshInstance)
                        meshes.Add(child as MeshInstance);
                }
            }

            if (instance is MeshInstance)
            {
                meshes.Add(instance as MeshInstance);
            }

            int totalSurfaces = 0;
            foreach (var mi in meshes)
            {
                var newMesh = (mi as MeshInstance).Mesh;
                for (int surface = 0; surface < newMesh.GetSurfaceCount(); surface++)
                {
                    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, newMesh.SurfaceGetArrays(surface));
                    var lastId = mesh.GetSurfaceCount() - 1;

                    var mat = newMesh.SurfaceGetMaterial(surface);
                    mat.ResourceLocalToScene = true;
                    mesh.SurfaceSetMaterial(lastId, mat);
                    mesh.SurfaceSetName(lastId, (totalSurfaces == 0) ? overlayName : overlayName + "/" + totalSurfaces);
                    totalSurfaces++;
                }

                changes++;
            }


            return mesh;
        }



        /**
            Clean up old surfacess
        **/
        private ArrayMesh cleanOldSlots(ArrayMesh oldMesh, UMAReciepe reciepe)
        {
            List<string> oldSurfaces = new List<string>();
            for (int surface = 0; surface < oldMesh.GetSurfaceCount(); surface++)
            {
                oldSurfaces.Add(oldMesh.SurfaceGetName(surface));
            }

            var allSlotNames = reciepe.slots.Select(tf => tf.Key).ToList();

            //clean up current surface by new slots
            foreach (var oldSurface in oldSurfaces)
            {
                bool exist = false;
                foreach (var reicpeSlotName in allSlotNames)
                {
                    if (oldSurface.Contains(reicpeSlotName))
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    Console.WriteLine("[UMA] Delete surface " + oldSurface);
                    oldMesh.SurfaceRemove(oldMesh.SurfaceFindByName(oldSurface));

                    changes++;
                }
            }

            return oldMesh;
        }


        private int translateKey(string key)
        {
            if (String.IsNullOrEmpty(key))
                return -1;

            if (key.ToLower().Contains("torso"))
            {
                return 0;
            }
            else if (key.ToLower().Contains("legs"))
            {
                return 1;
            }
            else
                return -1;
        }




    }
}
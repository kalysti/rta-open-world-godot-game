using Godot;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UMA;

[Tool]
public class UMASkeleton : UMASkeletonBase
{
    public bool activeRenderQueue = true;

    public System.Threading.Thread renderThread = null;

    public override void _ExitTree()
    {
        disconnectEvents();

        activeRenderQueue = false;

        if (meshin != null)
        {

            meshin.Mesh = null;
            meshin.Skin = null;
        }

    }

    public override void _EnterTree()
    {
        setSkinGroups();
        scanSlotFolder();
        loadDefaultBones();
        connectEvents();

        renderThread = new System.Threading.Thread(new System.Threading.ThreadStart(rendering));
        renderThread.Start();
    }

    public override void _Ready()
    {
        //assign primary mesh
        meshin = GetNode<MeshInstance>("MeshInstance");

        //Set up mesh and skin
        meshin.Mesh = new ArrayMesh();
        meshin.Skin = new Skin();


        meshin.Skin.ResourceLocalToScene = true;
        meshin.Mesh.ResourceLocalToScene = true;

        //load default reciepe
        if (Engine.EditorHint)
        {
            var reciepe = getDefaultReciepe();
            if (reciepe != null)
                loadReciepeToEdtior(reciepe);

            editorUpdate();

            //load bone poses
            generateDNA(_dna);
        }
    }


    #region rendering

    public void loadReciepe(UMAReciepe reciepe)
    {
        renderQueue.Enqueue(reciepe);
    }

    protected void rendering()
    {
        while (activeRenderQueue)
        {
            if (renderQueue.Count > 0)
            {
                var reciepe = renderQueue.Dequeue();
                if (reciepe != null)
                {
                    renderMesh(reciepe);
                }
            }

            //to domesthing with mesh
            System.Threading.Thread.Sleep(50);
        }
    }

    protected void renderMesh(UMAReciepe reciepe)
    {
        try
        {
            var timeStart = OS.GetUnixTime();
            var generator = new UMAMeshGenerator();
            reciepe.slotPath = _slotDir;
            foreach (var slot in reciepe.slots)
            {
                var origSlot = _slotList[slot.Key];

                foreach (var overlay in slot.Value)
                {
                    overlay.Value.overlayPath = origSlot.overlayDir;
                }
            }

            List<string> boneNames = new List<string>();
            for (int bone = 0; bone < GetBoneCount(); bone++)
            {
                boneNames.Add(GetBoneName(bone));
            }

            var newMesh = generator.generate((ArrayMesh)meshin.Mesh, meshin.Skin, reciepe, boneNames);
            newMesh = changeSkinColorSurface(newMesh, reciepe);

            CallDeferred("assignMesh", timeStart);

            //set reciepe to last reciepe
            _lastReciepe = reciepe;

        }
        catch (Exception e)
        {
            GD.PrintErr("[UMA] Fails: " + e.Message + ", " + e.StackTrace);
        }
    }


    protected void assignMesh(ulong timeStart)
    {
        var timeEnd = OS.GetUnixTime() - timeStart;

        meshin.ExtraCullMargin = 1f;
        GD.Print("[UMA] Generate with " + timeEnd + " seconds");
        meshin.ExtraCullMargin = 0f;
    }
    #endregion

}

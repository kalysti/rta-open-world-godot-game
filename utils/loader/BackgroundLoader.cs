using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public class BackgroundLoader : Node
{
    protected Queue<BackgroundLoaderBaseItem> loadingQueue = new Queue<BackgroundLoaderBaseItem>();

    protected BackgroundLoaderBaseItem currentLoader = null;

    protected System.Threading.Thread loadingThread;

    protected bool loadingThreadRunning = false;

    [Export]
    public int timeMax = 30;

    public override void _Ready()
    {
        //  loader = new ResourceLoader();
        loadingThreadRunning = true;
        loadingThread = new System.Threading.Thread(() => LoaderProcess());
        loadingThread.Start();
    }

    public override void _ExitTree()
    {
        loadingThreadRunning = false;
        currentLoader = null;
        loadingThread?.Abort();
    }

    public void Load(BackgroundLoaderBaseItem item)
    {
        //check if its already exist
        if (ResourceLoader.HasCached(item.path))
        {
            item.resource = ResourceLoader.Load(item.path);
            item.CallOnLoaderComplete();
        }
        else
        {
            loadingQueue.Enqueue(item);
        }
    }

    protected void LoaderProcess()
    {
        while (loadingThreadRunning)
        {
            try
            {
                if (currentLoader != null)
                {
                    var t = OS.GetTicksMsec();

                    while (OS.GetTicksMsec() < t + timeMax)
                    {
                        var p = currentLoader.loader.Poll();

                        if (p == Error.FileEof)
                        {
                            var loadedResource = currentLoader.loader.GetResource(); ;

                            currentLoader.resource = loadedResource;
                            currentLoader.CallOnLoaderComplete();

                            //remove same elements form queue, and call for same elements, loading finish
                            if (loadingQueue.Count > 0)
                            {
                                foreach (var q in loadingQueue.Where(tf => tf.path == currentLoader.path))
                                {
                                    q.resource = loadedResource;
                                    q.CallOnLoaderComplete();
                                }

                                loadingQueue = new Queue<BackgroundLoaderBaseItem>(loadingQueue.Where(tf => tf.path != currentLoader.path));
                            }

                            currentLoader = null;

                            break;
                        }
                        else if (p == Error.Ok)
                        {
                            var progress = ((float)currentLoader.loader.GetStage()) / currentLoader.loader.GetStageCount();
                            currentLoader.CallOnLoaderUpdate(progress);

                            //call for all others the update
                            if (loadingQueue.Count > 0)
                            {
                                foreach (var q in loadingQueue.Where(tf => tf.path == currentLoader.path))
                                {
                                    q.CallOnLoaderUpdate(progress);
                                }
                            }
                        }
                        else
                        {
                            GD.Print("[Loader] Error ");
                            currentLoader.CallOnLoaderError();

                            //remove the same elements from queue and set error event
                            if (loadingQueue.Count > 0)
                            {
                                foreach (var q in loadingQueue.Where(tf => tf.path == currentLoader.path))
                                {
                                    q.CallOnLoaderError();
                                }

                                loadingQueue = new Queue<BackgroundLoaderBaseItem>(loadingQueue.Where(tf => tf.path != currentLoader.path));
                            }

                            currentLoader = null;


                            break;
                        }
                    }

                    /* check of all other elements in queue and set to finish */

                }
                else if (loadingQueue.Count > 0)
                {
                    var element = loadingQueue.Dequeue();

                    try
                    {
                        currentLoader = element;
                        currentLoader.loader = ResourceLoader.LoadInteractive(currentLoader.path);
                    }
                    catch
                    {
                        currentLoader = null;
                    }
                }

            }
            catch (Exception e)
            {
                GD.PrintErr("[BackgroundLoader] " + e.Message);
            }
            
            System.Threading.Thread.Sleep(50);
        }
    }
}

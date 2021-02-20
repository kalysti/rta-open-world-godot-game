using Godot;
using System;
using System.Collections.Generic;

namespace UMA.Helper
{
    public static class ScanFolderUtility
    {
        public static void scanDir<T>(string path, string filter, ref Godot.Collections.Dictionary<string, T> dic, object defaultObject)
        {
            if (defaultObject == null)
                defaultObject = (bool)false;

            List<string> names = new List<string>();
            if (!String.IsNullOrEmpty(path))
            {
                var dir = new Godot.Directory();
                dir.Open(path);
                dir.ListDirBegin();
                while (true)
                {
                    var file = dir.GetNext();

                    if (String.IsNullOrEmpty(file))
                        break;

                    else if (!file.BeginsWith(".") && file.Contains("." + filter))
                    {
                        var fileName = file.Replace("." + filter, "");
                        if (!names.Contains(fileName))
                        {
                            names.Add(fileName);
                        }
                    }
                }

                foreach (var x in names)
                {
                    if (!dic.ContainsKey(x))
                    {
                        if (defaultObject is Resource)
                        {
                            var df = (defaultObject as Resource);
                            df.ResourceLocalToScene = true;
                            var dup = (object)(df.Duplicate());
                            dic.Add(x, (T)dup);
                        }
                        else
                            dic.Add(x, (T)defaultObject);
                    }
                }

                foreach (var t in dic)
                {
                    if (!names.Contains(t.Key))
                    {
                        dic.Remove(t.Key);
                    }
                    else
                    {
                        if (t.Value is Resource)
                        {
                            (t.Value as Resource).ResourceLocalToScene = true;
                        }
                    }
                }
            }
            else
            {
                dic.Clear();
            }
        }
    }

}
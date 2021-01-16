using ZeroFormatter;
using System.Text;
using Godot;
using ByteSizeLib;

namespace Game.Networking
{
    public class NetworkStats
    {
        private int tempPackages = 0;
        private int tempPackagesIn = 0;
        public int packagesPerSecond = 0;
        private float tempTraffic = 0;
        private float tempTrafficIn = 0;
        public float NetOut = 0;
        public float NetIn = 0;
        private ulong lastTime = 0;
        public uint lastPackageAt = 0;

        public uint pingMs = 0;

        public void loop()
        {
            if ((OS.GetSystemTimeSecs() - lastTime) >= 1)
            {
                packagesPerSecond = tempPackages;
                NetOut = tempTraffic;
                NetIn = tempTrafficIn;
                tempPackages = 0;
                tempPackagesIn = 0;
                tempTrafficIn = 0;
                tempTraffic = 0;
                lastTime = OS.GetSystemTimeSecs();
            }
        }

        public void AddPackage(string message)
        {
            tempTraffic += System.Text.ASCIIEncoding.Unicode.GetByteCount(message);
            tempPackages++;
        }
        public void AddInPackage(string message)
        {
            tempTrafficIn += System.Text.ASCIIEncoding.Unicode.GetByteCount(message);
            tempPackagesIn++;
        }

        public string getNetOut()
        {
            return ByteSize.FromBytes(NetOut).ToString()  + "/s" ;
        }
        public string getNetIn()
        {
            return ByteSize.FromBytes(NetIn).ToString()  + "/s" ;
        }

    }
}


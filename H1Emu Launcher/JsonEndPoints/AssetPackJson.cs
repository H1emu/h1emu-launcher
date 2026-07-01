using System.Collections.Generic;

namespace H1Emu_Launcher.JsonEndPoints
{
    public class AssetPackJson
    {
        public class Asset
        {
            public string version { get; set; }
            public string filename { get; set; }
            public string url { get; set; }
            public string hash { get; set; }
        }

        public class Root
        {
            public List<Asset> assets { get; set; }
        }
    }
}

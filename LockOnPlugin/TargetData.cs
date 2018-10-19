﻿using System.Collections.Generic;

namespace LockOnPluginKK
{
    public class TargetData
    {
        public List<string> normalTargets;
        public List<string> femaleTargets;
        public List<string> maleTargets;
        public List<CustomTarget> customTargets;
        public List<CenterWeigth> centerWeigths;

        public class CustomTarget
        {
            public string target;
            public string point1;
            public string point2;
            public float midpoint;
        }

        public class CenterWeigth
        {
            public string bone;
            public float weigth;
        }
    }
}

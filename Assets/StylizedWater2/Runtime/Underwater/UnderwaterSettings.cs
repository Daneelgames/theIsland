using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWater2
{
#if URP
    [Serializable, VolumeComponentMenu("Stylized Water2/Underwater")]
    public class UnderwaterSettings : VolumeComponent
    {
        public FloatParameter verticalDensity = new FloatParameter(100f);
        [Min(0f)]
        public FloatParameter verticalDepth = new FloatParameter(5f);
        public FloatParameter horizontalDensity = new FloatParameter(8f);
        public FloatParameter startDistance = new FloatParameter(1f);
        public FloatParameter fogBrightness = new FloatParameter(1f);
        public FloatParameter subsurfaceStrength = new FloatParameter(1f);
    }
#else
    public class UnderwaterSettings : ScriptableObject { }
#endif
}
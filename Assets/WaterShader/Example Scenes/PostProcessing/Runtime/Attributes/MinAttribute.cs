using UnityEngine;

namespace GTASP.WaterShader.Example_Scenes.PostProcessing.Runtime.Attributes
{
    public sealed class MinAttribute : PropertyAttribute
    {
        public readonly float min;

        public MinAttribute(float min)
        {
            this.min = min;
        }
    }
}

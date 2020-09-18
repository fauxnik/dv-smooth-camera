using UnityModManagerNet;

namespace SmoothCamera
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable verbose logging")]
        public bool isLoggingEnabled =
#if DEBUG
        true;
#else
        false;
#endif
        [Draw(Label = "Field Of View", Min = 30, Max = 360)]
        public float fieldOfView = 90;
        [Draw(Label = "Smoothing Time - Position (values other than 0 may have undesirable results)", Min = 0)]
        public float smoothTimePosition = 0f;
        [Draw(Label = "Smoothing Time - Rotation", Min = 0)]
        public float smoothTimeRotation = 0.7f;

        override public void Save(UnityModManager.ModEntry entry)
        {
            Save<Settings>(this, entry);
        }

        public void OnChange() { }
    }
}

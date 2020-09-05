﻿using UnityModManagerNet;

namespace SmoothCamera
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable logging")]
        public bool isLoggingEnabled =
#if DEBUG
        true;
#else
        false;
#endif
        [Draw(Label = "Field Of View", Min = 30, Max = 360)]
        public float fieldOfView = 75;
        [Draw(Label = "Smoothing Time (Position)", Min = 0)]
        public float smoothTimePosition = 0f;
        [Draw(Label = "Smoothing Time (Rotation)", Min = 0)]
        public float smoothTimeRotation = 0.5f;

        override public void Save(UnityModManager.ModEntry entry)
        {
            Save<Settings>(this, entry);
        }

        public void OnChange() { }
    }
}
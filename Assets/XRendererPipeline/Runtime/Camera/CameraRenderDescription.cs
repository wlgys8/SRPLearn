using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{
    public struct CameraRenderDescription
    {
        public bool requireTempRT;
        public int pixelWidth;
        public int pixelHeight;
        public int msaaLevel;

        public Camera camera;

        public CameraRenderDescription(Camera camera){
            this.camera = camera;
            this.msaaLevel = 1;
            this.pixelWidth = camera.pixelWidth;
            this.pixelHeight = camera.pixelHeight;
            this.requireTempRT = false;
        }

    }
}

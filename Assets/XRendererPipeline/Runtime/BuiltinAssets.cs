using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{

    [CreateAssetMenu(menuName = "SRPLearn/BuiltinAssets")]
    public class BuiltinAssets:ScriptableObject
    {
        [SerializeField]
        private Texture2D _BRDFLUT;

        public Texture2D BRDFLUT{
            get{
                return _BRDFLUT;
            }
        }
    }
}

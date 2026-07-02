using UnityEngine;

namespace ImmediateShapes {
    public class Scriptable_UI_Renderer : MonoBehaviour
    {
        public ScriptableGUI GUI;


        void Start()
        {
            ScriptableGUIRenderer.UIMaterialGroup MatGroup = new ScriptableGUIRenderer.UIMaterialGroup();
            

            ScriptableGUIRenderer.init();
            Debug.Log("Registering");
            ScriptableGUIRenderer.RegisterScriptableGUI(MatGroup, GUI);

        }
    }
}



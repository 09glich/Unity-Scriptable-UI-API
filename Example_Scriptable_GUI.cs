using UnityEngine;



namespace ImmediateShapes
{
    [CreateAssetMenu(fileName = "ScriptableGUI", menuName = "ImGUI/ScriptableGUIExample")]
    public class Example_Scriptable_GUI : ScriptableGUI
    {
        public override void Render()
        {

            ScriptableGUIRenderer.DrawRect(Vector2.one * 20, Vector2.one * 200);
        }
    }

}



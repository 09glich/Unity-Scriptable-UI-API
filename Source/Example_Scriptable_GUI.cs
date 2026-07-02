using UnityEngine;



namespace ImmediateShapes
{
    [CreateAssetMenu(fileName = "ScriptableGUI", menuName = "ImGUI/ScriptableGUIExample")]
    public class Example_Scriptable_GUI : ScriptableGUI
    {
        public override void Render()
        {
            ScriptableGUIRenderer.ChangeColor(new Color(1, 1, 1, Mathf.Abs(Mathf.Sin(Time.time))));
            ScriptableGUIRenderer.DrawRect(Vector2.one * 20, Vector2.one * 200);

            ScriptableGUIRenderer.ChangeColor(new Color(.25f, .25f, .25f, 1f));
            ScriptableGUIRenderer.DrawRect((int)((Mathf.Sin(Time.time)*100) + 400 ), (int)(Mathf.Cos(Time.time) * 100), 100, 100);
        }
    }

}



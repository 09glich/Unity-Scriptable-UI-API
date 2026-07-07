using System;
using UnityEngine;



namespace ImmediateShapes
{
    [CreateAssetMenu(fileName = "ScriptableGUI", menuName = "ImGUI/ScriptableGUIExample")]
    public class Example_Scriptable_GUI : ScriptableGUI
    {
        public override void Render()
        {
            ScriptableGUIRenderer.ResetMatrix();

            ScriptableGUIRenderer.ChangeColor(new Color(1, 1, 1, Mathf.Abs(Mathf.Sin(Time.time))));
            ScriptableGUIRenderer.DrawRect(Vector2.one * 20, Vector2.one * 125);

            ScriptableGUIRenderer.ChangeColor(new Color(.25f, .25f, .25f, 1f));
            ScriptableGUIRenderer.DrawRect((int)((Mathf.Sin(Time.time)*100) + 400 ), (int)(Mathf.Cos(Time.time) * 100), 100, 100);

            ScriptableGUIRenderer.ChangeColor(new Color(1, 1, 1, Mathf.Abs(Mathf.Sin(Time.time))));
            ScriptableGUIRenderer.DrawRect(new Vector2(20, 100), Vector2.one * 125);

            ScriptableGUIRenderer.ChangeColor(Color.grey);
            ScriptableGUIRenderer.DrawRect(new Vector2((MathF.Sin(Time.time) * 20) + 20, 200), Vector2.one*100);

            ScriptableGUIRenderer.ChangeColor(Color.darkGray);
            ScriptableGUIRenderer.DrawRect(new Vector2(10, 190), Vector2.one*120f);

            ScriptableGUIRenderer.ChangeColor(Color.black);
            ScriptableGUIRenderer.DrawRect(new Vector2((MathF.Sin(Time.time + Mathf.PI) * 20) + 20, 200), Vector2.one * 100);



            ScriptableGUIRenderer.DrawTriangle(
                new Vector2(400 + (Mathf.Sin(Time.time) * 50), 400 + (Mathf.Cos(Time.time) * 50)),
                new Vector2(500 + (Mathf.Sin(Time.time) * 50), 400 + (Mathf.Cos(Time.time) * 50)),
                new Vector2(500, 500)
            );

            Color CircleColor = Color.purple;
            CircleColor.a = MathF.Sin(Time.time);

            ScriptableGUIRenderer.ChangeColor(CircleColor);
            ScriptableGUIRenderer.DrawCircle(Vector2.one * 500, (int)(Mathf.Sin(Time.time*MathF.PI)*100));

            ScriptableGUIRenderer.PushMatrix();

            ScriptableGUIRenderer.Translate(new Vector2(800, 300));

            ScriptableGUIRenderer.DrawLine(
                new Vector2(Mathf.Sin(Time.time), Mathf.Cos(Time.time)) * 50 ,
                new Vector2(Mathf.Sin(Time.time + Mathf.PI), Mathf.Cos(Time.time + Mathf.PI)) * 50,
                Mathf.Sin(Time.time) * 10
            );

            ScriptableGUIRenderer.PopMatrix();

            ScriptableGUIRenderer.Translate(new Vector2(700, 200));
            ScriptableGUIRenderer.Scale(new Vector2(200, 500));
            ScriptableGUIRenderer.RotateRaw(Time.time * 60);


            ScriptableGUIRenderer.DrawTriangle(
                new Vector2(Mathf.Sin(0), Mathf.Cos(0)),
                new Vector2(Mathf.Sin(Mathf.PI * 2f / 3f), Mathf.Cos(Mathf.PI * 2f / 3f)),
                new Vector2(Mathf.Sin(Mathf.PI * 4f / 3f), Mathf.Cos(Mathf.PI * 4f / 3f))
            );




        }
    }

}



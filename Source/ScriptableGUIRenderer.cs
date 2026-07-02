using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace ImmediateShapes {
    public class ScriptableGUIRenderer
    {

        private static bool Running = false;
        public enum UIMeshMode { 
            Opaque,
            Transparent,
            Circle,
            CircleTransparent,
            Line,
            Text
        }

        static Dictionary<UIMeshMode, UIMeshElement> UIMeshes;
        static Dictionary<string, ScriptableUIRegistry> ScriptableGUIElements;

        public struct ScriptableUIRegistry{
            public ScriptableGUI GUIElement;
            public UIMaterialGroup MaterialGroup;
        }

        public class UIMaterialGroup {
            private Dictionary<UIMeshMode, Material> Materials;

            public Material GetMaterialFromMode(UIMeshMode MaterialMode) {
                Materials.TryGetValue(MaterialMode, out Material material);
                return material;
            }

            public UIMaterialGroup() {
                Materials = new Dictionary<UIMeshMode, Material>();

                Shader Opaque = Shader.Find("IMGUIMainFrame/URPUnlitOpaque");
                Shader Transparent = Shader.Find("IMGUIMainFrame/URPUnlitTransparent");

                Debug.Log(Opaque);
                Debug.Log(Transparent);

                Materials.Add(UIMeshMode.Opaque, new Material(Opaque));
                Materials.Add(UIMeshMode.Transparent, new Material(Transparent));

            }
        }

        private class UIMeshElement {
            public Mesh CurrentMesh;

            public List<Vector3> Verticies;
            public List<int> Indicys;
            public List<Vector2> UVs;
            public List<Color> Colors;

            public bool Modifyed = false;

            public UIMeshElement() {
                CurrentMesh = new Mesh();
                CurrentMesh.MarkDynamic();

                Verticies = new List<Vector3>();
                Indicys = new List<int>();
                UVs = new List<Vector2>();
                Colors = new List<Color>();
            }

            public void RenderToScreen(RasterCommandBuffer cmdbuffer, Material Mat) {
                CurrentMesh.Clear();
                CurrentMesh.SetVertices(Verticies);
                CurrentMesh.SetUVs(0, UVs);
                CurrentMesh.SetColors(Colors);
                CurrentMesh.SetIndices(Indicys.ToArray(), MeshTopology.Triangles, 0);
                CurrentMesh.RecalculateNormals();

                CurrentMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 500000f);

                Debug.Log("Rendering Mesh");

                Mat.SetInt("_ScreenWidth", Screen.width);
                Mat.SetInt("_ScreenHeight", Screen.height);

                if (Modifyed) {
                    cmdbuffer.DrawMesh(CurrentMesh, Matrix4x4.identity, Mat);
                }

                Modifyed = false;

                ZIndex = 0;
                Verticies.Clear();
                UVs.Clear();
                Colors.Clear();
                Indicys.Clear();
            }
        }

        static int ZIndex = 0;
        private static void AddQuad(UIMeshElement UIMesh,Vector2Int Position, Vector2Int Size, Color VertexColor)
        {
            UIMesh.Modifyed = true;

            Vector2 TruePos = new Vector2(Position.x, Position.y);


            Vector3 Position1 = new Vector3(TruePos.x, TruePos.y, ZIndex);
            Vector3 Position2 = new Vector3(TruePos.x + Size.x, TruePos.y, ZIndex);
            Vector3 Position3 = new Vector3(TruePos.x, TruePos.y + Size.y, ZIndex);
            Vector3 Position4 = new Vector3(TruePos.x + Size.x, TruePos.y + Size.y, ZIndex);

            int QuadOffset = UIMesh.Verticies.Count;

            //Positions
            UIMesh.Verticies.Add(Position1);
            UIMesh.Verticies.Add(Position2);
            UIMesh.Verticies.Add(Position3);
            UIMesh.Verticies.Add(Position4);

            //Tri 1
            UIMesh.Indicys.Add(QuadOffset + 0);
            UIMesh.Indicys.Add(QuadOffset + 1);
            UIMesh.Indicys.Add(QuadOffset + 2);
            
            //Tri 2
            UIMesh.Indicys.Add(QuadOffset + 2);
            UIMesh.Indicys.Add(QuadOffset + 3);
            UIMesh.Indicys.Add(QuadOffset + 1);
            
            //Uvs
            UIMesh.UVs.Add(new Vector2(0, 0));
            UIMesh.UVs.Add(new Vector2(1, 0));
            UIMesh.UVs.Add(new Vector2(0, 1));
            UIMesh.UVs.Add(new Vector2(1, 1));

            //Colors
            UIMesh.Colors.Add(VertexColor);
            UIMesh.Colors.Add(VertexColor);
            UIMesh.Colors.Add(VertexColor);
            UIMesh.Colors.Add(VertexColor);

            ZIndex++;
        }

        private static Color CurrentColor = Color.white;

        //Change Active Color
        public static void ChangeColor(Color Color)
        {
            CurrentColor = Color;
        }

        //Render Rectangle
        public static void DrawRect(int X, int Y, int Width, int Height)
        {
            if (CurrentColor.a < 1)
            {
                AddQuad(UIMeshes[UIMeshMode.Transparent], new Vector2Int(X, Y), new Vector2Int(Width, Height), CurrentColor);
            }
            else
            {
                AddQuad(UIMeshes[UIMeshMode.Opaque], new Vector2Int(X, Y), new Vector2Int(Width, Height), CurrentColor);
            }

        }
        public static void DrawRect(Vector2 Position, Vector2 Size) { DrawRect((int)Position.x, (int)Position.y, (int)Size.x, (int)Size.y); }

        static List<ScriptableGUI> ScriptableScreenGUIs;
        static List<ScriptableGUI> ScriptableWorldGUIs;


        // Register a full screen GUI renderer to be rendered every frame
        public static void RegisterScriptableGUI(UIMaterialGroup MatGroup, ScriptableGUI GUIRenderer, string RendererName) {
            ScriptableUIRegistry ScriptableGUIElement = new ScriptableUIRegistry();
            ScriptableGUIElement.MaterialGroup = MatGroup;
            ScriptableGUIElement.GUIElement = GUIRenderer;

            ScriptableGUIElements.Add(RendererName, ScriptableGUIElement);
        }

        static int UnnamedGUIs = 0;
        public static void RegisterScriptableGUI(UIMaterialGroup MatGroup, ScriptableGUI GUIRenderer) {
            RegisterScriptableGUI(MatGroup, GUIRenderer, $"Unnamed {UnnamedGUIs}");
            UnnamedGUIs++;
        }

        //Initialize everything such as Render meshes and Materials
        public static void init()
        {
            Running = true;
            if (ScriptableGUIElements != null) { return; }

            ScriptableGUIElements = new Dictionary<string, ScriptableUIRegistry>();
            UIMeshes = new Dictionary<UIMeshMode, UIMeshElement>();

            UIMeshes.Add(UIMeshMode.Opaque, new UIMeshElement());
            UIMeshes.Add(UIMeshMode.Transparent, new UIMeshElement());

            ScriptableScreenGUIs = new List<ScriptableGUI>();
            ScriptableWorldGUIs = new List<ScriptableGUI>();
        }

        //Called by the imidiate GUI render Feature so it renders above post prosessing
        public static void RenderToScreen(RasterCommandBuffer CMDBuffer)
        {
            if (!Running) { return; }
            foreach (KeyValuePair<string, ScriptableUIRegistry> item in ScriptableGUIElements)
            {
                item.Value.GUIElement.Render();

                foreach (KeyValuePair<UIMeshMode, UIMeshElement> UIMesh in UIMeshes)
                {
                    UIMesh.Value.RenderToScreen(CMDBuffer, item.Value.MaterialGroup.GetMaterialFromMode(UIMesh.Key));
                }
            }

            
        }
    }
}



using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace ImmediateShapes {
    public class ScriptableGUIRenderer
    {

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
                Shader OpaqueCircle = Shader.Find("IMGUIMainFrame/URPUnlitCircleSDFOpaque");
                Shader TransparentCircle = Shader.Find("IMGUIMainFrame/URPUnlitCircleSDFTransparent");

                Debug.Log(TransparentCircle);

                Materials.Add(UIMeshMode.Opaque, new Material(Opaque));
                Materials.Add(UIMeshMode.Transparent, new Material(Transparent));
                Materials.Add(UIMeshMode.Circle, new Material(OpaqueCircle));
                Materials.Add(UIMeshMode.CircleTransparent, new Material(TransparentCircle));

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

        static Stack<Matrix4x4> TransformHistory = new();
        static Matrix4x4 TransformMatrix = Matrix4x4.identity;

        //Matrix Transformations
        public static void SetMatrix(Matrix4x4 NewMatrix)
        {
            TransformMatrix = NewMatrix;
        }
        public static void PushMatrix() { TransformHistory.Push(TransformMatrix); }
        public static void PopMatrix() { TransformMatrix = TransformHistory.Pop(); }
        public static void ResetMatrix() { TransformMatrix = Matrix4x4.identity; }

        // Rotation
        public static void RotateRaw(float Direction) { TransformMatrix *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, Direction))); }
        public static void RotateRaw(Quaternion DirectQuaternion) { TransformMatrix *= Matrix4x4.Rotate(DirectQuaternion); }

        //Position
        public static void Translate(Vector2 Position) { TransformMatrix *= Matrix4x4.Translate(new Vector3(Position.x, Position.y,0)); }
        public static void Translate(int X, int Y) { Translate(new Vector2(X, Y)); }

        //Scale

        public static void Scale(Vector2 Scale) { TransformMatrix *= Matrix4x4.Scale(new Vector3(Scale.x, Scale.y, 1)); }
        public static void Scale(int X, int Y) { Scale(new Vector2(X, Y)); }



        static int ZIndex = 0;
        static float ZIndexDamp = .001f;
        
        private static void AddQuad(UIMeshElement UIMesh,Vector2Int Position, Vector2Int Size, Color VertexColor)
        {
            UIMesh.Modifyed = true;

            Vector2 TruePos = new Vector2(Position.x, Position.y);


            Vector3 Position1 = TransformMatrix.MultiplyPoint3x4(new Vector3(TruePos.x, TruePos.y, ZIndex * ZIndexDamp));
            Vector3 Position2 = TransformMatrix.MultiplyPoint3x4(new Vector3(TruePos.x + Size.x, TruePos.y, ZIndex * ZIndexDamp));
            Vector3 Position3 = TransformMatrix.MultiplyPoint3x4(new Vector3(TruePos.x, TruePos.y + Size.y, ZIndex * ZIndexDamp));
            Vector3 Position4 = TransformMatrix.MultiplyPoint3x4(new Vector3(TruePos.x + Size.x, TruePos.y + Size.y, ZIndex * ZIndexDamp));

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

        //Render Tri

        public static void DrawTriangle(Vector2 Vert1, Vector2 Vert2, Vector2 Vert3) {
            UIMeshElement element = UIMeshes[UIMeshMode.Opaque];

            if (CurrentColor.a < 1) {
                element = UIMeshes[UIMeshMode.Transparent];
            }
            int VertexCount = element.Verticies.Count;

            element.Verticies.Add(TransformMatrix.MultiplyPoint3x4(new Vector3(Vert1.x, Vert1.y, ZIndex * ZIndexDamp)));
            element.Verticies.Add(TransformMatrix.MultiplyPoint3x4(new Vector3(Vert2.x, Vert2.y, ZIndex * ZIndexDamp)));
            element.Verticies.Add(TransformMatrix.MultiplyPoint3x4(new Vector3(Vert3.x, Vert3.y, ZIndex * ZIndexDamp)));

            element.Indicys.Add(VertexCount + 0);
            element.Indicys.Add(VertexCount + 1);
            element.Indicys.Add(VertexCount + 2);

            element.Colors.Add(CurrentColor);
            element.Colors.Add(CurrentColor);
            element.Colors.Add(CurrentColor);

            element.UVs.Add(new Vector2(0, 0));
            element.UVs.Add(new Vector2(1, 0));
            element.UVs.Add(new Vector2(0, 1));

            element.Modifyed = true;


            ZIndex++;
        }

        //Has not been implimented yet
        public static void DrawPolyCircle(Vector2 Position, float Radius, int resolution) {
            UIMeshElement element = UIMeshes[UIMeshMode.Opaque];

            if (CurrentColor.a < 1)
            {
                element = UIMeshes[UIMeshMode.Transparent];
            }
            int VertexCount = element.Verticies.Count;

            element.Verticies.Add(new Vector3(Position.x, Position.y, ZIndex * ZIndexDamp));
            element.Colors.Add(CurrentColor);
            element.UVs.Add(Vector2.one * 0.5f);


            for (int Point = 0; Point <= resolution; Point++) { 
               
            }


            ZIndex++;
        }

        //Draw SDF Circle
        public static void DrawCircle(Vector2 Position, int Radius) {
            UIMeshElement element = UIMeshes[UIMeshMode.Circle];

            if (CurrentColor.a < 1)
            {
                element = UIMeshes[UIMeshMode.CircleTransparent];
            }

            AddQuad(
                element, 
                Vector2Int.FloorToInt(Position) - (Vector2Int.one * (Radius / 2)), 
                Vector2Int.one * Radius, CurrentColor
            );

            ZIndex++;
        }

        public static void DrawLine(Vector2 Point1, Vector2 Point2, float Width) 
        {
            Vector2 LookDirection = (Point1 - Point2).normalized;
            Vector2 LookUPVec = new Vector2(-LookDirection.y, LookDirection.x);

            Vector2 Vert1 = Point1 + LookUPVec * (Width * .5f)  ;
            Vector2 Vert2 = Point1 - LookUPVec * (Width * .5f)  ;
            Vector2 Vert3 = Point2 + LookUPVec * (Width * .5f)  ;
            Vector2 Vert4 = Point2 - LookUPVec * (Width * .5f)  ;

            DrawTriangle(Vert1, Vert2, Vert3);
            DrawTriangle(Vert2, Vert4, Vert3);

        }

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
            if (ScriptableGUIElements != null) { return; }

            ScriptableGUIElements = new Dictionary<string, ScriptableUIRegistry>();
            UIMeshes = new Dictionary<UIMeshMode, UIMeshElement>();

            UIMeshes.Add(UIMeshMode.Opaque, new UIMeshElement());
            UIMeshes.Add(UIMeshMode.Transparent, new UIMeshElement());
            UIMeshes.Add(UIMeshMode.Circle, new UIMeshElement());
            UIMeshes.Add(UIMeshMode.CircleTransparent, new UIMeshElement());

            ScriptableScreenGUIs = new List<ScriptableGUI>();
            ScriptableWorldGUIs = new List<ScriptableGUI>();
        }

        //Called by the imidiate GUI render Feature so it renders above post prosessing
        public static void RenderToScreen(RasterCommandBuffer CMDBuffer)
        {
            
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



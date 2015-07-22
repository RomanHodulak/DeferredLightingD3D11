using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public class VertexDefinition
    {
        public List<VertexParameterType> Parameters { get; set; }

        public VertexDefinition()
        {
            Parameters = new List<VertexParameterType>();
        }
        
        public int SizeInBytes
        {
            get
            {
                int size = 0;
                foreach (VertexParameterType vp in Parameters)
                    size += SizeOf(vp);
                return size;
            }
        }

        public static int SizeOf(VertexParameterType vp)
        {
            int size = 0;
            if (vp == (VertexParameterType.Binormal))
                size = 3 * sizeof(float);
            else if (vp == (VertexParameterType.Normal))
                size = 3 * sizeof(float);
            else if (vp == (VertexParameterType.Position))
                size = 3 * sizeof(float);
            else if (vp == (VertexParameterType.Tangent))
                size = 3 * sizeof(float);
            else if (vp == (VertexParameterType.TextureCoordinate))
                size = 2 * sizeof(float);
            else if (vp == (VertexParameterType.Weights))
                size = 4 * sizeof(float);
            else if (vp == (VertexParameterType.Bones32Bit))
                size = 4 * sizeof(uint);
            else if (vp == (VertexParameterType.ColorRGB))
                size = 3 * sizeof(float);
            return size;
        }

        public int PositionOf(VertexParameterType vp)
        {
            int offset = 0;
            foreach (VertexParameterType vpt in Parameters)
            {
                if (vpt != vp)
                    offset += SizeOf(vpt);
                else
                    break;
            }
            return offset;
        }

        public static SlimDX.DXGI.Format FormatOf(VertexParameterType vp)
        {
            if (vp == VertexParameterType.Position | vp == VertexParameterType.Normal | vp == VertexParameterType.Binormal | vp == VertexParameterType.Tangent | vp == VertexParameterType.ColorRGB)
            {
                return SlimDX.DXGI.Format.R32G32B32_Float;
            }
            else if (vp == VertexParameterType.Bones32Bit)
            {
                return SlimDX.DXGI.Format.R32G32B32A32_UInt;
            }
            else if (vp == VertexParameterType.TextureCoordinate)
            {
                return SlimDX.DXGI.Format.R32G32_Float;
            }
            else if (vp == VertexParameterType.Weights)
            {
                return SlimDX.DXGI.Format.R32G32B32A32_Float;
            }
            return SlimDX.DXGI.Format.Unknown;
        }

        public static string StringOf(VertexParameterType vp)
        {
            if (vp == VertexParameterType.Bones32Bit)
                return "Bones";
            else if (vp == VertexParameterType.ColorRGB)
                return "Color";
            else if (vp == VertexParameterType.TextureCoordinate)
                return "TexCoord";
            return Enum.GetName(typeof(VertexParameterType), vp);
        }

        public SlimDX.Direct3D11.InputElement[] InputElements
        {
            get
            {
                List<SlimDX.Direct3D11.InputElement> ie = new List<SlimDX.Direct3D11.InputElement>();
                foreach (VertexParameterType vp in Parameters)
                {
                    //System.Windows.Forms.MessageBox.Show(VertexDefinition.StringOf(vp) + ", " + VertexDefinition.FormatOf(vp) + ", " + PositionOf(vp));
                    ie.Add(new SlimDX.Direct3D11.InputElement(VertexDefinition.StringOf(vp), 0, VertexDefinition.FormatOf(vp), PositionOf(vp), 0));
                }
                return ie.ToArray();
            }
        }

        public SlimDX.Direct3D11.InputLayout GetInputLayout(SlimDX.D3DCompiler.ShaderBytecode bytecode)
        {
            SlimDX.Direct3D11.InputLayout IL = new SlimDX.Direct3D11.InputLayout(Renderer.Device, bytecode, InputElements);
            return IL;
        }

        public override string ToString()
        {
            return "SizeInBytes: " + SizeInBytes + " Elements per vertex: " + Parameters.Count;
        }
    }

    public abstract class VertexParameter
    {
        public VertexParameterType Type { get; set; }
    }

    public class VertexParameter<T> : VertexParameter where T : struct
    {
        public T Value { get; set; }

        public VertexParameter(VertexParameterType Type, IEquatable<T> Value)
        {
            this.Type = Type;
            this.Value = (T)Value;
        }

        public static bool operator ==(VertexParameter<T> value1, VertexParameter<T> value2)
        {
            return (value1.Value as IEquatable<T>) == (value2.Value as IEquatable<T>) & value1.Type == value2.Type;
        }

        public static bool operator !=(VertexParameter<T> value1, VertexParameter<T> value2)
        {
            return (value1.Value as IEquatable<T>) != (value2.Value as IEquatable<T>) | value1.Type != value2.Type;
        }
    }

    [Serializable]
    public class Vertex
    {
        [NonSerialized]
        public List<VertexParameter> Vertices;

        public Vertex()
        {
            this.Vertices = new List<VertexParameter>();
        }

        public VertexParameter<T> GetVertex<T>(VertexParameterType type) where T : struct
        {
            foreach (VertexParameter i in Vertices)
                if (i.Type == type)
                    return i as VertexParameter<T>;
            return null;
        }

        public static bool operator ==(Vertex value1, Vertex value2)
        {
            return value1.Vertices == value2.Vertices;
        }

        public static bool operator !=(Vertex value1, Vertex value2)
        {
            return value1.Vertices != value2.Vertices;
        }

        //public static DataStream ToStream(params Vertex[] verts)
        //{
        //    foreach (Vertex v in verts)
        //    {
        //        foreach (VertexParameter vparam in v.Vertices)
        //        {
        //            vparam.Type == VertexParameterType.
        //        }
        //    }
        //}
    }

    public class VertexSkinning : Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TexCoord { get; set; }
        public Vector4 Bones { get; set; }
        public Vector4 Weights { get; set; }

        public VertexSkinning(Vector3 Position, Vector3 Normal, Vector2 TexCoord, Vector4 Weights, Vector4 Bones)
        {
            this.Position = Position;
            this.Normal = Normal;
            this.TexCoord = TexCoord;
            this.Bones = Bones;
            this.Weights = Weights;
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Normal, this.Normal));
            this.Vertices.Add(new VertexParameter<Vector2>(VertexParameterType.TextureCoordinate, this.TexCoord));
            this.Vertices.Add(new VertexParameter<Vector4>(VertexParameterType.Bones32Bit, this.Bones));
            this.Vertices.Add(new VertexParameter<Vector4>(VertexParameterType.Weights, this.Weights));
        }
    }

    public class VertexPosNorTex : Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TexCoord { get; set; }

        public VertexPosNorTex(Vector3 Position, Vector3 Normal, Vector2 TexCoord)
        {
            this.Position = Position;
            this.Normal = Normal;
            this.TexCoord = TexCoord;
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Normal, this.Normal));
            this.Vertices.Add(new VertexParameter<Vector2>(VertexParameterType.TextureCoordinate, this.TexCoord));
        }
    }

    public class VertexPosNorTexTanBin : Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TexCoord { get; set; }
        public Vector3 Tangent { get; set; }
        public Vector3 Binormal { get; set; }

        public VertexPosNorTexTanBin(Vector3 Position, Vector3 Normal, Vector2 TexCoord, Vector3 Tangent, Vector3 Binormal)
        {
            this.Position = Position;
            this.Normal = Normal;
            this.TexCoord = TexCoord;
            this.Tangent = Tangent;
            this.Binormal = Binormal;
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Normal, this.Normal));
            this.Vertices.Add(new VertexParameter<Vector2>(VertexParameterType.TextureCoordinate, this.TexCoord));
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Tangent, this.Tangent));
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Binormal, this.Binormal));
        }
    }

    public class VertexPosCol : Vertex
    {
        public Vector3 Position { get; set; }
        public Vector4 Color { get; set; }

        public VertexPosCol(Vector3 Position, Color4 Color)
        {
            this.Position = Position;
            this.Color = Color.ToVector4();
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector4>(VertexParameterType.ColorRGBA, this.Color));
        }

        public VertexPosCol(Vector3 Position, Vector4 Color)
        {
            this.Position = Position;
            this.Color = Color;
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector4>(VertexParameterType.ColorRGBA, this.Color));
        }

        public VertexPosCol(Vector3 Position, Vector3 Color)
        {
            this.Position = Position;
            this.Color = new Vector4(Color, 1);
            this.Vertices.Add(new VertexParameter<Vector3>(VertexParameterType.Position, this.Position));
            this.Vertices.Add(new VertexParameter<Vector4>(VertexParameterType.ColorRGBA, this.Color));
        }
    }

    public enum MatrixType
    {
        World,
        View,
        Projection,
        WorldView,
        ViewProj,
        WorldViewProj,
        InverseView,
        InverseTransposeWorld,
        InverseTransposeView,
    }

    public enum VertexParameterType
    {
        Position,
        Normal,
        TextureCoordinate,
        Tangent,
        Binormal,
        Weights,
        Bones32Bit,
        ColorRGB,
        ColorRGBA,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    public class Camera : Actor
    {
        #region Fields & Properties

        public BoundingFrustum ViewFrustum
        {
            get
            {
                if (viewFrustum == null)
                    SetMatrices();
                return viewFrustum;
            }
        }
        BoundingFrustum viewFrustum;

        public float AspectRatio
        {
            get
            {
                return OutputSize.X / OutputSize.Y;
            }
        }

        public Vector2 OutputSize
        {
            get
            {
                return new Vector2(Viewport.Width, Viewport.Height);
            }
            set
            {
                Viewport = new Viewport(Viewport.X, Viewport.Y, value.X, value.Y);
            }
        }

        public Viewport Viewport
        {
            get
            {
                return viewport;
            }
            set
            {
                viewport = value;
            }
        }
        Viewport viewport = new Viewport(0, 0, 1, 1);

        public Matrix MatrixView
        {
            get
            {
                SetMatrices();
                return matrixView;
            }
            set
            {
                matrixView = value;
                matrixViewProj = matrixView * matrixProj;
            }
        }
        Matrix matrixView;

        public Matrix MatrixProj
        {
            get
            {
                SetMatrices();
                return matrixProj;
            }
            set
            {
                matrixProj = value;
                matrixViewProj = matrixView * matrixProj;
            }
        }
        Matrix matrixProj;

        public Matrix MatrixViewProj
        {
            get
            {
                SetMatrices();
                return matrixViewProj;
            }
        }
        Matrix matrixViewProj;

        public bool UseCustomMatrices = false;
        public Matrix customMatrix = Matrix.Identity;

        public override Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector3 position;

        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private Vector3 rotation;

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        private Vector3 direction;

        public Vector3 Target { get; set; }
        public float Offset { get; set; }

        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        private Vector3 up = new Vector3(0, 1, 0);

        public float FoV
        {
            get { return foV; }
            set { foV = value; }
        }
        private float foV = 45;

        public float zNear
        {
            get { return znear; }
            set { znear = value; }
        }
        private float znear = 0.001f;

        public float zFar
        {
            get { return zfar; }
            set { zfar = value; }
        }
        private float zfar = 100f;

        public CameraProjectionType ProjectionType
        {
            get
            {
                return proj;
            }
            set
            {
                proj = value;
            }
        }
        CameraProjectionType proj = CameraProjectionType.Perspective;

        public CameraViewType ViewType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        CameraViewType type = CameraViewType.FirstPerson;

#endregion

        public Camera(CameraProjectionType proj, CameraViewType view)
        {
            this.ProjectionType = proj;
            this.ViewType = view;
            Target = Vector3.Zero;
            Offset = 1;
            viewFrustum = new BoundingFrustum(Matrix.Identity);
            this.Name = "Camera";
        }

        public Camera()
        {
            Target = Vector3.Zero;
            Offset = 1;
            this.Name = "Camera";
        }

        void SetMatrices()
        {
            if (UseCustomMatrices)
            {
                MatrixView = customMatrix;
                MatrixProj = Matrix.Identity;
                viewFrustum.SetMatrix(ref matrixViewProj);
                return;
            }
            if (type == CameraViewType.Orbital)
            {
                float rotationNormalized = NormalizeRotation();

                Matrix rotationMatrix = Matrix.RotationYawPitchRoll(rotation.X, rotationNormalized + MathHelper.TwoPi, rotation.Z);
                Vector3 transformedReference = MathHelper.ToVector3(Vector3.Transform(new Vector3(0, 1, 0), rotationMatrix)) * Offset;

                Position = transformedReference + Target;
                Direction = Target;

                MatrixView = Matrix.LookAtLH(position, Target, up);
            }
            else if (type == CameraViewType.FirstPerson)
            {
                NormalizeRotation();

                Matrix rotationMatrix = Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
                Vector3 transformedReference = MathHelper.ToVector3(Vector3.Transform(new Vector3(0, -1, 0), rotationMatrix));

                Direction = transformedReference + Position;

                MatrixView = Matrix.LookAtRH(Position, Direction, up);
            }
            else if (type == CameraViewType.Directional)
            {
                //Direction = position + direction;
                MatrixView = Matrix.LookAtRH(Position, Direction, Up);
            }

            float aspectRatio = (float)Viewport.Width / (float)Viewport.Height;
            if (proj == CameraProjectionType.Perspective)
            {
                MatrixProj = Matrix.PerspectiveFovLH(MathHelper.ToRadians(foV), aspectRatio, zfar, znear);
            }
            else if (proj == CameraProjectionType.Ortho)
            {
                MatrixProj = Matrix.OrthoOffCenterRH(-Offset * aspectRatio, Offset * aspectRatio, -Offset, Offset, -50, zFar);
            }
            viewFrustum.SetMatrix(ref matrixViewProj);
        }

        float NormalizeRotation()
        {
            float rotationNormalized = 0;
            if (rotation.Y > 0)
                rotationNormalized = rotation.Y - (MathHelper.TwoPi * (int)(rotation.Y / MathHelper.TwoPi)) - MathHelper.TwoPi;
            else
                rotationNormalized = rotation.Y - (MathHelper.TwoPi * (int)(rotation.Y / MathHelper.TwoPi));

            if (rotationNormalized <= 0 | rotationNormalized >= MathHelper.Pi)
                up = new Vector3(0, -1, 0);
            if (rotationNormalized <= -MathHelper.Pi)
                up = new Vector3(0, 1, 0);
            return rotationNormalized;
        }
    }

    public enum CameraProjectionType
    {
        Perspective,
        Ortho
    }

    public enum CameraViewType
    {
        FirstPerson,
        Orbital,
        Directional,
    }
}

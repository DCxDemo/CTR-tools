﻿using ctrviewer.Engine.Render;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ctrviewer.Engine
{
    public enum CameraType
    {
        DefaultCamera,
        LeftEyeCamera,
        RightEyeCamera,
        SkyCamera
    }

    public partial class MainEngine : IDisposable
    {
        public EngineSettings settings;

        public Dictionary<CameraType, FirstPersonCamera> Cameras = new Dictionary<CameraType, FirstPersonCamera>();

        public List<InstancedModel> external = new List<InstancedModel>();
        public List<InstancedModel> instanced = new List<InstancedModel>();
        public List<InstancedModel> paths = new List<InstancedModel>();

        //hi and low scenes converted to monogame
        public List<MGLevel> MeshHigh = new List<MGLevel>();
        public List<MGLevel> MeshLow = new List<MGLevel>();

        //bounding boxes for visdata
        public List<WireBox> bbox = new List<WireBox>();
        public Dictionary<int, List<WireBox>> bbox2 = new Dictionary<int, List<WireBox>>();


        //sky
        public MGLevel sky;
        public Color BackgroundColor = Color.DarkBlue;

        public MainEngine(Game game)
        {
            InitializeCameras(game);

            Settings.onFieldOfViewChanged += UpdateFOV;
        }

        public void InitializeCameras(Game game)
        {
            Cameras.Add(CameraType.DefaultCamera, new FirstPersonCamera(game));
            Cameras.Add(CameraType.LeftEyeCamera, new FirstPersonCamera(game));
            Cameras.Add(CameraType.RightEyeCamera, new FirstPersonCamera(game));
            Cameras.Add(CameraType.SkyCamera, new FirstPersonCamera(game));
        }

        public void UpdateProjectionMatrices()
        {
            foreach (var camera in Cameras.Values)
                camera.UpdateProjectionMatrix();
        }

        public void UpdateStereoCamera(CameraType cameraType, float separationValue)
        {
            Vector3 moveVector = Vector3.Transform((cameraType == CameraType.RightEyeCamera ? Vector3.Left : Vector3.Right) * separationValue / 100f, Cameras[CameraType.DefaultCamera].GetYawPitchRollMatrix());
            Cameras[cameraType].Position = Cameras[CameraType.DefaultCamera].Position + moveVector;
            Cameras[cameraType].rotationSpeed = Cameras[CameraType.DefaultCamera].rotationSpeed;
            Cameras[cameraType].Target = Cameras[CameraType.DefaultCamera].Target;
        }

        public void UpdateFOV()
        {
            foreach (var camera in Cameras.Values)
                camera.ViewAngle = Settings.FieldOfView;
        }

        public void Draw()
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Clear()
        {
            sky = null;
            instanced.Clear();
            MeshHigh.Clear();
            MeshLow.Clear();
            paths.Clear();
            ContentVault.Clear();
            bbox.Clear();
            bbox2.Clear();
        }

        public void Dispose()
        {
            Settings.Save();
            Settings.onFieldOfViewChanged -= UpdateFOV;

            Cameras.Clear();
            this.Clear();
        }
    }
}

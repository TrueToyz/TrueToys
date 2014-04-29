<?xml version="1.0" ?>
<MiddleVR>
    <Kernel LogLevel="2" LogInSimulationFolder="0" EnableCrashHandler="0" Version="1.4.0.b3" />
    <DeviceManager WandAxis="RazerHydra.Joystick0.Axis" WandHorizontalAxis="0" WandHorizontalAxisScale="1" WandVerticalAxis="1" WandVerticalAxisScale="1" WandButtons="RazerHydra.Joystick0.Buttons" WandButton0="0" WandButton1="6" WandButton2="5" WandButton3="5" WandButton4="5" WandButton5="5">
        <Driver Type="vrDriverDirectInput" />
        <Driver Type="vrDriverOculusRift" />
        <Driver Type="vrDriverRazerHydra" />
    </DeviceManager>
    <DisplayManager Fullscreen="0" WindowBorders="0" ShowMouseCursor="0" VSync="1" AntiAliasing="0" ForceHideTaskbar="0" SaveRenderTarget="0">
        <Node3D Name="CenterNode" Parent="VRRootNode" Tracker="0" PositionLocal="0.000000,0.000000,0.000000" OrientationLocal="0.000000,0.000000,0.000000,1.000000" />
        <Node3D Name="RazerHydraBaseOffset" Parent="CenterNode" Tracker="0" PositionLocal="0.000000,0.000000,0.065000" OrientationLocal="0.000000,0.000000,0.000000,1.000000" />
        <Node3D Name="HandNode" Tag="Hand" Parent="RazerHydraBaseOffset" Tracker="RazerHydra.Tracker0" UseTrackerX="1" UseTrackerY="1" UseTrackerZ="1" UseTrackerYaw="1" UseTrackerPitch="1" UseTrackerRoll="1" />
        <Node3D Name="HeadTracking" Parent="RazerHydraBaseOffset" Tracker="RazerHydra.Tracker1" OrientationLocal="0.000000,0.000000,0.000000,1.000000" UseTrackerX="1" UseTrackerY="1" UseTrackerZ="1" UseTrackerYaw="0" UseTrackerPitch="0" UseTrackerRoll="0" />
        <Node3D Name="OffsetRazerHydraHeadTracking" Parent="HeadTracking" Tracker="0" PositionLocal="0.040000,-0.080000,0.000000" OrientationLocal="0.000000,0.000000,0.000000,1.000000" />
        <Node3D Name="NeckNode" Parent="OffsetRazerHydraHeadTracking" Tracker="OculusRift.Tracker" PositionLocal="0.000000,0.000000,0.000000" UseTrackerX="1" UseTrackerY="1" UseTrackerZ="0" UseTrackerYaw="1" UseTrackerPitch="1" UseTrackerRoll="1" />
        <Node3D Name="HeadNode" Parent="NeckNode" Tracker="0" PositionLocal="0.000000,0.000000,0.000000" OrientationLocal="0.000000,0.000000,0.000000,1.000000" />
        <CameraStereo Name="CameraStereo0" Parent="HeadNode" Tracker="0" PositionLocal="0.000000,0.000000,0.000000" OrientationLocal="0.000000,0.000000,0.000000,1.000000" VerticalFOV="111.211" Near="0.03" Far="1000" Screen="0" ScreenDistance="1" UseViewportAspectRatio="0" AspectRatio="0.8" InterEyeDistance="0.065" LinkConvergence="0" />
        <Viewport Name="Viewport0" Left="0" Top="0" Width="1280" Height="800" Camera="CameraStereo0" Stereo="1" StereoMode="3" CompressSideBySide="0" StereoInvertEyes="0" OculusRiftWarping="1" />
    </DisplayManager>
    <ClusterManager NVidiaSwapLock="0" DisableVSyncOnServer="1" ForceOpenGLConversion="0" BigBarrier="0" SimulateClusterLag="0" />
</MiddleVR>

using Godot;
using System;
using GDExtension.Wrappers;

public partial class VisionTask : Control
{
    public string                                MainScenePath = "res://Main.tscn";
    public MediaPipeTask.VisionRunningMode       RunningMode   = MediaPipeTask.VisionRunningMode.RunningModeImage;
    public MediaPipeTaskBaseOptions.DelegateEnum DelegateType  = MediaPipeTaskBaseOptions.DelegateEnum.DelegateCpu;
    public MediaPipeCameraHelper                 CameraHelper  = MediaPipeCameraHelper.Instantiate();

    public TextureRect       ImageView;
    public VideoStreamPlayer VideoPlayer;
    public Button            BtnBack;
    public Button            BtnLoadImage;
    public Button            BtnLoadVideo;
    public Button            BtnOpenCamera;
    public FileDialog        ImageFileDialog;
    public FileDialog        VideoFileDialog;
    public AcceptDialog      PermissionDialog;
    public void InitNode()
    {
        ImageView = GetNode<TextureRect>("VBoxContainer/Image");
        VideoPlayer = GetNode<VideoStreamPlayer>("Video");
        BtnBack = GetNode<Button>("VBoxContainer/Title/Back");
        BtnLoadImage = GetNode<Button>("VBoxContainer/Buttons/LoadImage");
        BtnLoadVideo = GetNode<Button>("VBoxContainer/Buttons/LoadVideo");
        BtnOpenCamera = GetNode<Button>("VBoxContainer/Buttons/OpenCamera");
        ImageFileDialog = GetNode<FileDialog>("ImageFileDialog");
        VideoFileDialog = GetNode<FileDialog>("VideoFileDialog");
        PermissionDialog = GetNode<AcceptDialog>("PermissionDialog");

    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitNode();
        BtnBack.Pressed += _Back;
        BtnLoadImage.Pressed += ImageFileDialogPopupCenteredRatio;
        BtnLoadVideo.Pressed += VideoFileDialogPopupCenteredRatio;
        BtnOpenCamera.Pressed += _OpenCamera;
        ImageFileDialog.FileSelected += _LoadImage;
        ImageFileDialog.RootSubfolder = OS.GetSystemDir(OS.SystemDir.Pictures);
        VideoFileDialog.FileSelected += _LoadVideo;
        VideoFileDialog.RootSubfolder = OS.GetSystemDir(OS.SystemDir.Movies);
        CameraHelper.PermissionResult += _PermissionResult;
        CameraHelper.NewFrame += _CameraFrame;
        if (OS.GetName() == "Android")
        {
            CameraHelper.SetGpuResources(MediaPipeGPUResources.Instantiate());
        }
        InitTask();
        
    }
    public void ImageFileDialogPopupCenteredRatio()
    {
       ImageFileDialog.PopupCenteredRatio();
    }
    public void VideoFileDialogPopupCenteredRatio()
    {
        VideoFileDialog.PopupCenteredRatio();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!VideoPlayer.IsPlaying()) return;
        var texture = VideoPlayer.GetVideoTexture();
        var image   = texture?.GetImage();
        if (image is null) return;
        if (!(RunningMode == MediaPipeTask.VisionRunningMode.RunnineModeVideo))
        {
            RunningMode = MediaPipeTask.VisionRunningMode.RunnineModeVideo;
            InitTask();
        }
        ProcessVideoFrame(image, Time.GetTicksMsec());
    }
    
    public virtual void _Back()
    {
        GetTree().ChangeSceneToFile(MainScenePath);
    }
    public virtual void _LoadImage(string path)
    {
       Reset();
       if (RunningMode != MediaPipeTask.VisionRunningMode.RunnineModeVideo)
       {
           RunningMode = MediaPipeTask.VisionRunningMode.RunnineModeVideo;
           InitTask();
       }
       var image = Image.LoadFromFile(path);
       ProcessImageFrame(image);
    }
    public virtual void _LoadVideo(string path)
    {
        Reset();
        VideoPlayer.Stream = GD.Load<VideoStream>(path);
        VideoPlayer.Play();
    }
    public virtual void _OpenCamera()
    {
        if (CameraHelper.PermissionGranted())
        {
            StartCamera();
            return;
        }
        CameraHelper.RequestPermission();
    }
    public virtual void _PermissionResult(bool granted)
    {
        if (granted)
        {
            StartCamera();
            return;
        }
        PermissionDialog.PopupCentered();
    }
    public virtual void _CameraFrame(MediaPipeImage image)
    {
        if (RunningMode != MediaPipeTask.VisionRunningMode.RunningModeLiveStream)
        {
            RunningMode = MediaPipeTask.VisionRunningMode.RunningModeLiveStream;
            InitTask();
        }
        if (DelegateType == MediaPipeTaskBaseOptions.DelegateEnum.DelegateCpu && image.IsGpuImage())
        {
            image.ConvertToCpu();
        }
        ProcessCameraFrame(image, Time.GetTicksMsec());
    }
    public virtual void InitTask()
    {

    }
    public virtual void ProcessVideoFrame(Image image, ulong timestamp)
    {

    }
    public virtual void ProcessImageFrame(Image image)
    {

    }
    public virtual void ProcessCameraFrame(MediaPipeImage image, ulong timestamp)
    {

    }
    public void UpdateImage(Image image)
    {
        CallDeferred(VisionTask.MethodName.SetTexture, image);
    }
    public void SetTexture(Image image)
    {
        ImageView.Texture = ImageTexture.CreateFromImage(image);;
    }
    public void StartCamera()
    {
        Reset();
        CameraHelper.SetMirrored(true);
        CameraHelper.Start((int)MediaPipeCameraHelper.CameraFacing.FacingFront, new Vector2(640, 480));
    }
    public void Reset()
    {
        VideoPlayer.Stop();
        CameraHelper.Close();
    }
}
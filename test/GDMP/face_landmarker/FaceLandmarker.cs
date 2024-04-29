using Godot;
using System;
using System.Linq;
using GDExtension.Wrappers;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class FaceLandmarker : VisionTask
{
    public MediaPipeFaceLandmarker Task;
    public string                  TaskFile = "res://vision/face_landmarker/face_landmarker_v2_with_blendshapes.task";
    public Label                   LabelBlendShapes;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LabelBlendShapes = GetNode<Label>("VBoxContainer/Image/Blendshapes");
        base._Ready();
    }

    public virtual void _ResultCallback(MediaPipeFaceLandmarkerResult result, MediaPipeImage image, int timestampMs)
    {
        var img = image.GetImage();
        ShowResult(img, result);
    }
    public override void InitTask()
    {
        var options = MediaPipeTaskBaseOptions.Instantiate();
        options.Delegate = (int)DelegateType;
        using var file = FileAccess.Open(TaskFile, FileAccess.ModeFlags.Read);
        options.ModelAssetBuffer = file.GetBuffer((long)file.GetLength());
        Task = MediaPipeFaceLandmarker.Instantiate();
        Task.Initialize(options, (int)RunningMode, 1, 0.5f, 0.5f, 0.5f, true, true);
        Task.ResultCallback += _ResultCallback;
    }
    public override void ProcessImageFrame(Image image)
    {
        var inputImage = MediaPipeImage.Instantiate();
        inputImage.SetImage(image);
        var result = Task.Detect(inputImage, default, default);
        ShowResult(image, result);
    }
    public override void ProcessVideoFrame(Image image, ulong timestamp)
    {
        var inputImage = MediaPipeImage.Instantiate();
        inputImage.SetImage(image);
        var result = Task.DetectVideo(inputImage, (int)timestamp, default, default);
        ShowResult(image, result);

    }
    public override void ProcessCameraFrame(MediaPipeImage image, ulong timestamp)
    {
        Task.DetectAsync(image, (int)timestamp, default, default);
    }

    public void ShowResult(Image image, MediaPipeFaceLandmarkerResult result)
    {
        var faceLandmarks = new Array<GodotObject>(result.FaceLandmarks).Select(MediaPipeNormalizedLandmarks.Bind);
        foreach (var landmark in faceLandmarks)
        {
            DrawLandmarks(image, landmark);
        }
        if (result.HasFaceBlendshapes())
        {
            var blendShapes = new Array<GodotObject>(result.FaceBlendshapes).Select(MediaPipeClassifications.Bind);
            foreach (var blendShape in blendShapes)
            {
                CallDeferred(MethodName.ShowBlendShapers, image, blendShape);
            }
        }
        UpdateImage(image);
    }
    public void DrawLandmarks(Image image, MediaPipeNormalizedLandmarks landmarks)
    {
        var color = Colors.Green;
        var rect  = Image.Create(4, 4, false, image.GetFormat());
        rect.Fill(color);
        var imageSize      = (Vector2)image.GetSize();
        var landmarksArray = new Array<GodotObject>(landmarks.Landmarks).Select(MediaPipeNormalizedLandmark.Bind);
        foreach (var landmark in landmarksArray)
        {
            var pos = new Vector2(landmark.X, landmark.Y);
            image.BlitRect(rect, rect.GetUsedRect(), ((Vector2I)(imageSize * pos)) - rect.GetSize() / 2);
        }

    }
    public void ShowBlendShapers(Image image, MediaPipeClassifications blendShapes)
    {
        LabelBlendShapes.Text = "";
        var categories = new Array<GodotObject>(blendShapes.Categories).Select(MediaPipeCategory.Bind);
        foreach (var category in categories)
        {
            if (category.Score >= 0.5 && category.HasCategoryName())
            {
                LabelBlendShapes.Text += $"{category.CategoryName}: {category.Score}\n";
            }
        }
    }

}
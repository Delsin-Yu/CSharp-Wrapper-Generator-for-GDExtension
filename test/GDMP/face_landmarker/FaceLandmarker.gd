extends VisionTask

var task: MediaPipeFaceLandmarker
var task_file := "res://vision/face_landmarker/face_landmarker_v2_with_blendshapes.task"

@onready var lbl_blendshapes: Label = $VBoxContainer/Image/Blendshapes

func _result_callback(result: MediaPipeFaceLandmarkerResult, image: MediaPipeImage, timestamp_ms: int) -> void:
	var img := image.get_image()
	show_result(img, result)

func init_task() -> void:
	var base_options := MediaPipeTaskBaseOptions.new()
	base_options.delegate = delegate
	var file := FileAccess.open(task_file, FileAccess.READ)
	base_options.model_asset_buffer = file.get_buffer(file.get_length())
	task = MediaPipeFaceLandmarker.new()
	task.initialize(base_options, running_mode, 1, 0.5, 0.5, 0.5, true)
	task.result_callback.connect(self._result_callback)

func process_image_frame(image: Image) -> void:
	var input_image := MediaPipeImage.new()
	input_image.set_image(image)
	var result := task.detect(input_image)
	show_result(image, result)

func process_video_frame(image: Image, timestamp_ms: int) -> void:
	var input_image := MediaPipeImage.new()
	input_image.set_image(image)
	var result := task.detect_video(input_image, timestamp_ms)
	show_result(image, result)

func process_camera_frame(image: MediaPipeImage, timestamp_ms: int) -> void:
	task.detect_async(image, timestamp_ms)

func show_result(image: Image, result: MediaPipeFaceLandmarkerResult) -> void:
	for landmarks in result.face_landmarks:
		draw_landmarks(image, landmarks)
	if result.has_face_blendshapes():
		for blendshape in result.face_blendshapes:
			call_deferred("show_blendshapes", image, blendshape)
	update_image(image)

func draw_landmarks(image: Image, landmarks: MediaPipeNormalizedLandmarks) -> void:
	var color := Color.GREEN
	var rect := Image.create(4, 4, false, image.get_format())
	rect.fill(color)
	var image_size := Vector2(image.get_size())
	for landmark in landmarks.landmarks:
		var pos := Vector2(landmark.x, landmark.y)
		image.blit_rect(rect, rect.get_used_rect(), Vector2i(image_size * pos) - rect.get_size() / 2)

func show_blendshapes(image: Image, blendshapes: MediaPipeClassifications) -> void:
	lbl_blendshapes.text = ""
	for category in blendshapes.categories:
		if category.score >= 0.5:
			if category.has_category_name():
				lbl_blendshapes.text += "%s: %.2f\n" % [category.category_name, category.score]

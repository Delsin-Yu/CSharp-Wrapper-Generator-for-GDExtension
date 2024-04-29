class_name VisionTask
extends Control

var main_scene := preload("res://Main.tscn")
var running_mode := MediaPipeTask.RUNNING_MODE_IMAGE
var delegate := MediaPipeTaskBaseOptions.DELEGATE_CPU
var camera_helper := MediaPipeCameraHelper.new()

@onready var image_view: TextureRect = $VBoxContainer/Image
@onready var video_player: VideoStreamPlayer = $Video
@onready var btn_back: Button = $VBoxContainer/Title/Back
@onready var btn_load_image: Button = $VBoxContainer/Buttons/LoadImage
@onready var btn_load_video: Button = $VBoxContainer/Buttons/LoadVideo
@onready var btn_open_camera: Button = $VBoxContainer/Buttons/OpenCamera
@onready var image_file_dialog: FileDialog = $ImageFileDialog
@onready var video_file_dialog: FileDialog = $VideoFileDialog
@onready var permission_dialog: AcceptDialog = $PermissionDialog

func _ready():
	btn_back.pressed.connect(self._back)
	btn_load_image.pressed.connect(image_file_dialog.popup_centered_ratio)
	btn_load_video.pressed.connect(video_file_dialog.popup_centered_ratio)
	btn_open_camera.pressed.connect(self._open_camera)
	image_file_dialog.file_selected.connect(self._load_image)
	image_file_dialog.root_subfolder = OS.get_system_dir(OS.SYSTEM_DIR_PICTURES)
	video_file_dialog.file_selected.connect(self._load_video)
	video_file_dialog.root_subfolder = OS.get_system_dir(OS.SYSTEM_DIR_MOVIES)
	camera_helper.permission_result.connect(self._permission_result)
	camera_helper.new_frame.connect(self._camera_frame)
	if OS.get_name() == "Android":
		var gpu_resources := MediaPipeGPUResources.new()
		camera_helper.set_gpu_resources(gpu_resources)
	init_task()

func _process(delta: float) -> void:
	if video_player.is_playing():
		var texture := video_player.get_video_texture()
		if texture:
			var image := texture.get_image()
			if image:
				if not running_mode == MediaPipeTask.RUNNINE_MODE_VIDEO:
					running_mode = MediaPipeTask.RUNNINE_MODE_VIDEO
					init_task()
				process_video_frame(image, Time.get_ticks_msec())

func _back() -> void:
	reset()
	get_tree().change_scene_to_packed(main_scene)

func _load_image(path: String) -> void:
	reset()
	if not running_mode == MediaPipeTask.RUNNING_MODE_IMAGE:
		running_mode = MediaPipeTask.RUNNING_MODE_IMAGE
		init_task()
	var image := Image.load_from_file(path)
	process_image_frame(image)

func _load_video(path: String) -> void:
	reset()
	var stream: VideoStream = load(path)
	video_player.stream = stream
	video_player.play()

func _open_camera() -> void:
	if camera_helper.permission_granted():
		start_camera()
	else:
		camera_helper.request_permission()

func _permission_result(granted: bool) -> void:
	if granted:
		start_camera()
	else:
		permission_dialog.popup_centered()

func _camera_frame(image: MediaPipeImage) -> void:
	if not running_mode == MediaPipeTask.RUNNING_MODE_LIVE_STREAM:
		running_mode = MediaPipeTask.RUNNING_MODE_LIVE_STREAM
		init_task()
	if delegate == MediaPipeTaskBaseOptions.DELEGATE_CPU and image.is_gpu_image():
		image.convert_to_cpu()
	process_camera_frame(image, Time.get_ticks_msec())

func init_task() -> void:
	pass

func process_image_frame(image: Image) -> void:
	pass

func process_video_frame(image: Image, timestamp_ms: int) -> void:
	pass

func process_camera_frame(image: MediaPipeImage, timestamp_ms: int) -> void:
	pass

func update_image(image: Image) -> void:
	if Vector2i(image_view.texture.get_size()) == image.get_size():
		image_view.texture.call_deferred("update", image)
	else:
		image_view.texture.call_deferred("set_image", image)

func start_camera() -> void:
	reset()
	camera_helper.set_mirrored(true)
	camera_helper.start(MediaPipeCameraHelper.FACING_FRONT, Vector2(640, 480))

func reset() -> void:
	video_player.stop()
	camera_helper.close()

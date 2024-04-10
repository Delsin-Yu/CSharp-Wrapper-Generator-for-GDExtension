class_name CustomMainLoop

extends SceneTree

func _initialize():
	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_START");
	for name in ClassDB.get_class_list():
		print(name);
	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_END");
	quit();

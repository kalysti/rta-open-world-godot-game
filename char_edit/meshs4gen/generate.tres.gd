extends Node
export var cloth_array=[]

func _ready():
	#Это генерация нужных массивов.
	var forms= load("res://char_edit/char_edit_GUI.tscn").instance()
	add_child(forms)
	forms = forms.forms
	
	var blndshp = []
	var shp_name_index = {}
	var mesh_arrs = load("res://char_edit/meshs4gen/body.mesh")
	for i in range(mesh_arrs.get_blend_shape_count ()):
		shp_name_index[mesh_arrs.get_blend_shape_name(i)] = i
	var blends = mesh_arrs.surface_get_blend_shape_arrays(0)
	mesh_arrs = mesh_arrs.surface_get_arrays(0)
	var mesh = Mesh.new()
	mesh.add_surface_from_arrays (Mesh.PRIMITIVE_TRIANGLES,mesh_arrs)
	ResourceSaver.save("res://char_edit/meshs/body.mesh",mesh,32)
	var base_form = mesh_arrs[Mesh.ARRAY_VERTEX]
	var vertex_UV = mesh_arrs[Mesh.ARRAY_TEX_UV]
	for i in range(len(blends)):
		var temp = blends[i][0]
		for j in range(len (base_form)):
			temp[j] -= base_form [j]
		blndshp.push_back(temp)
	var tex = load("res://char_edit/meshs4gen/vertex_groups.png").get_data() # на текстуре надо следить, чтобы одинаковые вершины на швах UV были в одной группе, иначе будет дырка в меше.
	var tex_size = tex.get_size()
	tex.lock()
	for i in forms["body"].keys():
		var index=shp_name_index[i]
		for j in range(len(blndshp[index])):
			if forms["body"][i]:
				var uv_coord =vertex_UV[j]*tex_size
				if not tex.get_pixel(uv_coord.x,uv_coord.y) in forms["body"][i]: #Тут мы применяем группы вершин по текстуре, чтобы форма носа не влияла на всё тело.
					blndshp[index][j]=Vector3.ZERO
	for i in forms["head"].keys():
		var index=shp_name_index[i]
		for j in range(len(blndshp[index])):
			if forms["head"][i]:
				var uv_coord =vertex_UV[j]*tex_size
				if not tex.get_pixel(uv_coord.x,uv_coord.y) in forms["head"][i]:
					blndshp[index][j]=Vector3.ZERO
	for i in forms["exp"].keys():
		var index=shp_name_index[i]
		for j in range(len(blndshp[index])):
			if forms["exp"][i]:
				var uv_coord =vertex_UV[j]*tex_size
				if not tex.get_pixel(uv_coord.x,uv_coord.y) in forms["exp"][i]:
					blndshp[index][j]=Vector3.ZERO
	tex.unlock()
	var meshs_shapes={}
	meshs_shapes["body"] = {}
	meshs_shapes["body"]["blendshapes"] = blndshp.duplicate()
	meshs_shapes["body"]["base_form"] = base_form
	meshs_shapes["body"]["shp_name_index"] = shp_name_index.duplicate()
	meshs_shapes["forms"] = forms
	print(len(meshs_shapes["body"]["base_form"]))
	for text in cloth_array:
		shp_name_index = {}
		blndshp = []
		mesh_arrs = load("res://char_edit/meshs4gen/"+text+".mesh")
		for i in range(mesh_arrs.get_blend_shape_count ()):
			shp_name_index[mesh_arrs.get_blend_shape_name(i)] = i
		blends = mesh_arrs.surface_get_blend_shape_arrays(0)
		mesh_arrs = mesh_arrs.surface_get_arrays(0)
		base_form= mesh_arrs[Mesh.ARRAY_VERTEX]
		mesh = Mesh.new()
		mesh.add_surface_from_arrays (Mesh.PRIMITIVE_TRIANGLES,mesh_arrs)
		ResourceSaver.save("res://char_edit/meshs/"+text+".mesh",mesh,32)
		for i in range(len(blends)):
			var temp = blends[i][0]
			for j in range(len (base_form)):
				temp[j] -= base_form[j]
			blndshp.push_back(temp)
		meshs_shapes[text] ={}
		meshs_shapes[text]["blendshapes"] = blndshp.duplicate()
		meshs_shapes[text]["base_form"] = base_form
		meshs_shapes[text]["shp_name_index"] = shp_name_index.duplicate()
		print (text +": "+ str(len(meshs_shapes[text]["base_form"])))
	var file=File.new()
	file.open_compressed("res://char_edit/shapes.dat",File.WRITE)
	file.store_var(meshs_shapes)
	file.close()
	get_tree().quit()

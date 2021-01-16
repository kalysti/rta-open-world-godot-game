# Licensed under the MIT License.
# Copyright (c) 2018 Leonardo (Toshiwo) Araki

tool
extends Spatial

var earth_mat = preload("res://addons/TerrainLoader/helpers/TerrainMaterial.tres")
var smf = preload("res://addons/TerrainLoader/helpers/slippy_map_functions.gd")
var sth = preload("res://addons/TerrainLoader/helpers/surface_tool_helper.gd")
var array_mesh_tool = load("res://addons/TerrainLoader/helpers/ArrayMeshHelper.gd")
var earth_circ = smf.radius_to_circ(smf.EARTH_RADIUS)

var sq_heights = [[0,1,2],[0,1,2],[0,1,2]]
# the heightmap image width and heigth
var width = 0.0
var heigth = 0.0
var surf_tool =  sth.new()
var max_min_height = null
var altitude_multiplier = 0.0

func _init():
	pass
	
static func _subsetToXYCoords(_subset, _divideinto):
	var subsetX = 1
	var subsetY = 1
	for ss in range(1, _subset):
		subsetX += 1
		if subsetX > _divideinto:
			subsetY += 1
			subsetX = 1
	
#	print("Subset: " + var2str(_subset) + " - Divide into: " + var2str(_divideinto)
#	+ " | Y:" + var2str(subsetY) + " - X:" + var2str(subsetX))
	return {x=subsetX, y=subsetY}
	
static func GetPixelDistance(_heightMap, _totalSize):
	var dist = 0
	if _heightMap != null:
		var hmSize = float(_heightMap.size())
		var tileSize = hmSize
		if _totalSize == null:
			_totalSize = 0.0
		if _totalSize > 0.0:
			tileSize = _totalSize
		dist = float (tileSize / hmSize)
	return dist
	
static func GetMaxMinHight(_img = Image.new()):
	var MaxMin = {minh=999999999.0, maxh=-1.0}
	var alt_tl = 0.0
	_img.lock()
	for y in range(_img.get_height()):
		for x in range(_img.get_width()):
			alt_tl = GetHeightFromPxl(_img.get_pixel(x, y))
			if alt_tl < MaxMin.minh:
				MaxMin.minh = alt_tl
			if alt_tl > MaxMin.maxh:
				MaxMin.maxh = alt_tl
	_img.unlock()
	return MaxMin

# This is the formula that MapBox provides to convert HM image into meters
static func GetHeightFromPxl(_pxl):
	return -10000.0 + ((_pxl.r8 * 65536.0 + _pxl.g8 * 256.0 + _pxl.b8) * 0.1)

func GetImageSubset(_image, _divideinto, _subset, _addpixel = Vector2(0, 0)):
	if _image != null:
		var coords = _subsetToXYCoords(_subset, _divideinto)
		var imgsswidth = _image.get_width() / _divideinto
		var imgssheight = _image.get_height() / _divideinto
		var imgstart = Vector2( imgsswidth * (coords.x - 1),
								imgssheight * (coords.y - 1))
		var imgssize = Vector2(imgsswidth + _addpixel.x, imgssheight + _addpixel.y)
		var imgsbst = _image.get_rect(Rect2(imgstart, imgssize))
		return imgsbst

func SetMaterialTexture(_txtr_img):
	var imgtxtr = ImageTexture.new()
	imgtxtr.create_from_image(_txtr_img)
	var mat = earth_mat.duplicate() #This way it clones the material for each instance
	mat.albedo_texture = imgtxtr
	return mat

#	Generates an array of arrays that contains
#	height information from an image coming from 
#	Mapbox elevation data, AKA Mapbox Terrain-RGB:
#	https://www.mapbox.com/help/access-elevation-data/#mapbox-terrain-rgb
func GenerateHeightMap(_hm_img = Image.new()):
	var hm = Array()
	var minh = 999999.0
	var maxh = -1.0
	if !_hm_img.is_empty():
		var startt = float(OS.get_ticks_msec())
		var TerrainImage = _hm_img
		TerrainImage.lock()
		var width = TerrainImage.get_width()
		var heigth = TerrainImage.get_height()
		var rangeX = range(width)
		var rangeY = range(heigth)
		var pxl = Color()
		var altitude = 0.0
		hm.resize(heigth)
		for y in rangeY:
			var x_arr = Array()
			x_arr.resize(width)
			for x in rangeX:
				pxl = TerrainImage.get_pixel(x, y)
				altitude = -10000.0 + ((pxl.r8 * 65536.0 + pxl.g8 * 256.0 + pxl.b8) * 0.1)
				x_arr[x] =  altitude
				if altitude < minh:
					minh = altitude
				if altitude > maxh:
					maxh = altitude
			hm[y] = x_arr
		TerrainImage.unlock()
		var endtt = float(OS.get_ticks_msec())
		print("Heightmap of"
		+ " W/H: " + var2str(width) + "/" + var2str(heigth) 
		+ ", Min/Max height: " + var2str(minh) + "/" + var2str(maxh)
		+ " generated in %.2f seconds" % ((endtt - startt)/1000.0))
	return {heightmap = hm, min_height = minh, max_height = maxh}

func createMeshFromImage(_hm_img = Image.new(), _txtr_img = Image.new(), total_size = 0, height_multiplier = 1, Zoom = 1, _tilex = 0.0, _tiley = 0.0, _subset = 1, _divideinto = 4, _remove_offset = false, _mesh_path = null):
	var arr_tool = array_mesh_tool.new() 
	var color_vertices = false
	var coords = _subsetToXYCoords(_subset, _divideinto)
	if !_hm_img.is_empty() && !_txtr_img.is_empty():
		var startt = float(OS.get_ticks_msec())
		var pxl_mtrs = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, _tiley, Zoom)
		# generated with the array mesh helper
		var hm_array = GenerateHeightMap(_hm_img)
		max_min_height = hm_array
		arr_tool.heights_to_squares_array(hm_array.heightmap, SetMaterialTexture(_txtr_img), _divideinto, pxl_mtrs, hm_array.min_height)
#		arr_tool.regen_normalmaps()
		
		var endtt = float(OS.get_ticks_msec())
		print("Mesh generation"
		+ " Tile " + var2str(_subset) + "/" + var2str(_divideinto * _divideinto)
		+ " X/Y: " + var2str(coords["x"]) + "/" + var2str(coords["y"])
		+ " Meters/Pixel: " + var2str(pxl_mtrs)
		+ " Min/Max Alt.: " + var2str(hm_array.min_height) + "/" + var2str(hm_array.max_height)
		+ " finished in %.2f seconds" % ((endtt - startt)/1000.0))
		return arr_tool
		
#func CreateMeshFromImage_sph(_hm_img = Image.new(), _txtr_img = Image.new(), total_size = 0, height_multiplier = 1, Zoom = 1, _tilex = 0, _tiley = 0, _subset = 1, _divideinto = 4, _remove_offset = false, _mesh_path = null):
#	var color_vertices = false
#	var coords = _subsetToXYCoords(_subset, _divideinto)
#	var lastvalx = 1
#	if coords["x"] == _divideinto:
#		lastvalx = -1
#	var lastvaly = 1
#	if coords["y"] == _divideinto:
#		lastvaly = -1
#	if !_hm_img.is_empty() && !_txtr_img.is_empty():
#		var surf_tool =  sth.new()#SurfaceTool.new()
#		var startt = float(OS.get_ticks_msec())
#		var hm_sbs_img = _hm_img
#		var txtr_sbs_img = _txtr_img
#		if _divideinto > 1:
#			hm_sbs_img = GetImageSubset(_hm_img, _divideinto, _subset, Vector2(lastvalx, lastvaly))
#			txtr_sbs_img = GetImageSubset(_txtr_img, _divideinto, _subset, Vector2(lastvalx, lastvaly))
#		var max_min_h = GetMaxMinHight(hm_sbs_img)
#		hm_sbs_img.lock()
#		txtr_sbs_img.lock()
#		var width = hm_sbs_img.get_width()
#		var heigth = hm_sbs_img.get_height()
#		var step_size = 2
#		var rangeX = range(0,width, step_size)
#		var rangeY = range(0,heigth, step_size)
#		var x_limiter = 0
#		var y_limiter = 0
#		var pxl_mtrs_max = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, _tiley, Zoom)
#		if pxl_mtrs_max < smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley+1), Zoom):
#			pxl_mtrs_max = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley+1), Zoom)
#		var pxl_mtrs_t = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley), Zoom)
#		var pxl_mtrs_b = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley), Zoom)
#		var pxl_mtrs_b2 = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley), Zoom)
#		var dist_proportion_t = pxl_mtrs_t / pxl_mtrs_max
#		var dist_proportion_b = pxl_mtrs_b / pxl_mtrs_max
#		var dist_proportion_b2 = pxl_mtrs_b2 / pxl_mtrs_max
#
#		var size = float(heigth)
#		if total_size == null:
#			total_size = 0
#		if total_size > 0:
#			size = total_size
#		var half_size = size /2.0
#
#		var radius_factor = 5
#
#		var txr_tl = Color()
#		var txr_tr = Color()
#		var txr_tr2 = Color()
#		var txr_bl = Color()
#		var txr_b2l = Color()
#		var txr_br = Color()
#		var txr_b2r = Color()
#		var txr_br2 = Color()
#		var txr_b2r2 = Color()
#
#		var arr_vtx = PoolVector3Array()
#		var arr_uvs = PoolVector2Array()
#		var arr_cols = PoolColorArray()
#
#		surf_tool.begin(Mesh.PRIMITIVE_TRIANGLES)
##		surf_tool.add_color(Color(1,1,1))
#		var ll = Vector3()
#		for y in rangeY:
#			if heigth-y <= 2:
#				y_limiter = 1
#			x_limiter = 0
#			# getting adjusted distances
#			# as it should change only on latitute change, 
#			# we adjust it here in the y loop
#			pxl_mtrs_t = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley) + float(y)/float(heigth), Zoom)
#			pxl_mtrs_b = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley) + float(y+1)/float(heigth), Zoom)
#			pxl_mtrs_b2 = smf.adjust_dist_from_tile_zoom(earth_circ, _tilex, float(_tiley) + float(y+2)/float(heigth), Zoom)
#			dist_proportion_t = pxl_mtrs_t / pxl_mtrs_max
#			dist_proportion_b = pxl_mtrs_b / pxl_mtrs_max
#			dist_proportion_b2 = pxl_mtrs_b2 / pxl_mtrs_max
#			for x in rangeX:
#				if width-x <= 2:
#					x_limiter = 1
#				arr_vtx.resize(0)
#				arr_uvs.resize(0)
#				arr_cols.resize(0)
#				set_squareHeights(hm_sbs_img, x, y, x_limiter, y_limiter, 0)
#
#				if color_vertices:
#					txr_tl = txtr_sbs_img.get_pixel(x, y)
#					txr_tr = txtr_sbs_img.get_pixel(x + 1, y)
#					txr_tr2 = txtr_sbs_img.get_pixel(x + 2 - x_limiter, y)
#					txr_bl = txtr_sbs_img.get_pixel(x, y + 1)
#					txr_b2l = txtr_sbs_img.get_pixel(x, y + 2 - y_limiter)
#					txr_br = txtr_sbs_img.get_pixel(x + 1, y + 1)
#					txr_b2r = txtr_sbs_img.get_pixel(x + 2 - x_limiter, y + 1)
#					txr_br2 = txtr_sbs_img.get_pixel(x + 1, y + 2 - y_limiter)
#					txr_b2r2 = txtr_sbs_img.get_pixel(x + 2 - x_limiter, y + 2 - y_limiter)
#
#				ll = smf.tile_on_sdddddddrtgyphere_q2((sq_heights[1][1]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+1)/float(width+1), float(y+1)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x+1)/float(width+1), float(y+1)/float(heigth+1)))
#
#				ll = smf.tile_on_sphere_q2((sq_heights[0][0]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x)/float(width+1), float(y)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x)/float(width+1), float(y)/float(heigth+1)))
#				if color_vertices:
#					arr_cols.append(txr_br)
#					arr_cols.append(txr_tl)
#
#				if true:
#					ll = smf.tile_on_sphere_q2((sq_heights[1][0]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+1)/float(width+1), float(y)/float(heigth+1), Zoom)
#					arr_vtx.append(ll)
#					arr_uvs.append(Vector2(float(x+1)/float(width+1), float(y)/float(heigth+1)))
#					if color_vertices:
#						arr_cols.append(txr_tr)
#
#				ll = smf.tile_on_sphere_q2((sq_heights[2][0]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+2)/float(width+1), float(y)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x+2)/float(width+1), float(y)/float(heigth+1)))
#				if color_vertices:
#					arr_cols.append(txr_tr2)
#
#				if true:
#					ll = smf.tile_on_sphere_q2((sq_heights[1][2]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+2)/float(width+1), float(y+1)/float(heigth+1), Zoom)
#					arr_vtx.append(ll)
#					arr_uvs.append(Vector2(float(x+2)/float(width+1), float(y+1)/float(heigth+1)))
#					if color_vertices:
#						arr_cols.append(txr_br2)
#
#				ll = smf.tile_on_sphere_q2((sq_heights[2][2]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+2)/float(width+1), float(y+2)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x+2)/float(width+1), float(y+2)/float(heigth+1)))
#				if color_vertices:
#					arr_cols.append(txr_b2r2)
#
#				if true:
#					ll = smf.tile_on_sphere_q2((sq_heights[1][2]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x+1)/float(width+1), float(y+2)/float(heigth+1), Zoom)
#					arr_vtx.append(ll)
#					arr_uvs.append(Vector2(float(x+1)/float(width+1), float(y+2)/float(heigth+1)))
#					if color_vertices:
#						arr_cols.append(txr_b2r)
#
#				ll = smf.tile_on_sphere_q2((sq_heights[0][2]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x)/float(width+1), float(y+2)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x)/float(width+1), float(y+2)/float(heigth+1)))
#				if color_vertices:
#					arr_cols.append(txr_b2l)
#
#				if true:
#					ll = smf.tile_on_sphere_q2((sq_heights[0][1]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x)/float(width+1), float(y+1)/float(heigth+1), Zoom)
#					arr_vtx.append(ll)
#					arr_uvs.append(Vector2(float(x)/float(width+1), float(y+1)/float(heigth+1)))
#					if color_vertices:
#						arr_cols.append(txr_bl)
#
#				ll = smf.tile_on_sphere_q2((sq_heights[0][0]*height_multiplier+smf.EARTH_RADIUS)/pxl_mtrs_max, float(_tilex), float(_tiley), float(x)/float(width+1), float(y)/float(heigth+1), Zoom)
#				arr_vtx.append(ll)
#				arr_uvs.append(Vector2(float(x)/float(width+1), float(y)/float(heigth+1)))
#				if color_vertices:
#					arr_cols.append(txr_tl)
#
#				surf_tool.add_rectangle(arr_vtx, arr_uvs, arr_cols, false, true)
#
#		hm_sbs_img.unlock()
#		txtr_sbs_img.unlock()
#		surf_tool.prepare_to_commit(SetMaterialTexture(txtr_sbs_img))
#		var mesh = surf_tool.commit()
#
#		var dist = float (size / width)
#
#		var endtt = float(OS.get_ticks_msec())
#		print("Mesh generation"
#		+ " Tile " + var2str(_subset) + "/" + var2str(_divideinto * _divideinto)
#		+ " X/Y: " + var2str(coords["x"]) + "/" + var2str(coords["y"])
#		+ " Size: " + var2str(size)
#		+ " Dist: " + var2str(dist)
#		+ " Meters/Pixel: " + var2str(pxl_mtrs_max)
#		+ " Min/Max Alt.: " + var2str(max_min_h.minh) + "/" + var2str(max_min_h.maxh)
#		+ " finished in %.2f seconds" % ((endtt - startt)/1000))
#		return mesh
		
func GetSideVertices(_mesh = ArrayMesh.new(), _side = Vector2()):
	var side_vertices = []
	var mdt = MeshDataTool.new()
	var error = mdt.create_from_surface(_mesh, 0)
	var mi = MeshInstance.new()
	mi.mesh = _mesh
	var mesh_aabb = mi.get_aabb()
	var plane_max = Vector3()
	var current_vertex = Vector3()
	for v in range(mdt.get_vertex_count()):
		current_vertex = mdt.get_vertex(v)
		if _side.x == -1:
			if current_vertex.x <= mesh_aabb.position.x:
				side_vertices.append([v, current_vertex])
				print([v, current_vertex])
		if _side.x == 1:
			if current_vertex.x >= mesh_aabb.end.x:
				side_vertices.append([v, current_vertex])
				print([v, current_vertex])
		if _side.y == -1:
			if current_vertex.z <= mesh_aabb.position.z:
				side_vertices.append([v, current_vertex])
				print([v, current_vertex])
		if _side.y == 1:
			if current_vertex.z >= mesh_aabb.end.z:
				side_vertices.append([v, current_vertex])
				print([v, current_vertex])
	return side_vertices

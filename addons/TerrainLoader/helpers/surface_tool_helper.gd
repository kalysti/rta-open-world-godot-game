tool
extends SurfaceTool

var arr_vtx = PoolVector3Array()
var arr_uvs = PoolVector2Array()
var arr_cols = PoolColorArray()
var vertex_distance = 1.0
var height_scale = 1.0
var approximation = 0.0
var divide_by = 8
var heights_squares = null
var tile_mesh = ArrayMesh.new()

func _init():
	pass
	
func add_fan_element(_vtx, _uv, _col = null):
	arr_vtx.append(_vtx)
	arr_uvs.append(_uv)
	if(_col != null):
		arr_cols.append(_col)
		
func add_single_square(_heights, sq_y, sq_x, _mt_pxl, _offset = 0.0, to_mesh = false):
	var sq_heights = Array()
	var y_heights_start = (sq_y) * divide_by
	var x_heights_start = (sq_x) * divide_by
	var y_heights_index = 0
	var x_height_index = 0
	var h_y_half = 0.0
	var h_x_half = 0.0
	sq_heights.resize(_heights.size()/divide_by)
	#parsing each height belonging to the square
	for h_y in range(0,_heights.size()/divide_by,2):
		h_y_half = h_y/2
		y_heights_index = y_heights_start + h_y
		sq_heights[h_y_half] = Array()
		sq_heights[h_y_half].resize(_heights[h_y].size()/divide_by)
		for h_x in range(0,_heights[h_y].size()/divide_by,2):
			h_x_half = h_x/2
			x_height_index = x_heights_start + h_x
#			print([y_heights_index, x_height_index])
			sq_heights[h_y_half][h_x_half] = Array()
			#creating Addithe triangle fan
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 1)*_mt_pxl, _heights[y_heights_index + 1][x_height_index + 1] - _offset, (y_heights_index + 1)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 0)*_mt_pxl, _heights[y_heights_index + 0][x_height_index + 0] - _offset, (y_heights_index + 0)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 1)*_mt_pxl, _heights[y_heights_index + 0][x_height_index + 1] - _offset, (y_heights_index + 0)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 2)*_mt_pxl, _heights[y_heights_index + 0][x_height_index + 2] - _offset, (y_heights_index + 0)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 2)*_mt_pxl, _heights[y_heights_index + 1][x_height_index + 2] - _offset, (y_heights_index + 1)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 2)*_mt_pxl, _heights[y_heights_index + 2][x_height_index + 2] - _offset, (y_heights_index + 2)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 1)*_mt_pxl, _heights[y_heights_index + 2][x_height_index + 1] - _offset, (y_heights_index + 2)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 0)*_mt_pxl, _heights[y_heights_index + 2][x_height_index + 0] - _offset, (y_heights_index + 2)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 0)*_mt_pxl, _heights[y_heights_index + 1][x_height_index + 0] - _offset, (y_heights_index + 1)*_mt_pxl))
			sq_heights[h_y_half][h_x_half].append(Vector3((x_height_index + 0)*_mt_pxl, _heights[y_heights_index + 0][x_height_index + 0] - _offset, (y_heights_index + 0)*_mt_pxl))
			if to_mesh:
				add_triangle_fan(sq_heights[h_y_half][h_x_half])
	return sq_heights

#	This function should convert the single array into groups of arrays 
#	each group is an 8th in width and height
#	heights should also be a square with a 2^x value, and a minimum size of 8x8
#	Ie. 8, 16, 32, 64, etc.
func _heights_to_squares_array(_heights = Array(), _divide_by = 8, _mtpxl = 1.0, _offset = 0.0, to_mesh = false):
	var startt = float(OS.get_ticks_msec())
	tile_mesh = ArrayMesh.new()
	clear()
	divide_by = _divide_by
	vertex_distance = _mtpxl
	heights_squares = Array()
	heights_squares.resize(_divide_by)
	if to_mesh:
		tile_mesh = ArrayMesh.new()
		begin(Mesh.PRIMITIVE_TRIANGLES)
		add_smooth_group(true)
	#parsing each of the _divide_by squares
	for sq_y in range(_divide_by):
		heights_squares[sq_y] = Array()
		heights_squares[sq_y].resize(_divide_by)
		for sq_x in range(_divide_by):
			#append the fan to the squares array
			heights_squares[sq_y][sq_x] = add_single_square(_heights, sq_y, sq_x, _mtpxl, _offset, to_mesh)
			if to_mesh:
				var mesh_array = Array()
				mesh_array.resize(ArrayMesh.ARRAY_MAX)
				mesh_array[ArrayMesh.ARRAY_VERTEX] = heights_squares[sq_y][sq_x]
				tile_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES,  mesh_array)
				var position = Transform()
				position.origin = Vector3(sq_x*_mtpxl, 0, sq_y*_mtpxl)
				append_from(tile_mesh, tile_mesh.get_surface_count()-1, position)
	if to_mesh:
		generate_normals()
		tile_mesh = commit(tile_mesh)
	var endtt = float(OS.get_ticks_msec())
	print("Squares of heights generated in %.2f seconds" % ((endtt - startt)/1000))
	return heights_squares

func reset_fan():
	arr_vtx.resize(0)
	arr_uvs.resize(0)
	arr_cols.resize(0)
	
func commit_fan():
	add_triangle_fan(arr_vtx, arr_uvs, arr_cols)
	reset_fan()
	
func prepare_to_commit( _material, _index = true):
	if(_index):
		index()
	generate_normals()
	set_material(_material)

func add_rectangle(_vertices = PoolVector3Array(), _uvs = PoolVector3Array(), _colors = PoolColorArray()):
	add_triangle_fan(_vertices, _uvs, _colors)


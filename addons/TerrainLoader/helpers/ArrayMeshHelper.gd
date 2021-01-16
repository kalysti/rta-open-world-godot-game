tool
extends ArrayMesh
export(Array) var heights setget _set_heights

# The number of times a tile is subdivided
# it should always be in form of 2^x
# i.e. 2, 4, 8, 16, 32, etc.
export(int) var divide_by = 8

# the size of a single square
export(int) var heights_y_size = 0
export(int) var heights_x_size = 0

func _set_heights(_newval):
	heights = _newval
	heights_y_size = heights.size()/divide_by
	if heights.size() > 0:
		heights_x_size = heights[0].size()/divide_by
	
func _get_square_with_size(_sqy, _sqx, _divide_by):
	var vtxs = null
	if get_surface_count() > (_sqy * _divide_by + _sqx) && (_sqy * _divide_by + _sqx) >= 0:
		vtxs = surface_get_arrays(_sqy * _divide_by + _sqx)[ArrayMesh.ARRAY_VERTEX]
	var y_sq_not_first = 0
	var x_sq_not_first = 0
	if _sqy > 0:
		y_sq_not_first = 1
	if _sqx > 0:
		x_sq_not_first = 1
		
	return {vertices = vtxs, y = heights_y_size + y_sq_not_first, x = heights_x_size + x_sq_not_first}

func get_vertex_from_yx_coordinates(_sqsz, _y, _x):
	return _sqsz.vertices[_y * _sqsz.x + _x]

#	It adds a single square to the tile
#	_heights	is an array of arrays containing the heights of the whole tile
#				heights are represented in meters
#	_sq_y		the y coordinate of the single square
#	_sq_x		the x coordinate of the single square
#	_mt_pxl		Meters per pixel
#	_divide_by	determines the size of the single square (a mesh surface)
#				_divide_by = 4 creates a 4x4 set of surfaces
#	_offset		the minimum height of the whole set of surfaces,
#				it simply subtracts every height
func add_single_square(_heights, sq_y, sq_x, _mt_pxl, _divide_by, _offset = 0.0):
	var sq_heights = PoolVector3Array()
	var sq_normals = PoolVector3Array()
	var sq_uvs = PoolVector3Array()
	var sq_indices = PoolIntArray()
	# if not first square we have to iterate from the previous pixels
	# so that each square is correctly joined		
	var sq_heights_y_size = _get_square_with_size(sq_y, sq_x, _divide_by).y
	var sq_heights_x_size = _get_square_with_size(sq_y, sq_x, _divide_by).x
	# I need to pre store square sizes of adjacent surfaces
#	var sq_11 = _get_square_with_size(sq_y-1, sq_x-1, _divide_by)
	var sq_01 = _get_square_with_size(sq_y, sq_x-1, _divide_by)
	var sq_10 = _get_square_with_size(sq_y-1, sq_x, _divide_by)
	# half size is used to center the geometry
	var half_y_size = heights_y_size*_divide_by*_mt_pxl/2
	var half_x_size = heights_x_size*_divide_by*_mt_pxl/2
	var y_heights_start = sq_y * heights_y_size - (sq_heights_y_size - heights_y_size)
	var x_heights_start = sq_x * heights_x_size - (sq_heights_x_size - heights_x_size)
	var y_heights_index = 0
	var x_height_index = 0
	var index = 0
	sq_heights.resize((sq_heights_y_size)*(sq_heights_x_size))
	sq_normals.resize((sq_heights_y_size)*(sq_heights_x_size))
	sq_uvs.resize((sq_heights_y_size)*(sq_heights_x_size))
	#parsing each height belonging to the square
	for h_y in range(0,sq_heights_y_size):
		y_heights_index = y_heights_start + h_y
		for h_x in range(0,sq_heights_x_size):
			x_height_index = x_heights_start + h_x
				
			#creating the vertex belonging to the MeshArray surface (the mesh is divided into _divide_by * _divide_by surfaces)
			sq_heights[index] = Vector3((x_height_index)*_mt_pxl-half_x_size, _heights[y_heights_index][x_height_index] - _offset, (y_heights_index)*_mt_pxl-half_y_size)
		
			#Create the UVS, would be great if it could be placed only on corners of single surface
			sq_uvs[index] = Vector3(float(x_height_index)/float(_heights[sq_y].size()), float(y_heights_index)/float(_heights.size()), 0)

			if h_y > 0 && h_x > 0:
				# Generate the vertices index
				# drawing 2 triangles as follows
				# 1-2			  2
				# |/ 	then	 /|
				# 3  	    	1-3
				sq_indices.append((h_y-1) * (sq_heights_x_size) + (h_x-1))
				sq_indices.append((h_y-1) * (sq_heights_x_size) + (h_x))
				sq_indices.append((h_y) * (sq_heights_x_size) + (h_x-1))
				sq_indices.append((h_y) * (sq_heights_x_size) + (h_x-1))
				sq_indices.append((h_y-1) * (sq_heights_x_size) + (h_x))
				sq_indices.append((h_y) * (sq_heights_x_size) + (h_x))
			
				sq_normals[index] = Plane(sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x-1)],
				sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x)],
				sq_heights[(h_y) * (sq_heights_x_size) + (h_x-1)]).normal
				
				# here we manage normals on top and left side of the tile
				# so that it merges nicely with the previous tile(s)
				var vtx1 = sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x-1)]
				var vtx2 = sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x-1)]
				var vtx3 = sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x-1)]
				if h_y == 1 && h_x == 1:
					vtx1 = sq_heights[0]
					vtx2 = sq_heights[1]
					vtx3 = sq_heights[sq_heights_x_size]
					if sq_y > 0:
						vtx1 = get_vertex_from_yx_coordinates(sq_10, sq_10.y -2, h_x-1)
						vtx2 = get_vertex_from_yx_coordinates(sq_10, sq_10.y -2, h_x)
					sq_normals[0] = Plane(vtx1, vtx2, vtx3).normal
					
				elif h_y == 1 && h_x > 1:
					vtx2 = sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x)]
					vtx3 = sq_heights[(h_y-1) * (sq_heights_x_size) + (h_x-1)]
					if sq_y > 0:
						vtx1 = get_vertex_from_yx_coordinates(sq_10, sq_10.y -2, h_x-1)
						vtx2 = get_vertex_from_yx_coordinates(sq_10, sq_10.y -2, h_x)
					sq_normals[h_x] = Plane(vtx1, vtx2, vtx3).normal
					
				elif h_y > 1 && h_x == 1:
					if sq_x > 0:
						vtx1 = get_vertex_from_yx_coordinates(sq_01, h_y-2, sq_01.x -2)
						vtx2 = get_vertex_from_yx_coordinates(sq_01, h_y-2, sq_01.x -1)
					vtx3 = sq_heights[(h_y-1) * (sq_heights_x_size)]
					sq_normals[(h_y-1) * (sq_heights_x_size)] = Plane(vtx1, vtx2, vtx3).normal
				
			index += 1
	return {heights=sq_heights, normals=sq_normals, indices=sq_indices, uv=sq_uvs}

#	This function should convert the single array into groups of arrays 
#	each group is an 8th in width and height
#	heights should also be a square with a 2^x value, and a minimum size of 8x8
#	Ie. 8, 16, 32, 64, etc.
func heights_to_squares_array(_heights = Array(), _mat = Material.new(), _divide_by = 8, _mtpxl = 1.0, _offset = 0.0):
	var startt = float(OS.get_ticks_msec())
	# Clean up the previous mesh
	while get_surface_count() > 0:
		surface_remove(get_surface_count()-1)
	# Passed to property so that is globally visible
	# Always declare first the divide_by property
	# as it is used to calculate the square size
	# use self. as otherwise setget won't work
	self.divide_by = _divide_by
	self.heights = _heights
	var heights_squares = Array()
	heights_squares.resize(_divide_by)
	#parsing each of the _divide_by squares
	for sq_y in range(_divide_by):
		heights_squares[sq_y] = Array()
		heights_squares[sq_y].resize(_divide_by)
		for sq_x in range(_divide_by):
			#append the fan to the squares array
			heights_squares[sq_y][sq_x] = add_single_square(_heights, sq_y, sq_x, _mtpxl, _divide_by, _offset)
			var mesh_array = Array()
			mesh_array.resize(ArrayMesh.ARRAY_MAX)
			mesh_array[ArrayMesh.ARRAY_VERTEX] = heights_squares[sq_y][sq_x].heights
			mesh_array[ArrayMesh.ARRAY_NORMAL] = heights_squares[sq_y][sq_x].normals
			mesh_array[ArrayMesh.ARRAY_INDEX] = heights_squares[sq_y][sq_x].indices
			mesh_array[ArrayMesh.ARRAY_TEX_UV] = heights_squares[sq_y][sq_x].uv
			add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, mesh_array)
			var surfidx = get_surface_count()-1
			var surfname = var2str(sq_y)+"|"+var2str(sq_x)
			surface_set_material(surfidx, _mat)
			surface_set_name(surfidx, surfname)
	var endtt = float(OS.get_ticks_msec())
	print("Squares of heights generated in %.2f seconds" % ((endtt - startt)/1000))
	return heights_squares

#	this is not working yet
func _array_to_normalmap(_normals = PoolVector3Array(), _width = 256):
	var nmap = Image.new()
	nmap.create(_width, _normals.size()/_width, false, Image.FORMAT_RG8)
	nmap.lock()
	for hy in range(0,_normals.size()/_width):
		for hx in range(0,_width):
			nmap.set_pixel(hx, hy, Color(_normals[hy*_width+hx].x,_normals[hy*_width+hx].y,0.0))
	nmap.unlock()
	return nmap
	
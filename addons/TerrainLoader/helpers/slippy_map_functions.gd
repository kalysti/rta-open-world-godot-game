# Licensed under the MIT License.
# Copyright (c) 2018 Leonardo (Toshiwo) Araki

extends Object
#	The following functions are the GDScript conversion of:
#	https://help.openstreetmap.org/questions/747/given-a-latlon-how-do-i-find-the-precise-position-on-the-tile
# ...or maybe they where, I forgot already how much have I changed here.

const EARTH_RADIUS = 6371000
const MAX_LAT = 85.0511

static func radius_to_circ(_rad):
	return float(_rad) * 2 * PI

#	Metres per pixel math
#	---------------------
#	The distance represented by one pixel (S) is given by
#
#	    _circ is the (equatorial) circumference of the Earth
#	    _zoom is the zoom level
#	    _lat is the latitude of where you're interested in the scale.
static func adjust_dist_from_latzoom(_circ, _lat, _zoom):
	var s = abs(float(_circ) * cos(float(deg2rad(_lat)))/pow(2, _zoom+8))
	return s
	
static func adjust_dist_from_tile_zoom(_circ = radius_to_circ(EARTH_RADIUS), _tilex = 0.0, _tiley = 0.0, _zoom = 1):
	var tll = tile_to_latlon(_tilex, _tiley, _zoom)
	return adjust_dist_from_latzoom(_circ, tll.lat, _zoom)
	
static func get_height_from_color(col):
	return -10000 + ((col.r8 * 256 * 256 + col.g8 * 256 + col.b8) * 0.1)

static func calc_max_lon(_lat):
	return _lat * PI / 180
	
static func latlon_to_tile_pxl(lat_deg, lon_deg, zoom):
	if(abs(lat_deg) > MAX_LAT):
		lat_deg = sign(lat_deg) * MAX_LAT
		
	var lat_rad = deg2rad(lat_deg)
	var n = pow(2.0, zoom)
#	var xtile_f = ((lon_deg + 180.0) / 360.0 * n)
	var xtile_f = (lon_deg + 180.0) / 360.0 * (1 << zoom)
#	var ytile_f = ((1.0 - log(tan(lat_rad) + (1.0 / cos(lat_rad))) / PI) / 2.0 * n)
	var ytile_f = (1.0 - log(tan(lat_deg * PI / 180.0) + 1.0 / cos(lat_rad)) / PI) / 2.0 * (1 << zoom)
	var tilex = int(xtile_f)
	var tiley = int(ytile_f)
	var pxlx = int(256 * (xtile_f - tilex))
	var pxly = int(256 * (ytile_f - tiley))
	return {tilex=tilex, tiley=tiley,
			pxlx=pxlx, pxly=pxly}

static func tile_to_latlon(_tilex, _tiley, _zoom):
	var n = float(pow(2.0, _zoom))
	var _lon = _tilex * 360.0 / n - 180.0
	var _lat = rad2deg(atan(sinh(PI * (1.0 - 2.0 * float(_tiley) / n))))
	return {lat=_lat, lon=_lon}

static func tile_on_sphere(_radius, _tilex, _tiley, _zoom):
	var n = float(pow(2.0, _zoom))
	var sp_pt = Transform(Vector3(1,0,0), Vector3(0,1,0), Vector3(0,0,1), Vector3(0,float(_radius),0))
	sp_pt = sp_pt.rotated(Vector3(1,0,0), (PI/2-atan(sinh(PI * (1.0 - 2.0 * (float(_tiley) / n))))))
	sp_pt = sp_pt.rotated(Vector3(0,1,0), deg2rad(_tilex * (360.0 / n) - 180.0))
	return sp_pt

# It returns a transform from an origin point
# use the .origin to get the position relative to the origin point
static func lat_lon_on_sphere(_radius, _lat, _lon):
	var sp_pt = Transform(Vector3(1,0,0), Vector3(0,1,0), Vector3(0,0,1), Vector3(0,float(_radius),0))
	sp_pt = sp_pt.rotated(Vector3(1,0,0), deg2rad(90.0-_lat))
	sp_pt = sp_pt.rotated(Vector3(0,1,0), deg2rad(_lon))
	return sp_pt

# It returns the corresponding rotation
# It uses quat to to a rotation
# More study on quat needed...
static func lat_lon_on_sphere_q(_radius, _lat, _lon):
	var latr = deg2rad(-_lat)
	var lonr = deg2rad(180-_lon)
	var qr = Quat(cos(latr/2)*cos(lonr/2), -sin(latr/2)*sin(lonr/2), cos(latr/2)*sin(lonr/2), sin(latr/2)*cos(lonr/2))
	var br = Basis(qr)
	br.z = br.z * _radius
	return br.z
	
# It returns the corresponding rotation
# It uses the Vector3.rotated method 
static func lat_lon_on_sphere_v(_radius, _lat, _lon):
	var latr = deg2rad(_lat)
	var lonr = deg2rad(180-_lon)
	var br = Vector3(_radius, 0, 0)
	br = br.rotated(Vector3(0,0,1), latr)
	br = br.rotated(Vector3(0,1,0), lonr)
	return br

# It returns a transform from an origin point
# use the .origin to get the position relative to the origin point
#static func tile_on_sphere2(_radius, _tilex, _tiley, _zoom):
#	var n = pow(2.0, _zoom)
#	var latr = -(PI/2-atan(sinh(PI * (1.0 - 2.0 * (float(_tiley/n))))))
#	var lonr = deg2rad(_tilex * (360.0 / n) - 180.0)
#	var qr = Quat(cos(latr/2)*cos(lonr/2), -sin(latr/2)*sin(lonr/2), cos(latr/2)*sin(lonr/2), sin(latr/2)*cos(lonr/2))
#	var br = Basis(qr)
#	br.z = br.z * _radius
#	return br.z

static func tile_on_sphere_v(_radius, _tilex, _tiley, _xam, _yam, _zoom, _verbose = false):
	var n = pow(2.0, _zoom)
	var latr = (PI/2-atan(sinh(PI * (1.0 - 2.0 * (float(_tiley/n))))))
	var lonr = deg2rad(_tilex * (360.0 / n) - 180.0)
	var br = Vector3(_radius, 0, 0)
	br = br.rotated(Vector3(0,0,1), latr)
	br = br.rotated(Vector3(0,-1,0), lonr)
	return br
	
static func tile_on_sphere_q(_radius, _tilex, _tiley, _zoom, _verbose = false):
	var n = pow(2.0, _zoom)
	var latr = atan(sinh(PI * (1.0 - 2.0 * float(_tiley) / n)))
	var lonr = deg2rad(_tilex * (360.0 / n) - 180.0)
	var latmr = atan(sinh(PI * (1.0 - 2.0 * floor(_tiley) / n)))
	var lonmr = deg2rad(floor(_tilex) * (360.0 / n) - 180.0)
	var latnr = atan(sinh(PI * (1.0 - 2.0 * floor(_tiley+1) / n)))
	var lonnr = deg2rad(floor(_tilex+1) * (360.0 / n) - 180.0)
	var proportion = 1
	if(abs(latr) < abs(latnr)):
		proportion = adjust_dist_from_latzoom(_radius*2*PI, latnr, _zoom)/adjust_dist_from_latzoom(_radius*2*PI, latr, _zoom)
	else:
		proportion = adjust_dist_from_latzoom(_radius*2*PI, latr, _zoom)/adjust_dist_from_latzoom(_radius*2*PI, latnr, _zoom)
			
	var latam = _tiley - floor(_tiley)
	var lonam = _tilex - floor(_tilex)
#	var latam = 1.0-(abs(latnr)-abs(latr))/(abs(latnr)-abs(latmr)) * proportion
#	var lonam = 1.0-(abs(lonnr)-abs(lonr))/(abs(lonnr)-abs(lonmr))
	var orig = Vector3(0, 0, _radius)
	var axis = Vector3(-1,0,0)
	var q1 = Quat(axis,latmr)
	var q2 = Quat(axis, latnr)
	var qi = q1.slerp(q2, latam)
	var sli = qi.xform(orig)
	axis = Vector3(0,1,0)
	q1 = Quat(axis, lonmr)
	q2 = Quat(axis, lonnr)
	qi = q1.slerp(q2, lonam)
	sli = qi.xform(sli)
	if(_verbose):
		print("Item pos: %s - Amount: %f/%f, MIN lat/lon: %f/%f, MAX lat/lon: %f/%f" % [var2str(sli), latam, lonam, latr, lonr, latnr, lonnr])
	return sli
	
static func tile_on_sphere_q2(_radius, _tilex, _tiley, _xam, _yam, _zoom, _verbose = false):
	var n = pow(2.0, _zoom)
	var latmr = atan(sinh(PI * (1.0 - 2.0 * floor(_tiley) / n)))
	var lonmr = deg2rad(floor(_tilex) * (360.0 / n) - 180.0)
	var latnr = atan(sinh(PI * (1.0 - 2.0 * floor(_tiley+1) / n)))
	var lonnr = deg2rad(floor(_tilex+1) * (360.0 / n) - 180.0)
	var orig = Vector3(0, 0, _radius)
	var axis = Vector3(-1,0,0)
	var q1 = Quat(axis,latmr)
	var q2 = Quat(axis, latnr)
	var qi = q1.slerp(q2, _yam)
	var sli = qi.xform(orig)
	axis = Vector3(0,-1,0)
	q1 = Quat(axis, lonmr)
	q2 = Quat(axis, lonnr)
	qi = q1.slerp(q2, _xam)
	sli = qi.xform(sli)
	if(_verbose):
		print("Item pos: %s - Amount: %f/%f, MIN lat/lon: %f/%f, MAX lat/lon: %f/%f" % [var2str(sli), _xam, _yam, latmr, lonmr, latnr, lonnr])
	return sli
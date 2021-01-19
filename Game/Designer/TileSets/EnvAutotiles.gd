tool
extends TileSet



var binds = {0 : [0], 1 : [0, 1]}

func _is_tile_bound(drawn_id, neighbor_id):
	if binds.keys().count(drawn_id) > 0:
		if binds[drawn_id].count(neighbor_id) > 0: 
			return true
	
	return false
	

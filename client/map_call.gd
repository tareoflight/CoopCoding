extends Node3D


var map_node_scene = preload("res://MapNodes/MapNode.tscn")

#db stuff
var host = "127.0.0.1"
var port = 25569 #add logic later
var client = StreamPeerTCP.new()

func	connect_to_server():
	var e = client.connect_to_host(host,port)
	if e != OK:
		push_error("NO db to connect too")
		return false
	return true

func send_sql_query(query: String) -> String:
	if not connect_to_server():
		return ""
	client.put_data(query.to_utf8_buffer())
	var r = ""
	while client.get_available_bytes() >0:
		r += client.get_string(client.get_available_bytes())
	client.disconnect_from_host()
	return r

func _ready():
	spawn_map_nodes()

func spawn_map_nodes():
	var result = send_sql_query("SELECT x, y, z FROM map;")
	
	if result.is_empty():
		push_error("No data returned from SQL query.")
		return ""
	
	var locations = result.split("\n")  # In case multiple rows are returned
	
	for loc in locations:
		var coords = loc.split(",")
		if coords.size() == 3:
			var x = coords[0].to_float()
			var y = coords[1].to_float()
			var z = coords[2].to_float()
			
			var map_node = map_node_scene.instantiate()
			map_node.position = Vector3(x, y, z)  # Adjust according to your node type
			map_node.set_contents("air",0.5)
			map_node.set_contents("ground",0.5)
			
			add_child(map_node)  # Add to the current scene
			
			print("Spawned MapNode at: ", Vector3(x, y, z))

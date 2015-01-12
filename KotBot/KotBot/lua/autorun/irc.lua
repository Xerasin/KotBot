IRC.Connect("176.9.103.226")

hook.Add("IRC.Registered", "IRC.Registered", function(client)
	client.Channels:Join("#xerasinhideout")
end)
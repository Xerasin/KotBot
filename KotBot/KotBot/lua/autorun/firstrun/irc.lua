IRC.Connect("66.151.244.66")

hook.Add("IRC.Registered", "IRC.Registered", function(client)
	client.Channels:Join("#test")
end)
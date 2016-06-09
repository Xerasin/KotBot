ircs = {
	--[=[["66.151.244.66"] = {
		NickName = "KotBot",
		RealName = "Katie Bot",
		UserName = "KotBot",
		Password = "asword",
		Email = "test@gmail.om",
		channels =
		{
			"#test",
			"#gcinema",
		}
	}]=]

}
clients = {}
for k,v in pairs(ircs) do
	if not IRC.IsConnected(k) then
		local info = IRC.GetBlankInfo()
		info.NickName = v.NickName
		info.RealName = v.RealName
		info.UserName = v.UserName
		info.Password = v.Password
		v.info = info
		local client = IRC.Connect(k, info)
		v.client = client
		clients[client] = k 
	end
end

hook.Add("IRC.Disconnected", "IRC.Connected", function(client)
	if client then
		local new_client = IRC.Reconnect(client)
		if clients[client] then
			local IP = clients[client]
			local tab = ircs[IP]
			tab.client = new_client
			clients[client] = nil
			clients[new_client] = IP
		end
	end
end)

hook.Add("IRC.Registered", "IRC.Registered", function(client)
	if clients[client] then
		local IP = clients[client]
		local tab = ircs[IP]
		for k,v in pairs(tab.channels) do
			client.Channels:Join(v)
		end
		--if not client.IsRegistered then
			IRC.Register(client, tab.Password, tab.Email)
		--end
	end
end)
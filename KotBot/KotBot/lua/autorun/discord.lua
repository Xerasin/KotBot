local DiscordData = {}
local function LoadDiscordData()
	local t = io.open("data/discorddata.json", "r")
	if t then
		DiscordData = json.parse(t:read("*a"))
		io.close(t)
	end
end

local function AddChannel(t)
	DiscordData[t] = true
	local t = io.open("data/discorddata.json", "w")
	if t then
		t:write(json.tostring(DiscordData))
		io.close(t)
	end
end

local function RemoveChannel(t)
	DiscordData[t] = nil
	local t = io.open("data/discorddata.json", "w")
	if t then
		t:write(json.tostring(DiscordData))
		io.close(t)
	end
end
hook.Add("ShouldCallMessage", "DiscordIgnore", function(message)
	local loc = message:GetClient():GetLocationString()
	if loc:sub(1, 12) == "|Discord|PM|" then
		return false
	end
	
	local text = message:GetMessage()
	if text == "!enablekotbot" then
		AddChannel(loc)
		message:Reply("Yay I'm enabling myself in this channel! *squees*")
	elseif text == "!disablekotbot" then
		RemoveChannel(loc)
		message:Reply("Okay I disabled myself :( *cries*")
	end
	if not DiscordData[loc] then
		return true
	else
		return false
	end
end)
LoadDiscordData()
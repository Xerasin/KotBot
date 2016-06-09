local Saves
local Adventures = {}

local function LoadAdventure(adv)
	local t = io.open("data/adventure/"..adv..".json", "r")
	if t then
		Adventures[adv] = json.parse(t:read("*a"))
		io.close(t)
	end
end

local function LoadSaves()
	local t = io.open("data/adventure/saves.json", "r")
	if t then
		Saves = json.parse(t:read("*a"))
		io.close(t)
	end
end
--[==[Commands.Register("!adventurestart", function(message, words, text) 
	if #words > 1 then
		if words[1] == "load" then
			if not Saves then
				LoadSaves()
			end
			if not Adventures[words[2]] then
				LoadAdventure(words[2])
			end
		end
	end
end, "Adventure Start", "")]==]
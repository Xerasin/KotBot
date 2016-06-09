Commands.Register("!urbandic", function(message, words, text) 
	if #words == 0 then return end
	Webclient.FetchAsync("http://api.urbandictionary.com/v0/define?term=" .. Webclient.Encode(text), function(str)
		local test = json.parse(str)
		local defs = test["list"]
		local word = defs[math.random(1, #defs)]
		if word then
			message:Reply(word["word"] .. ": " .. word["definition"])
		else
			message:Reply("No definition for " .. text .. " found!")
		end
	end)
end, "Get an urbandictionary def", "")
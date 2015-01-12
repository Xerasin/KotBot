local greetings = {
	["hi"] = true,
	["hello"] = true,
	["hey"] = true,
	["sup"] = true,
	["greetings"] = true,
}
hook.Add("MessageRecieved", "MessageRecieved.hi", function(message)
	local name = message:GetSender():GetName()
	local text = message:GetMessage():lower()
	local words = words.Split(text)
	for I=1,#words do
		local word = words[I]
		local nextword = words[I+1]
		if word and nextword then
			if greetings[word:match("^(%w+)")] then
				if nextword:match("^kot") then
					message:Reply("Hey, " .. name)
				end
			elseif greetings[nextword:match("^(%w+)")] then
				if word:match("^kot") then
					message:Reply("Hey, " .. name)
				end
			end
		end
	end
end)
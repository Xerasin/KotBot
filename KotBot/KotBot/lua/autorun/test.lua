hook.Add("MessageRecieved", "MessageRecieved.e", function(message)
	local user = message:GetClient():GetName()
	local name = message:GetSender():GetName()
	if name == user then return end
	local text = message:GetMessage()
	local words = words.Split(text)
	if text:sub(1, 1) == "@" then
		local in_quote = false
		for I=3, #text do
			if text[2] ~= [["]] then break end
			if text[I] == [["]] and text[I - 1] ~= "\\" then in_quote = I - 1 break end
		end
		local firstword = text:match("^([^ ]+)")
		if firstword or in_quote then
			local firstword_end = #firstword + 2
			if in_quote then
				firstword = "@" .. text:sub(3, in_quote)
				firstword_end = #firstword + 4
			end
			if not firstword then return end
			
			
			firstword = firstword:lower():gsub([[\"]], [["]])
			local rest = text:sub(firstword_end)
			firstword = firstword:sub(2)
			if firstword == "" then return end
			
			local target_ply = message:GetClient():FindUserByName(firstword)
			if target_ply then
				target_ply:Message(rest)
			end
		end
	end
end)
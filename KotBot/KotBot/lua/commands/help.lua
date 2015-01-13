Commands.Register("!help", function(message, words) 
	if #words > 0 then
		local tbl = Commands.Get(words[1])
		if tbl then
			message:GetSender():Message(words[1] .. ": " .. tbl.help)
		end
	else
		local str = ""
		for k,v in pairs(Commands.Commands) do
			str = str .. k .. ","
		end
		message:GetSender():Message(str:sub(1, -2))
	end
end, "Get help on a command", "o")
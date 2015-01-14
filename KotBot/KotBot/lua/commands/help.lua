Commands.Register("!help", function(message, words) 
	if #words > 0 then
		local tbl = Commands.Get(words[1])
		if tbl then
			message:Reply(words[1] .. ": " .. tbl.help)
		end
	else
		
		local tbl = {}
		for k,v in pairs(Commands.Commands) do
			if Commands.HasPermissions(message:GetSender(), v.perm) then
				table.insert(tbl, k)
			end
		end
		table.sort(tbl)
		
		local str = ""
		for k,v in pairs(tbl) do
			str = str .. v .. ", "
		end
		message:Reply(message:GetSender():GetName() .. ", I sent you the commands in a PM *giggle* <3~")
		message:GetSender():Message(str:sub(1, -3))
	end
end, "Get help on a command", "")
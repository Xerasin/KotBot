Commands = {}
Commands.Commands = {}
function Commands.Register(cmd, func, help, perm)
	local tbl = {
		func = func,
		help = help,
		perm = perm
	}
	Commands.Commands[cmd] = tbl
end

hook.Add("MessageRecieved", "MessageRecieved.commands", function(message)
	local name = message:GetSender():GetName()
	local text = message:GetMessage()
	local words = words.Split(text)
	if #words > 0 then
		local tbl = Commands.Commands[words[1]]
		if tbl then
			local text2 = text:sub(#words[1] + 1)
			table.remove(words, 1)
			pcall(tbl.func, message, words, text2)
		end
	end
end)
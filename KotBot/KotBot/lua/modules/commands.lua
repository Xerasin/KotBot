Commands = {}
Commands.Commands = {}
Commands.Aliases = {}
function Commands.Register(cmd, func, help, perm)
	local tbl = {
		func = func,
		help = help,
		perm = perm
	}
	Commands.Commands[cmd] = tbl
end

function Commands.Alias(cmd, alias)
	Commands.Aliases[alias] = cmd
end

function Commands.Get(cmd)
	if Commands.Aliases[cmd] then
		cmd = Commands.Aliases[cmd]
	end
	if Commands.Commands[cmd] then
		return Commands.Commands[cmd]
	end
end

hook.Add("MessageRecieved", "MessageRecieved.commands", function(message)
	local name = message:GetSender():GetName()
	local text = message:GetMessage()
	local words = words.Split(text)
	if #words > 0 then
		local tbl = Commands.Get(words[1])
		if tbl then
			local text2 = text:sub(#words[1] + 1)
			table.remove(words, 1)
			local status, error = pcall(tbl.func, message, words, text2)
			if not status then
				print(error)
			end
		end
	end
end)
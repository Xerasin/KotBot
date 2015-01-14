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
Commands.Permissions = {}
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
		if tbl and Commands.HasPermissions(message:GetSender(), tbl.perm) then
			local text2 = text:sub(#words[1] + 1)
			table.remove(words, 1)
			local status, error = pcall(tbl.func, message, words, text2)
			if not status then
				print(error)
			end
		end
	end
end)

function Commands.RefreshPermissions()
	Commands.Permissions = {}
	local t = io.open("permissions.inf", "r")
	if t then
		for line in t:lines() do
			local name, perms = line:match("^(.+);(.+)$")
			Commands.Permissions[name] = {}
			for I=1, #perms do
				Commands.Permissions[name][perms:sub(I,I)] = true
			end
		end
		io.close(t)
	end
end
Commands.RefreshPermissions()
function Commands.SavePermissions()
	local t = io.open("permissions.inf", "w")
	if t then
		for name, perms in pairs(Commands.Permissions) do
			local perms2 = ""
			for k,v in pairs(perms) do
				perms2 = perms2 .. k
			end
			t:write(name .. ";" .. perms2 .. "\n")
		end
		io.close(t)
	end
end

function Commands.AddPermission(user, id)
	local user = user:GetUserID()
	Commands.Permissions[user] = Commands.Permissions[user] or {}
	for I=1,#id do
		Commands.Permissions[user][id:sub(I, I)] = true
	end
	Commands.SavePermissions()
end

function Commands.HasPermissions(user, id)
	local user = user:GetUserID()
	local has = true
	if id == "" then return true end
	if not Commands.Permissions[user] then return false end
	for I=1,#id do
		if not Commands.Permissions[user][id:sub(I, I)] then
			has = false
		end
	end
	return has
end

function Commands.GetPerms(user)
	local user = user:GetUserID()
	local perms = Commands.Permissions[user]
	if not perms then return "" end
	local tbl = {}
	for k,v in pairs(perms) do
		table.insert(tbl, k)
	end
	table.sort(tbl)
	local perms2 = ""
	for k,v in pairs(tbl) do
		perms2 = perms2 .. v
	end
	return perms2
end

function Commands.SetPerms(user, id)
	local user = user:GetUserID()
	Commands.Permissions[user] = {}
	for I=1,#id do
		Commands.Permissions[user][id:sub(I, I)] = true
	end
	Commands.SavePermissions()
end

function Commands.RemovePermissions(user, id)
	local user = user:GetUserID()
	Commands.Permissions[user] = Commands.Permissions[user] or {}
	for I=1,#id do
		Commands.Permissions[user][id:sub(I, I)] = nil
	end
	Commands.SavePermissions()
end

function Commands.ClearPerms(user)
	local user = user:GetUserID()
	Commands.Permissions[user] = nil
end
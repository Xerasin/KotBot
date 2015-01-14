Commands.Register("!addperms", function(message, words) 
	local user = message:GetClient():FindUserByName(words[1])
	if user then 
		Commands.AddPermission(user, words[2])
		local perms = Commands.GetPerms(user)
		message:Reply("There you go <3 also, here are " .. user:GetName() .. "'s permissions *giggle*: " .. perms)
	else
		message:Reply("Unable to find user " .. words[1])
	end
end, "Add a permission flag", "z")

Commands.Register("!setperms", function(message, words) 
	local user = message:GetClient():FindUserByName(words[1])
	if user then 
		Commands.SetPerms(user, words[2])
		local perms = Commands.GetPerms(user)
		message:Reply("There you go <3 also, here are " .. user:GetName() .. "'s permissions *giggle*: " .. perms)
	else
		message:Reply("Unable to find user " .. words[1])
	end
end, "Set permission flags", "z")

Commands.Register("!removeperms", function(message, words) 
	local user = message:GetClient():FindUserByName(words[1])
	if user then 
		Commands.RemovePermissions(user, words[2])
		local perms = Commands.GetPerms(user)
		message:Reply("There you go <3 also, here are " .. user:GetName() .. "'s permissions *giggle*: " .. perms)
	else
		message:Reply("Unable to find user " .. words[1])
	end
end, "Remove a permission flag", "z")

Commands.Register("!clearperms", function(message, words) 
	local user = message:GetClient():FindUserByName(words[1])
	if user then 
		Commands.ClearPerms(user)
		message:Reply("Removed " .. user:GetName() .. " Permissions :( *sniffs*")
	else
		message:Reply("Unable to find user " .. words[1])
	end
end, "Clear permission flags", "z")

Commands.Register("!getperms", function(message, words) 
	local user = message:GetClient():FindUserByName(words[1])
	if user then 
		local perms = Commands.GetPerms(user)
		message:Reply("here are " .. user:GetName() .. "'s permissions *giggle*: " .. perms)
	else
		message:Reply("Unable to find user " .. words[1])
	end
end, "Get the permissions", "")
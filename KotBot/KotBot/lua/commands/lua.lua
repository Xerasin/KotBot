Commands.Register("!l", function(message, words, text) 
	local tbl = {dolua(text, "message", message)}
	if not tbl[1] then
		message:Reply(tostring(tbl[2]))
	end
end, "Run lua", "l")

Commands.Register("!print", function(message, words, text) 
	local tbl = {dolua("return " .. text, "message", message)}
	message:Reply(tostring(tbl[2]))
end, "Run lua", "l")

Commands.Register("!lurl", function(message, words, text) 
	local client = Webclient.Create()
	local status, lua = pcall(client.DownloadString, client, text)
	if status then
		local tbl = {dolua(lua, "message", message)}
		if not tbl[1] then
			message:Reply(tostring(tbl[2]))
		end
	else
		message:Reply("baka... this is a bad link... *pouts*")
	end
end, "Run lua from web", "l")
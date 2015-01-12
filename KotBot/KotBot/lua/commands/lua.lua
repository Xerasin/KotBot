Commands.Register("!l", function(message, words, text) 
	local error = dolua(text, "message", message)
	if error then
		message:Reply(error)
	end
end, "Run lua", "o")
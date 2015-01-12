hook.Add("MessageRecieved", "MessageRecieved.hi", function(message)
	local name = message:GetSender():GetName()
	local text = message:GetMessage():lower()
	if text:find("kot") and text:find("hi") then
		message:Reply("hey " .. name)
	end
	print(words.Split(text)[1] or "Eek")
end)
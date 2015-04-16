Commands.Register("!joinchannel", function(message, words) 
	if message:GetClient().JoinChannel and type(message:GetClient().JoinChannel) ~= "string" then
		message:Reply("Okay I'll join that channel!")
		message:GetClient():JoinChannel(words[1])
	end
end, "Join a channel", "a")
Commands.Register("!leavechannel", function(message, words) 
	if message:GetClient().LeaveChannel and type(message:GetClient().LeaveChannel) ~= "string" then
		message:Reply("Fine I'll leave...")
		message:GetClient():LeaveChannel()
	end
end, "Leave the current channel", "a")
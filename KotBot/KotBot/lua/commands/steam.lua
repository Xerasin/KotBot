Commands.Register("!addfriend", function(message, words) 
	if words[1]:sub(1, 4) == "http" then
		words[1] = Steam.GetSteamIDFromCommunity(words[1])
	end
	message:Reply("Okay I'll add " .. words[1] .. " if you want me to....")
	Steam.AddFriend(words[1])
end, "Add a steam friend", "z")
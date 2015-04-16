Commands.Register("!rtd", function(message, words) 
	local sides = tonumber(words[2]) or 6
	local times = tonumber(words[1]) or 1
	if sides and times and times >= 1 and sides > 1 then
		if times > 50 then times = 50 end
		local str = "I rolled " .. times .. " "..sides.." sided dice and got "
		if times == 1 then
			str = "I rolled a "..sides.." sided die and got "
		end
		for I=1,times do 
			str = str .. math.random(1, sides) .. ", "
		end
		message:Reply(str:sub(1, -3))
	end
end, "!rtd <times> <sides>", "")
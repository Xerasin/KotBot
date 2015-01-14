Commands.Register("!reload", function(message, words) 
	bot.reload()
	message:Reply("Wow, I reloaded myself *blush*")
end, "Reload The lua-side of the bot", "a")
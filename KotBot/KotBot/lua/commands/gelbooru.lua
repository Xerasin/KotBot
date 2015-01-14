Commands.Register("!gelbooru", function(message, words, text) 
	message:Reply("Okay you naughty person ;), I'll get right on that for you <3~")
	local client = Webclient.Create()
	local tags = ""
	if #words > 0 then
		tags = Webclient.Encode(text)
	end
	local str = client:DownloadString("http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags="..tags.. "&limit=1")
	local count = tonumber(str:match("count=\"([%d]+)"))
	if count > 0 then
		local str = client:DownloadString("http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags="..tags.. "&limit=1&pid=" .. math.random(0, count))
		local id = tonumber(str:match("id=\"([%d]+)"))
		local tags = str:match("tags=\"([^\"]+)\"")
		message:Reply("Random gelbooru! url = http://gelbooru.com/index.php?page=post&s=view&id=" .. id .. " tags = " .. tags)
	end
end, "HENTAAAAAAAAAAAAAI", "")
Commands.Alias("!gelbooru", "!hentai")
Commands.Alias("!gelbooru", "!porn")
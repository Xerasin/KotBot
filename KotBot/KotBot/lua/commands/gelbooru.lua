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
		local picked = math.random(0, count - 1)
		local str = client:DownloadString("http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags="..tags.. "&limit=1&pid=" .. picked)
		local id = tonumber(str:match("id=\"([%d]+)"))
		local tags2 = str:match("tags=\"([^\"]+)\"")
		if not id or id == "" then
			message:Reply("No gelbooru image found! (Error?)")
		else
			message:Reply("Random gelbooru! (".. (picked + 1) .."/".. count .." | "..text..") url = http://gelbooru.com/index.php?page=post&s=view&id=" .. id .. " tags = " .. tags2)
		end
	else
		message:Reply("No gelbooru image found!")
	end
end, "HENTAAAAAAAAAAAAAI", "")
Commands.Alias("!gelbooru", "!hentai")

Commands.Register("!e621", function(message, words, text) 
	message:Reply("Okay you naughty person ;), I'll get right on that for you <3~")
	local tags = ""
	if #words > 0 then
		tags = Webclient.Encode(text)
	end
	if #words > 6 then
		message:Reply("You need to have less than 6 tags :(")
		return
	end
	Webclient.FetchAsync("http://e621.net/post/index.xml?tags="..tags.. "&limit=1", function(str)
		local count = tonumber(str:match("count=\"([%d]+)"))
		if count > 0 then
			local picked = math.random(0, count - 1)
			Webclient.FetchAsync("http://e621.net/post/index.xml?tags="..tags.. "&limit=1&page=" .. picked, function(str)
				local id = tonumber(str:match(" id=\"([%d]+)\""))
				local tags2 = str:match("tags=\"([^\"]+)\"")
				if not id or id == "" then
					message:Reply("No e621 image found! (Error?)")
				else
					message:Reply("Random e621! (".. (picked + 1) .."/".. count .." | "..text..") url = http://e621.net/post/show/" .. id .. " tags = " .. tags2)
				end
			end)
		else
			message:Reply("No e621 image found!")
		end
	end)
end, "e621", "")
Commands.Alias("!e621", "!yiff")
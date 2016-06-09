Commands.Register("!gelbooru", function(message, words, text) 
	message:Reply("Okay you naughty person ;), I'll get right on that for you <3~")
	local tags = ""
	if #words > 0 then
		tags = Webclient.Encode(text)
	end
	Webclient.FetchAsync("http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags="..tags.. "&limit=1", function(str)
		local count = tonumber(str:match("count=\"([%d]+)"))
		if count > 0 then
			local picked = math.random(0, count - 1)
			local str = Webclient.FetchAsync("http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags="..tags.. "&limit=1&pid=" .. picked, function(str)
				local id = tonumber(str:match("id=\"([%d]+)"))
				local tags2 = str:match("tags=\"([^\"]+)\"")
				local fileURL = str:match("file_url=\"([^\"]+)\"")
				local realURL = "http://gelbooru.com/index.php?page=post&s=view&id=" .. id
				local outputUrl = realURL
				if not id or id == "" then
					message:Reply("No gelbooru image found! (Error?)")
				else
					local loc = message:GetClient():GetLocationString()
					local text = "Random gelbooru! (".. (picked + 1) .."/".. count .." | "..text..") url = " .. outputUrl .. " tags = " .. tags2
					if loc:sub(1, 9) == "|Discord|" then text = text .. " image = " .. fileURL end
					message:Reply(text)
				end
			end, "user_id=251550; pass_hash=0c0691a06e4ae99bd04dc3c1c45bef3fbe539009")
		else
			message:Reply("No gelbooru image found!")
		end
	end, "user_id=251550; pass_hash=0c0691a06e4ae99bd04dc3c1c45bef3fbe539009")
end, "HENTAAAAAAAAAAAAAI", "")
Commands.Alias("!gelbooru", "!hentai")

Commands.Register("!e621", function(message, words, text) 
	message:Reply("Okay you naughty person ;), I'll get right on that for you <3~")
	local tags = ""
	if #words > 0 then
		tags = Webclient.Encode(text)
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
			end, 
			function(error) 
				message:Reply("Random e621 error: "  .. error)
			end)
		else
			message:Reply("No e621 image found!")
		end
	end, 
	function(error) 
		message:Reply("No more than 6 tags")
	end)
end, "e621", "")
Commands.Alias("!e621", "!yiff")
local cache = {}
function FindMatch(str, patterns)
    for _, pattern in pairs(patterns) do
        local m = {str:match(pattern)}
        if m[1] then return m end
    end

    return nil
end
function PrintURL(message, url, title)
	message:Reply(url .. ": " .. title)
end
function lengthToLengthString(length)
	if(length == nil) then return end
	Seconds = ( math.floor(length) % 60 )
	Minutes = (math.floor(length /60) % 60)
	Hours = (math.floor(length / 60 / 60 ))
	local TimeS = string.format("%02d:%02d:%02d", Hours, Minutes, Seconds)
	return TimeS
end
hook.Add("MessageRecieved", "MessageRecieved.urlgrabber", function(message)
	local user = message:GetClient():GetName()
	local name = message:GetSender():GetName()
	if name == user then return end
	local text = message:GetMessage()
	local words = words.Split(text)
	for I=1,#words do
		local word = words[I]
		if word:match("[%w]+%.[%w]+") then
			local client = Webclient.Create()
			if word:sub(1, 7) ~= "http://" and word:sub(1, 8) ~= "https://" then
                word = "http://" .. word;
            end
			local m = FindMatch(word, {
				"^http[s]?://youtube%.com/watch%?.*v=([A-Za-z0-9_%-]+)",
				"^http[s]?://youtu%.be/([A-Za-z0-9_%-]+)",
				"^http[s]?://[A-Za-z0-9%.%-]*%.youtube%.com/watch%?.*v=([A-Za-z0-9_%-]+)",
				"^http[s]?://[A-Za-z0-9%.%-]*%.youtube%.com/v/([A-Za-z0-9_%-]+)",
				"^http[s]?://youtube%-nocookie%.com/watch%?.*v=([A-Za-z0-9_%-]+)",
				"^http[s]?://[A-Za-z0-9%.%-]*%.youtube%-nocookie%.com/watch%?.*v=([A-Za-z0-9_%-]+)",
				
			})
			if not m then
				if cache[word] then
					PrintURL(message, word, cache[word])
					return
				end
				local status, url_data = pcall(client.DownloadString, client, word)
				if status then
					local title = url_data:match("<title>(.+)</title>") or "???"
					cache[word] = title
					PrintURL(message, word, cache[word])
				end
			else
				if cache[word] then
					PrintURL(message, word, cache[word])
					return
				end
				local url_data = client:DownloadString("http://gdata.youtube.com/feeds/api/videos/" .. m[1])
				local title = url_data:match("<title type='text'>(.+)</title>") or "???"
				local views = url_data:match("viewCount='(%d+)'") or "???"
				local duration = url_data:match("<yt:duration seconds='(%d+)'/>") or "???"
				local name = url_data:match("<name>(.+)</name>") or "???"
				--sendto.SendEmote(string.Format("Youtube [ {0} ]: [{1}] Uploader - [{2}] Views - [{3}] Length - [{4:hh\\:mm\\:ss}]", URI, title, uploader, ViewCount, new TimeSpan(0, 0, Duration)));
				print(duration)
				cache[word] = "Youtube [ " .. word  .. " ]: [" .. title .. "] Uploader - ["..name.."] Views - ["..views.."] Length - ["..lengthToLengthString(tonumber(duration) or 0).. "]"
				PrintURL(message, word, cache[word])
			end
		end
	end
end)
Steam = Steam or {}
function Steam.GetSteamIDFromCommunity(url)
	local client = Webclient.Create()
	local url = url .. "?xml=1"
	local status, url_data = pcall(client.DownloadString, client, url)
	if status then
		local t = url_data:match("<steamID64>(.+)</steamID64>")
		return t
	end
end
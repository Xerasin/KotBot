Steam = Steam or {}
--import("SteamKit2")
--import("SteamKit2.Unified.Internal")

function Steam.GetSteamIDFromCommunity(url)
	local client = Webclient.Create()
	local url = url .. "?xml=1"
	local status, url_data = pcall(client.DownloadString, client, url)
	if status then
		local t = url_data:match("<steamID64>(.+)</steamID64>")
		return t
	end
end

hook.Add("SteamOnLogin", "Hooks", function(callback)
	local manager = Steam.GetCallbackManager

end)
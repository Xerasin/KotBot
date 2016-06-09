XerasinUtil = {}

function XerasinUtil.ShortenURL(url)
	local client = Webclient.Create()
	local status, url_data = pcall(client.DownloadString, client, "http://xeras.in/post.php?url=" .. Webclient.Encode(url))
	if status then
		return url_data
	end
end
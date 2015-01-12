words = {}
function words.Split(message)
	local t = {}
	for m in string.gmatch("([%S])", message) do
		table.insert(t, m)
	end
	return t
end
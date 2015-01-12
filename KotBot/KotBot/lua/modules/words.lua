words = {}
function words.Split(message)
	local t = {}
	for m in string.gmatch(message, "([^%s]+)") do
		table.insert(t, m)
	end
	return t
end
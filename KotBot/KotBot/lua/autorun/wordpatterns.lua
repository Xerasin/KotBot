local Phrases = {}
local greetings = {
	["hi"] = true,
	["hello"] = true,
	["hey"] = true,
	["sup"] = true,
	["greetings"] = true,
	["yo$"] = true,
}
local notgreetings = {
	["bye"] = true,
	["goodbye"] = true,
	["cya"] = true,
	["later"] = true,
}
local function registerwords(msg, ...)
	local crap = {...}
	local tbl = Phrases
	for I=1,#crap do
		local v = crap[I]
		tbl[v] = tbl[v] or {next = {}}
		tbl = tbl[v].next
	end
	tbl.msg = msg 
end

for k,v in pairs(greetings) do
	registerwords("Yay {user} noticed me! <3", "^" .. k, "^kot")
	registerwords("Hey, {user}... don't call me that... *cries*", "^" .. k, "^knot")
end
for k,v in pairs(notgreetings) do
	registerwords("See you {user}! <3", "^" .. k, "^kot")
	registerwords("Bye {user}...", "^" .. k, "^knot")
end
registerwords("That wasn't very nice, {user}. :( *cries*", "fuck", "you", "^kot")
registerwords("That was very mean, {user}. :( *runs crying*", "fuck", "you", "^knot")

registerwords("Okay, I'll hide :(", "^kot", "hide")
--registerwords("Thanks {user}... *stops crying whipes her eyes*", "I'm", "sorry", "^kot")
registerwords("Thanks {user}... *stops crying and whipes her eyes*", "sorry", "^kot")

--registerwords("Apology not accepted {user}... *pouts*", "I'm", "sorry", "^knot")
registerwords("Apology not accepted {user}... *pouts*", "sorry", "^knot")

hook.Add("MessageRecieved", "MessageRecieved.wordpatterns", function(message)
	local name = message:GetSender():GetName()
	local text = message:GetMessage():lower()
	local words = words.Split(text)
	for I=1,#words do
		local loop
		function loop(tbl, nex)	
			local word = words[nex]
			if word then
				for k,v in pairs(tbl) do
					if word:match(k) then
						if v.next then
							loop(v.next, nex + 1)
							return
						end
					end
				end
				if tbl.msg then
					message:Reply(tostring(tbl.msg:gsub("{user}", name)))
					return
				end
			else
				if tbl.msg then
					message:Reply(tostring(tbl.msg:gsub("{user}", name)))
					return
				end
				return
			end
		end
		loop(Phrases, I)
	end
end)
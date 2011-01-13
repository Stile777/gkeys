Console = luanet.import_type "System.Console"
GKey = luanet.import_type "GKeys.GKey"

function onGKeyDown(whichKey)
    local s = (whichKey+1) .. " was pressed"
	Console.WriteLine(s)
end
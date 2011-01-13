Console = luanet.import_type "System.Console"

GKey = luanet.import_type "GKeys.GKey"   
GKeyHandler = luanet.import_type "GKeys.GKeyHandler"

function onGKeyDown(whichKey)
    local s = "G" .. (whichKey+1) .. " was pressed down"
    Console.WriteLine(s)
end

function onGKeyUp(whichKey)
    local s = "G" .. (whichKey+1) .. " was released"
    Console.WriteLine(s)
end

function onModeChanged(newMode)

	Console.WriteLine("Mode changed to " .. (newMode+1))
end
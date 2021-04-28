-- defines a factorial function
		function fact (n)
			if (n == 0) then
				return 1
			else
				return n*fact(n - 1)
			end
		end

-- defines blah blah blah
function CheckCondition(obj)
	if (obj.isTopSoul()) then
		return true
	else
		return false
	end
end


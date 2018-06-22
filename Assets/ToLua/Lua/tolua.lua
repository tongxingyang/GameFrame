--------------------------------------------------------------------------------
--      Copyright (c) 2015 - 2016 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------
if jit then		
	if jit.opt then		
		jit.opt.start(3)				
	end		
	
	print("ver"..jit.version_num.." jit: ", jit.status())
	print(string.format("os: %s, arch: %s", jit.os, jit.arch))
end

if DebugServerIp then  
  require("mobdebug").start(DebugServerIp)
end

require "tolua_misc.functions"
Mathf		= require "tolua_UnityEngine.Mathf"
Vector3 	= require "tolua_UnityEngine.Vector3"
Quaternion	= require "tolua_UnityEngine.Quaternion"
Vector2		= require "tolua_UnityEngine.Vector2"
Vector4		= require "tolua_UnityEngine.Vector4"
Color		= require "tolua_UnityEngine.Color"
Ray			= require "tolua_UnityEngine.Ray"
Bounds		= require "tolua_UnityEngine.Bounds"
RaycastHit	= require "tolua_UnityEngine.RaycastHit"
Touch		= require "tolua_UnityEngine.Touch"
LayerMask	= require "tolua_UnityEngine.LayerMask"
Plane		= require "tolua_UnityEngine.Plane"
Time		= reimport "tolua_UnityEngine.Time"

list		= require "tolua_list"
utf8		= require "tolua_misc.utf8"

require "tolua_event"
require "tolua_typeof"
require "tolua_slot"
require "tolua_System.Timer"
require "tolua_System.coroutine"
require "tolua_System.ValueType"
require "tolua_System.Reflection.BindingFlags"

--require "misc.strict"
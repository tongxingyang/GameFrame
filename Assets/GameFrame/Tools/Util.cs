using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public static class Util {

		public static void SetResolution(int resolution){
			#if UNITY_STANDALONE || UNITY_STANDALONE_OSX
				Screen.SetResolution(1280,720,false);
			#else
				if(resolution<720) return;
				if(resolution>1080) return;
				int w = Screen.width;
				int h = Screen.height;
				if(h>resolution){
					int width = resolution *w /h;
					int height = resolution;
					Screen.SetResolution(width,height,true);
				}
			#endif
		}
	}

}

































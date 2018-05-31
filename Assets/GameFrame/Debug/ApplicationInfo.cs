using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFrame.Debug
{
    /// <summary>
    /// 获取应用程序设备运行信息
    /// </summary>
    public class ApplicationInfo
    {
        public static string GetAppInfo()
    	{
    		string info = "";
    		
    		info += "\nApplication.unityVersion=" + Application.unityVersion ;
    		info += "\nApplication.version=" + Application.version ;
    		info += "\nApplication.productName=" + Application.productName ;
    		info += "\nApplication.isEditor=" + Application.isEditor ;
    		info += "\nApplication.isConsolePlatform=" + Application.isConsolePlatform ;
    		info += "\nApplication.isMobilePlatform=" + Application.isMobilePlatform ;
    		info += "\nApplication.platform=" + Application.platform ;

    		info += "\nApplication.persistentDataPath=" + Application.persistentDataPath ;
    		info += "\nApplication.dataPath=" + Application.dataPath ;
    		info += "\nApplication.streamingAssetsPath=" + Application.streamingAssetsPath ;
    		info += "\nApplication.temporaryCachePath=" + Application.temporaryCachePath ;

    		info += "\n(WebPlay)Application.absoluteURL=" + Application.absoluteURL ;


    		
    		info += "\nApplication.internetReachability=" + Application.internetReachability ;

    		
            info += "\nSceneManager.sceneCountInBuildSettings=" + SceneManager.sceneCountInBuildSettings ;
            info += "\nSceneManager.GetActiveScene().name=" + SceneManager.GetActiveScene().name ;
            info += "\nSceneManager.GetActiveScene().path=" + SceneManager.GetActiveScene().path ;
            info += "\nSceneManager.GetActiveScene().isLoaded=" + SceneManager.GetActiveScene().isLoaded ;
            info += "\nSceneManager.GetActiveScene().IsValid()=" + SceneManager.GetActiveScene().IsValid() ;

    		info += "\nApplication.isPlaying=" + Application.isPlaying ;
    		info += "\nApplication.runInBackground=" + Application.runInBackground ;



    		info += "\nApplication.backgroundLoadingPriority=" + Application.backgroundLoadingPriority ;
    		info += "\nApplication.bundleIdentifier=" + Application.identifier ;
    		info += "\nApplication.cloudProjectId=" + Application.cloudProjectId ;
    		info += "\nApplication.companyName=" + Application.companyName ;
    		info += "\nApplication.genuine=" + Application.genuine ;
    		info += "\nApplication.genuineCheckAvailable=" + Application.genuineCheckAvailable ;
    		info += "\nApplication.installMode=" + Application.installMode ;
    		info += "\nApplication.genuineCheckAvailable=" + Application.genuineCheckAvailable ;
    		info += "\nApplication.sandboxType=" + Application.sandboxType ;
    		info += "\nApplication.streamedBytes=" + Application.streamedBytes ;
    		info += "\nApplication.systemLanguage=" + Application.systemLanguage ;
    		info += "\nApplication.targetFrameRate=" + Application.targetFrameRate ;

    		
    		info += "\n-------" ;

    		
    		info += "\nSystemInfo.DeviceModel : " + SystemInfo.deviceModel;
    		info += "\nSystemInfo.deviceName : " + SystemInfo.deviceName;
    		info += "\nSystemInfo.deviceType : " + SystemInfo.deviceType;
    		info += "\nSystemInfo.deviceUniqueIdentifier : " + SystemInfo.deviceUniqueIdentifier;
    		info += "\nSystemInfo.graphicsDeviceName : " + SystemInfo.graphicsDeviceName;
    		info += "\nSystemInfo.graphicsMemorySize : " + SystemInfo.graphicsMemorySize+ "MB";
    		info += "\nSystemInfo.graphicsShaderLevel : " + SystemInfo.graphicsShaderLevel;
    		info += "\nSystemInfo.maxTextureSize : " + SystemInfo.maxTextureSize;
    		info += "\nSystemInfo.npotSupport : " + SystemInfo.npotSupport;
    		info += "\nSystemInfo.operatingSystem : " + SystemInfo.operatingSystem;
    		info += "\nSystemInfo.processorCount : " + SystemInfo.processorCount;
    		info += "\nSystemInfo.processorType : " + SystemInfo.processorType;
    		info += "\nSystemInfo.systemMemorySize : " + SystemInfo.systemMemorySize+ "MB";
    		info += "\nScreen : " + Screen.currentResolution.width +" x "+ Screen.currentResolution.height;
    		info += "\nScreen.currentResolution.refreshRate : " + Screen.currentResolution.refreshRate;
    		info += "\nScreen.sleepTimeout : " + Screen.sleepTimeout;

            info += "\nDeviceResolution.width=" + Screen.width;
            info += "\nDeviceResolution.height=" + Screen.height;
    		info += "\n[Screen.resolutions] ";
    		for(int i = 0; i < Screen.resolutions.Length; i ++)
    		{
    			info += "\n    i=" + i + ",  width=" +  Screen.resolutions[i].width + ",  height=" + Screen.resolutions[i].height + ",  refreshRate=" + Screen.resolutions[i].refreshRate ;
    		}
    	
    		info += "\n";
    	
    		return info;
    	}
    }
}
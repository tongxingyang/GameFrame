using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static  class WebRequestExtensions  {

	public static WebResponse TryGetEndGetresponse(this WebRequest webRequest, IAsyncResult ansycResult)
	{
		try
		{
			return webRequest.EndGetResponse(ansycResult);
		}
		catch (Exception e)
		{
			throw;
		}
	}
}

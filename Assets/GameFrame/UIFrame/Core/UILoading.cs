using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : WindowBase
{

	private Image loadingSprite;
	protected override void OnInit(Camera UICamera)
	{
		base.OnInit(UICamera);
		loadingSprite = CacheTransform.Find("Loading").GetComponent<Image>();
		loadingSprite.material.SetFloat("_RotateSpeed",150);
	}
}

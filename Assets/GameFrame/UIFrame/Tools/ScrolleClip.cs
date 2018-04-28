using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrolleClip : MonoBehaviour
{
	public RectTransform m_rectTrans;
	private float m_halfWidth, m_halfHeigth, m_canvasScale;
	public Transform canvas;
	void Start()
	{
		Debug.LogError(canvas.localScale.x);
		m_halfWidth = m_rectTrans.sizeDelta.x * canvas.localScale.x * 0.5f;
		m_halfHeigth = m_rectTrans.sizeDelta.y * canvas.localScale.y * 0.5f;
		var renders = GetComponentsInChildren<Renderer>(); 
		Vector4 area = CalcArea(m_rectTrans.position); 
		for(int i = 0, j = renders.Length; i < j; i++)
		{
			renders[i].material.SetVector("_Area",area);
		}  
	}
	public Vector4 CalcArea(Vector3 position)
	{
		return new Vector4()
		{
			x = position.x - m_halfWidth,  
			y = position.y - m_halfHeigth,  
			z = position.x + m_halfWidth,  
			w = position.y + m_halfHeigth 
		};
	}
}

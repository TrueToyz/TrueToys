using UnityEngine;
using System.Collections;

public class TransitionEffect : MonoBehaviour {

	public	bool	m_isActive;
	

	public	virtual	void activate()
	{
		m_isActive = true;
	}

	public	virtual	void desactivate ()
	{
		m_isActive = false;
	}
}

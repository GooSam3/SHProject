using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKillMessageItem : MonoBehaviour
{
	[SerializeField] private Text ourTeamDeadText;
	[SerializeField] private GameObject ourTeamDeadObj;
	[SerializeField] private Text enemyTeamDeadText;
	[SerializeField] private GameObject enemyTeamDeadObj;

	public void Initialize()
	{
		ourTeamDeadObj.SetActive( false );
		enemyTeamDeadObj.SetActive( false );
		gameObject.SetActive( false );
	}

	public void Show( UIKillMessage.MessageInfo info )
	{
		switch( info.MessageType ) {
			case E_KillMessage.OurTeamDead: {
				ourTeamDeadObj.SetActive( true );
				enemyTeamDeadObj.SetActive( false );
				ourTeamDeadText.text = info.MessageText;
				break;
			}
			case E_KillMessage.EnemyTeamDead: {
				ourTeamDeadObj.SetActive( false );
				enemyTeamDeadObj.SetActive( true );
				enemyTeamDeadText.text = info.MessageText;
				break;
			}
		}

		gameObject.SetActive( true );
	}

	public void Hide()
	{
		ourTeamDeadObj.SetActive( false );
		enemyTeamDeadObj.SetActive( false );
		gameObject.SetActive( false );
	}

}
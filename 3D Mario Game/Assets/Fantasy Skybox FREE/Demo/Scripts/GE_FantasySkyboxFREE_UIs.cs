// Fantasy Skybox
// Version: 1.4.0
// Compatilble: Unity 2017.3.1 or higher, see more info in Readme.txt file.
//
// Developer:			Gold Experience Team (https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:4162)
// Unity Asset Store:	https://www.assetstore.unity3d.com/en/#!/content/18353
//
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com

#region Namespaces

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#endregion // Namespaces

// ######################################################################
// GE_FantasySkyboxFREE_UIs handles user key inputs.
// Note this class is attached with GE_FantasySkybox_UIs object in "Fantasy Skybox Demo (960x600px)" scene.
// ######################################################################
namespace FantasySkyFree
{
	public class GE_FantasySkyboxFREE_UIs : MonoBehaviour
	{
		// ########################################
		// Variables
		// ########################################

		#region Variables

		// Canvas
		public Canvas m_Canvas = null;

		// Help
		public Button m_Help_Button = null;
		public GameObject m_Help_Window = null;

		// Details
		public GameObject m_Details = null;

		// Details Panel
		public GameObject m_PanelDetails = null;

		// HowTo
		public GameObject m_HowTo1 = null;

		#endregion // Variables

		// ########################################
		// MonoBehaviour Functions
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
		// ########################################

		#region MonoBehaviour

		// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
		void Start()
		{
		}

		// Update is called every frame, if the MonoBehaviour is enabled.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
		void Update()
		{
		}

		#endregion // MonoBehaviour

		// ########################################
		// UI Respond functions
		// ########################################

		#region UI Respond functions

		// User press Support button
		public void Button_Help_Support()
		{
			// http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
			Application.OpenURL("mailto:geteamdev@gmail.com");
		}

		// User press Products button
		public void Button_Help_Products()
		{
			// http://docs.unity3d.com/ScriptReference/Application.ExternalEval.html
			//Application.ExternalEval("window.open('https://www.facebook.com/GETeamPage/','GOLD EXPERIENCE TEAM')");

			// http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
			Application.OpenURL("https://www.facebook.com/GETeamPage/");
		}

		#endregion // UI Respond Functions
	}
}
using UnityEngine;
using System.Collections;

public class LoadExamples : MonoBehaviour {

	public void LoadExample(string level){
#if UNITY_5_5 || UNITY_5_6
        UnityEngine.SceneManagement.SceneManager.LoadScene( level );
#else
		Application.LoadLevel( level );
#endif
    }
}

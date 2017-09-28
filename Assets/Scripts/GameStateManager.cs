using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
	public static GameStateManager Instance;

    public string TrackScene;
	public UIController UiController { get; set; }

    private CameraMovement Camera;

    void Awake () {
		if (Instance != null)
		{
			Debug.LogError("Multiple gamestate managers detected");
			return;
		}
		Instance = this;
        
	    //Load gui scene
	    SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
        
	    //Load track scene
	    SceneManager.LoadScene(TrackScene, LoadSceneMode.Additive);

        Camera = UnityEngine.Camera.main.GetComponent<CameraMovement>();
    }
	
	void Start ()
	{
	    TrackManager.Instance.BestCarChanged += OnBestCarChanged;
        EvolutionManager.Instance.StartEvolution();
	}

    private void OnBestCarChanged(CarManager bestCar)
    {
        Camera.SetTarget(bestCar == null ? null : bestCar.gameObject);

        if (UiController != null)
            UiController.SetDisplayTarget(bestCar);
    }
}

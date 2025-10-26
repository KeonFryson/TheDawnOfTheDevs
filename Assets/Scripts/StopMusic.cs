using UnityEngine;

public class StopMusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] public AudioSource musicSource;
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale == 0f)
        {

            musicSource.Pause();
        }

        if(Time.timeScale == 1f)
        {

            musicSource.UnPause();
        }
    }
}

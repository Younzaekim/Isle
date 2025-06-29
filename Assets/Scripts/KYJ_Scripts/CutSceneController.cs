using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public class CutSceneController : MonoBehaviour
{

    public PlayableDirector playableDirector;
    public TimelineAsset timelineAsset;
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        timelineAsset = playableDirector.playableAsset as TimelineAsset;
    }


    private void OTriggerEnter(Collider other)
    {
        if (other.CompareTag("CutSceneTrigger"))
        {
            // Check if the playableDirector is not null
            other.gameObject.SetActive(false);
            playableDirector.Play(timelineAsset);
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

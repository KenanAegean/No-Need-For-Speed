using UnityEngine;
using Unity.Cinemachine;

public class AutoAddPlayerToVcamTargets : MonoBehaviour
{
    [TagField]
    public string Tag = string.Empty;
    public bool isTargetFound = false;


    void Update()
    {
        SetCamera();
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetCamera()
    {
        var vcam = GetComponent<CinemachineCamera>();
        if (vcam != null && Tag.Length > 0)
        {
            var targets = GameObject.FindGameObjectsWithTag(Tag);
            if (targets.Length > 0)
                //vcam.LookAt = vcam.Follow = targets[0].transform;
                vcam.Target.TrackingTarget = targets[0].transform;
        }
    }

}
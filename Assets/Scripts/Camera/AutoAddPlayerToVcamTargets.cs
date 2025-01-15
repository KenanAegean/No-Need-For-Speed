using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class AutoAddPlayerToVcamTargets : MonoBehaviour
{
    public bool isTargetFound = false;


    void Update()
    {
        if (isTargetFound == false)
        {
            StartCoroutine(SetCamera());
        }
    }

    void Start()
    {

    }

    IEnumerator SetCamera()
    {
        yield return new WaitForSeconds(1f);

        if (GameObject.FindGameObjectsWithTag("Player2").Length > 0)
        {
            var vcam = GetComponent<CinemachineCamera>();
            if (vcam != null)
            {
                var targets = GameObject.FindGameObjectsWithTag("Player2");
                if (targets.Length > 0)
                    //vcam.LookAt = vcam.Follow = targets[0].transform;
                    vcam.Target.TrackingTarget = targets[0].transform;
            }
            isTargetFound = true;
        }
        else if (GameObject.FindGameObjectsWithTag("Player").Length > 0)
        {
            var vcam = GetComponent<CinemachineCamera>();
            if (vcam != null)
            {
                var targets = GameObject.FindGameObjectsWithTag("Player");
                if (targets.Length > 0)
                    //vcam.LookAt = vcam.Follow = targets[0].transform;
                    vcam.Target.TrackingTarget = targets[0].transform;
            }
            isTargetFound = true;
        }

    }

}
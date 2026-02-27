using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DissolvingController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;
    public ParticleSystem dissolveVFX;

    private Material[] skinnedMaterials; 

    void Start()
    {
        if (skinnedMesh != null)
        {
            skinnedMaterials = skinnedMesh.materials; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(DissolveCo());
            StartCoroutine(PlayVFXAfterDelay(0.5f));
        }
    }

    IEnumerator DissolveCo()
    {
        if (skinnedMaterials.Length > 0)
        {
            float counter = 0;

            while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
            {
                counter += dissolveRate;
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }

    IEnumerator PlayVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dissolveVFX != null)
            dissolveVFX.Play();
    }
}



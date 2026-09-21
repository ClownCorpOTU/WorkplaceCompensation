using UnityEngine;
using System.Collections;

public class DissolvingController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMesh;
    [SerializeField] private Material dissolvingBlobbyMat;
    [SerializeField] private ParticleSystem dissolveVFX;
    [SerializeField] private Rigidbody vestRB;

    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    private Material originalMaterial;
    private Material[] skinnedMaterials; 

    void Start()
    {
        if (skinnedMesh != null)
        {
            skinnedMaterials = skinnedMesh.materials;
            originalMaterial = skinnedMaterials[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ResetBurningFx();
            //BeginFx();
        }
    }

    public void BeginFx()
    {
        var playerColor = skinnedMaterials[0].GetColor("_ChromaKeyColorReplacement");
        
        skinnedMaterials[0] = dissolvingBlobbyMat;
        skinnedMesh.materials = skinnedMaterials;
        skinnedMaterials = skinnedMesh.materials;
        
        skinnedMaterials[0].SetColor("_ChromaKeyColorReplacement", playerColor);
        
        StartCoroutine(DissolveCo());
        StartCoroutine(PlayVFXAfterDelay(0.5f));
    }

    public void ResetBurningFx()
    {
        if (vestRB != null) vestRB.isKinematic = true;
        skinnedMaterials[0].SetFloat("_DissolveAmount", 0);

        skinnedMaterials[0] = originalMaterial;
        skinnedMesh.materials = skinnedMaterials;
        skinnedMaterials = skinnedMesh.materials;
        
        dissolveVFX.Stop();
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

        if (vestRB != null) vestRB.isKinematic = false;

        if (dissolveVFX != null)
            dissolveVFX.Play();
    }
}



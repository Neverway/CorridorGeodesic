using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_RiftPreviewEffects : MonoBehaviour
{
    //[SerializeField] private Alt_Item_Geodesic_Utility_GeoGun geoGun;

    [SerializeField] private Material riftMaterial;
    [SerializeField] private float defaultEmissionStrength = 6;
    [SerializeField] private float collapseExpandEmissionStrength = 9;

    //public float edgeStrength = 1.3f;
    //public float opacity = 0.05f;
    //public float edgeStrengthFactorPower = 1f;
    //public float opacityFactorPower = 2f;
    //public float burstFactorPower = 0.7f;

    private void OnEnable()
    {
        //riftManager.OnStateChanged += OnStateChanged;
    }
    private void OnDisable()
    {
        //riftManager.OnStateChanged -= OnStateChanged;
    }/*
    private void OnDestroy()
    {
        riftPreview.SetFloat("_EffectTime", 0);
        riftPreview.SetFloat("_SphereSize", 0);
        riftPreview.SetFloat("_EmissionStrength", defaultEmisStrength);
    }*/
    public void Update()
    {
        //Graphics_NixieBulbEffects bulb = Graphics_NixieBulbEffects.firstBulb;
        //if (bulb == null)
        //    return;

        //float lerpFactor = bulb.glowFactor * Mathf.Pow(bulb.previewBurstFactor, burstFactorPower);

        //GameObject obj = geoGun.cutPreviews[0];
        //Material mat = obj.GetComponentInChildren<Renderer>().sharedMaterial;

        //mat.SetFloat("_edgeStrength", edgeStrength * Mathf.Pow(lerpFactor, edgeStrengthFactorPower));
        //mat.SetFloat("_opacity", opacity * Mathf.Pow(lerpFactor, opacityFactorPower));

        //float newEdgeStrength = edgeStrength * Mathf.Pow(lerpFactor, edgeStrengthFactorPower);
        //float newOpacity = opacity * Mathf.Pow(lerpFactor, opacityFactorPower);

        //if (Alt_Item_Geodesic_Utility_GeoGun.currentState == RiftState.Closed)
        //{
        //    newEdgeStrength *= 1.5f;
        //    newOpacity *= 1.5f;
        //}

        //SetPreview(geoGun.cutPreviews[0], newEdgeStrength, newOpacity);
        //SetPreview(geoGun.cutPreviews[1], newEdgeStrength, newOpacity);

        //geoGun.cutPreviews[0].SetActive(Alt_Item_Geodesic_Utility_GeoGun.currentState != RiftState.Closed);
    }
/*
    void OnStateChanged()
    {
        if (riftManager.currentState != RiftState.None && riftManager.previousState == RiftState.None)
        {
            StopAllCoroutines();
            StartCoroutine(OnRiftCreated());
        }

        switch (riftManager.currentState)
        {
            case RiftState.None:
                StopAllCoroutines();
                riftPreview.SetFloat("_EffectTime", 0);
                riftPreview.SetFloat("_SphereSize", 0);
                riftPreview.SetFloat("_EmissionStrength", defaultEmisStrength);
                break;
            case RiftState.Preview:
                break;
            case RiftState.Collapsing:
                riftPreview.SetFloat("_EmissionStrength", collapseExpandEmisStrength);
                break;
            case RiftState.Closed:
                riftPreview.SetFloat("_EmissionStrength", defaultEmisStrength);
                break;
            case RiftState.Expanding:
                riftPreview.SetFloat("_EmissionStrength", collapseExpandEmisStrength);
                break;
            case RiftState.Idle:
                riftPreview.SetFloat("_EmissionStrength", defaultEmisStrength);
                break;
            default:
                break;
        }
    }*/

    public IEnumerator OnRiftCreated(GI_RiftManager _riftManager)
    {
        riftMaterial.SetFloat("_EffectTime", 0);
        riftMaterial.SetFloat("_SphereSize", 0);
        riftMaterial.SetFloat("_EmissionStrength", defaultEmissionStrength);

        //Vector3 bulbA = //geoGun.cutPreviews[0].transform.position;
        //Vector3 bulbB = //geoGun.cutPreviews[1].transform.position;
        var markerA = _riftManager.markerA.transform.position;
        var markerB = _riftManager.markerB.transform.position;

        riftMaterial.SetVector("_BulbsCenter", (markerA + markerB) * 0.5f);
        riftMaterial.SetFloat("_SphereSize", Vector3.Distance(markerA, markerB) * 0.5f);

        float effectTimer = 0;

        while (effectTimer < 1)
        {
            riftMaterial.SetFloat("_EffectTime", effectTimer);
            effectTimer += Time.deltaTime;
            yield return null;
        }

        riftMaterial.SetFloat("_EffectTime", 1);
    }
    /*public void SetPreview(GameObject preview, float edgeStrength, float opacity)
    {
        Material mat = preview.GetComponentInChildren<Renderer>().sharedMaterial;

        mat.SetFloat("_edgeStrength", edgeStrength);
        mat.SetFloat("_opacity", opacity);
    }*/
}

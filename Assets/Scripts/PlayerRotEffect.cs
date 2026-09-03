using UnityEngine;

public class PlayerRotRffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RotTimer rotTimer;
    [SerializeField] Material rotMaterial;

    private SpriteRenderer playerSprite;
    private Material runtimeMaterials;

    float rotAmount;

    private void Start()
    {
        playerSprite = GetComponentInChildren<SpriteRenderer>();
 
        if (rotTimer == null)
        {
            rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        }
        if (rotMaterial == null)
        {
            Debug.LogError("Rot Material is not assigned!");
            return;
        }
        runtimeMaterials = new Material(rotMaterial);
        playerSprite.material = runtimeMaterials;

       
    }
    void Update()
    {
        if (rotTimer == null || rotMaterial == null)
            return;

        rotAmount = 1 - rotTimer.NormalizedTime;

        rotAmount = Mathf.Clamp01(rotAmount);

        runtimeMaterials.SetFloat("_RotAmount", rotAmount);
   
    }

    private void OnDestroy()
    {
  
       if (runtimeMaterials != null)
       {
          Destroy(runtimeMaterials);
       }
    }
}

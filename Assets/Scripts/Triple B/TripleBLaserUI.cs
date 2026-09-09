using UnityEngine;
using UnityEngine.UI;

public class TripleBLaserUI : MonoBehaviour
{
    [SerializeField] TripleBLaser tripleBLaser;
    [SerializeField] Image staminaFill;
    void Update()
    {
        if (tripleBLaser == null || staminaFill == null)
            return;

        staminaFill.fillAmount= tripleBLaser.StaminaProgress;
    }
}

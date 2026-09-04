using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class KeyPromptUI : MonoBehaviour
{
    [System.Serializable]
    public class KeyIcon
    {
       public PlayerAction action;
       public KeyCode key;
       public Image icon;
    }

    [Header("Key Prompt UI Elements")]
    [SerializeField] private GameObject keyPromptPanel;
    [SerializeField] private KeyIcon[] keyIcons;

    [Header("Key Icon Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    public void ShowPrompt(PlayerAction action)
    {
        if (action == PlayerAction.None)
        {
            Hide();
            return;
        }

        keyPromptPanel.SetActive(true);
        foreach (var keyIcon in keyIcons)
        {
            bool match = keyIcon.action == action;
            keyIcon.icon.gameObject.SetActive(match);
            if (match) keyIcon.icon.color = inactiveColor;
        }
    }
        public void LightUpKey(KeyCode key)
        {
            foreach (var keyIcon in keyIcons)
            {
                if (keyIcon.key == key)
                {
                    keyIcon.icon.color = activeColor; 
                    break;
                }
            }
        }
    public void LightUpAction(PlayerAction action)
    {
        foreach (var keyIcon in keyIcons)
        {
            if (keyIcon.action == action)
            {
                keyIcon.icon.color = activeColor; 
                break;
            }
        }
    }
    public void Hide()
    {
        keyPromptPanel.SetActive(false);
    }



}

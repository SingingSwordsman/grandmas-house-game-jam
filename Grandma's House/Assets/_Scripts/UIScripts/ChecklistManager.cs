using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
public class ChecklistManager : MonoBehaviour
{
 

    [System.Serializable]
    public class ObjectiveEntry
    {
        public TextMeshProUGUI label;
        public Image strikethrough; // optional — leave empty if you don't want one
    }

    [SerializeField] GameObject piss;
    public ObjectiveEntry[] objectives;

    public void CompleteObjective(int index)
    {
        if (index < 0 || index >= objectives.Length) return;
        var obj = objectives[index];

        obj.label.color = new Color(0.5f, 0.5f, 0.5f);

        if (obj.strikethrough != null)
        {
            obj.strikethrough.gameObject.SetActive(true);
            StartCoroutine(AnimateStrike(obj.strikethrough));
        }
    }

    private IEnumerator AnimateStrike(Image strike)
    {
        RectTransform rt = strike.rectTransform;
        float duration = 0.25f, elapsed = 0f;
        rt.localScale = new Vector3(0, 1, 1);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.localScale = new Vector3(elapsed / duration, 1, 1);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    public void TogglePopUp()
    {
        piss.SetActive(!piss.activeInHierarchy);
    }
}
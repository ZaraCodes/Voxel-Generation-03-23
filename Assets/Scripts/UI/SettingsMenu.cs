using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private Slider renderDistance;

    [SerializeField] private TMP_Text renderDistanceDisplay;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void OnRenderDistanceSliderChanged(float newDistance)
    {
        renderDistanceDisplay.text = newDistance.ToString();
        SettingsManager.Instance.ViewDistance = (int) newDistance;
    }

    private void OnEnable()
    {
        renderDistanceDisplay.text = SettingsManager.Instance.ViewDistance.ToString();
    }
}

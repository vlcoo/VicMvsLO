using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindManager : MonoBehaviour
{
    public static RebindManager Instance;

    public InputActionAsset controls;
    public GameObject headerTemplate, buttonTemplate, axisTemplate, playerSettings, resetAll;
    public Toggle fireballToggle;

    private readonly List<RebindButton> buttons = new();

    public void Init()
    {
        Instance = this;

        buttonTemplate.SetActive(false);
        axisTemplate.SetActive(false);
        headerTemplate.SetActive(false);

        LoadBindings();
        CreateActions();
        resetAll.transform.SetAsLastSibling();
    }

    public void LoadBindings()
    {
        var json = GlobalController.Instance.controlsJson;

        if (json != null && json != "")
        {
            // we have old bindings...
            controls.LoadBindingOverridesFromJson(json);
            Debug.Log("load from globalcontroller: " + json);
        }
        else if (InputSystem.file.Exists)
        {
            //load bindings...
            try
            {
                Debug.Log("load from file: " + File.ReadAllText(InputSystem.file.FullName));
                controls.LoadBindingOverridesFromJson(File.ReadAllText(InputSystem.file.FullName));
                GlobalController.Instance.controlsJson = controls.SaveBindingOverridesAsJson();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public void ResetActions()
    {
        controls.RemoveAllBindingOverrides();

        foreach (var button in buttons)
        {
            button.targetBinding = button.targetAction.bindings[button.index];
            button.SetText();
        }

        fireballToggle.isOn = true;
        Settings.Instance.fireballFromSprint = true;
        Settings.Instance.SaveSettingsToPreferences();
        SaveRebindings();
    }

    private void CreateActions()
    {
        foreach (var map in controls.actionMaps)
        {
            if (map.name.StartsWith("!"))
                continue;

            var newHeader = Instantiate(headerTemplate);
            newHeader.name = map.name;
            newHeader.SetActive(true);
            newHeader.transform.SetParent(transform, false);
            newHeader.GetComponentInChildren<TMP_Text>().text = map.name;

            foreach (var action in map.actions)
            {
                if (action.name.StartsWith("!"))
                    continue;

                if (action.bindings[0].isComposite)
                {
                    //axis
                    var newTemplate = Instantiate(axisTemplate);
                    newTemplate.name = action.name;
                    newTemplate.transform.SetParent(transform, false);
                    newTemplate.SetActive(true);
                    var control = newTemplate.GetComponent<RebindControl>();
                    control.text.text = action.name;

                    var buttonIndex = 0;
                    for (var i = 0; i < action.bindings.Count; i++)
                    {
                        var binding = action.bindings[i];
                        if (binding.isComposite)
                            continue;

                        var button = control.buttons[buttonIndex];
                        button.targetAction = action;
                        button.targetBinding = binding;
                        button.index = i;
                        buttonIndex++;

                        buttons.Add(button);
                    }
                }
                else
                {
                    //button
                    var newTemplate = Instantiate(buttonTemplate);
                    newTemplate.name = action.name;
                    newTemplate.transform.SetParent(transform, false);
                    newTemplate.SetActive(true);
                    var control = newTemplate.GetComponent<RebindControl>();
                    control.text.text = action.name;
                    for (var i = 0; i < control.buttons.Length; i++)
                    {
                        var button = control.buttons[i];
                        button.targetAction = action;
                        button.targetBinding = action.bindings[i];
                        button.index = i;

                        buttons.Add(button);
                    }
                }
            }

            if (map.name == "Player")
                playerSettings.transform.SetAsLastSibling();
        }
    }

    public void SaveRebindings()
    {
        var json = controls.SaveBindingOverridesAsJson();

        if (!InputSystem.file.Exists)
            InputSystem.file.Directory.Create();

        File.WriteAllText(InputSystem.file.FullName, json);
        GlobalController.Instance.controlsJson = json;
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Input Stuff/Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Controls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""83f45c68-31db-4560-9e52-40081d44ab11"",
            ""actions"": [
                {
                    ""name"": ""Hotkey 1"",
                    ""type"": ""Button"",
                    ""id"": ""2cc8a49f-7f75-4d18-8c2a-392896506dca"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 2"",
                    ""type"": ""Button"",
                    ""id"": ""a939588c-4ea6-42b1-a6b7-8ab77d121879"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 3"",
                    ""type"": ""Button"",
                    ""id"": ""86d47edd-f113-4fe0-9ea4-b95c1c4f9f3f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 4"",
                    ""type"": ""Button"",
                    ""id"": ""c9d6c91c-7375-49e6-8089-aedda119a4bd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 5"",
                    ""type"": ""Button"",
                    ""id"": ""9f6dc4f6-f10b-4a9b-9c3f-e58935ba57f1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 6"",
                    ""type"": ""Button"",
                    ""id"": ""9f5c50e3-a711-482a-8d3f-afa3b1c26616"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 7"",
                    ""type"": ""Button"",
                    ""id"": ""783ea7fd-0ab3-4d40-8c45-991c682b7f1a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 8"",
                    ""type"": ""Button"",
                    ""id"": ""5bb1ebb7-de77-4dfa-8c6f-8f250108ccca"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 9"",
                    ""type"": ""Button"",
                    ""id"": ""b8f2fe15-6183-49b5-80e3-d50ee2e34166"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey 0"",
                    ""type"": ""Button"",
                    ""id"": ""25bcec78-ac2b-476a-ad32-332882d8e697"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Debug Screen"",
                    ""type"": ""Button"",
                    ""id"": ""c95eb970-1547-4586-bb3e-73e440084fe8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b0db8945-6306-498e-a5e0-57987103414a"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0aa52007-e67d-4f0d-8f4a-daf62cc754bd"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c3d4980d-6d6d-49db-a013-144c5ee5edb1"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""af7ad6d6-981c-4e0e-b02f-863e295610e7"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0c907b67-19e6-4ebe-89ad-4051060e3f19"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 5"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9245d6b3-203d-4ad1-9175-7a1b0ad728c2"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 6"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""51a97c9d-89a0-473a-bb83-ab50c32aeb28"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 7"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""67df148d-076e-4233-8990-95572208dac0"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 8"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bf3bd799-7c79-47c2-992e-74b4b7c2e083"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 9"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8ab03c6-e582-4977-9cf2-e3d758cc0bb0"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hotkey 0"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5c42ec4a-9701-43a8-a48c-50946c116080"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Debug Screen"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard + Mouse"",
            ""bindingGroup"": ""Keyboard + Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Hotkey1 = m_Player.FindAction("Hotkey 1", throwIfNotFound: true);
        m_Player_Hotkey2 = m_Player.FindAction("Hotkey 2", throwIfNotFound: true);
        m_Player_Hotkey3 = m_Player.FindAction("Hotkey 3", throwIfNotFound: true);
        m_Player_Hotkey4 = m_Player.FindAction("Hotkey 4", throwIfNotFound: true);
        m_Player_Hotkey5 = m_Player.FindAction("Hotkey 5", throwIfNotFound: true);
        m_Player_Hotkey6 = m_Player.FindAction("Hotkey 6", throwIfNotFound: true);
        m_Player_Hotkey7 = m_Player.FindAction("Hotkey 7", throwIfNotFound: true);
        m_Player_Hotkey8 = m_Player.FindAction("Hotkey 8", throwIfNotFound: true);
        m_Player_Hotkey9 = m_Player.FindAction("Hotkey 9", throwIfNotFound: true);
        m_Player_Hotkey0 = m_Player.FindAction("Hotkey 0", throwIfNotFound: true);
        m_Player_DebugScreen = m_Player.FindAction("Debug Screen", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_Hotkey1;
    private readonly InputAction m_Player_Hotkey2;
    private readonly InputAction m_Player_Hotkey3;
    private readonly InputAction m_Player_Hotkey4;
    private readonly InputAction m_Player_Hotkey5;
    private readonly InputAction m_Player_Hotkey6;
    private readonly InputAction m_Player_Hotkey7;
    private readonly InputAction m_Player_Hotkey8;
    private readonly InputAction m_Player_Hotkey9;
    private readonly InputAction m_Player_Hotkey0;
    private readonly InputAction m_Player_DebugScreen;
    public struct PlayerActions
    {
        private @Controls m_Wrapper;
        public PlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Hotkey1 => m_Wrapper.m_Player_Hotkey1;
        public InputAction @Hotkey2 => m_Wrapper.m_Player_Hotkey2;
        public InputAction @Hotkey3 => m_Wrapper.m_Player_Hotkey3;
        public InputAction @Hotkey4 => m_Wrapper.m_Player_Hotkey4;
        public InputAction @Hotkey5 => m_Wrapper.m_Player_Hotkey5;
        public InputAction @Hotkey6 => m_Wrapper.m_Player_Hotkey6;
        public InputAction @Hotkey7 => m_Wrapper.m_Player_Hotkey7;
        public InputAction @Hotkey8 => m_Wrapper.m_Player_Hotkey8;
        public InputAction @Hotkey9 => m_Wrapper.m_Player_Hotkey9;
        public InputAction @Hotkey0 => m_Wrapper.m_Player_Hotkey0;
        public InputAction @DebugScreen => m_Wrapper.m_Player_DebugScreen;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @Hotkey1.started += instance.OnHotkey1;
            @Hotkey1.performed += instance.OnHotkey1;
            @Hotkey1.canceled += instance.OnHotkey1;
            @Hotkey2.started += instance.OnHotkey2;
            @Hotkey2.performed += instance.OnHotkey2;
            @Hotkey2.canceled += instance.OnHotkey2;
            @Hotkey3.started += instance.OnHotkey3;
            @Hotkey3.performed += instance.OnHotkey3;
            @Hotkey3.canceled += instance.OnHotkey3;
            @Hotkey4.started += instance.OnHotkey4;
            @Hotkey4.performed += instance.OnHotkey4;
            @Hotkey4.canceled += instance.OnHotkey4;
            @Hotkey5.started += instance.OnHotkey5;
            @Hotkey5.performed += instance.OnHotkey5;
            @Hotkey5.canceled += instance.OnHotkey5;
            @Hotkey6.started += instance.OnHotkey6;
            @Hotkey6.performed += instance.OnHotkey6;
            @Hotkey6.canceled += instance.OnHotkey6;
            @Hotkey7.started += instance.OnHotkey7;
            @Hotkey7.performed += instance.OnHotkey7;
            @Hotkey7.canceled += instance.OnHotkey7;
            @Hotkey8.started += instance.OnHotkey8;
            @Hotkey8.performed += instance.OnHotkey8;
            @Hotkey8.canceled += instance.OnHotkey8;
            @Hotkey9.started += instance.OnHotkey9;
            @Hotkey9.performed += instance.OnHotkey9;
            @Hotkey9.canceled += instance.OnHotkey9;
            @Hotkey0.started += instance.OnHotkey0;
            @Hotkey0.performed += instance.OnHotkey0;
            @Hotkey0.canceled += instance.OnHotkey0;
            @DebugScreen.started += instance.OnDebugScreen;
            @DebugScreen.performed += instance.OnDebugScreen;
            @DebugScreen.canceled += instance.OnDebugScreen;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @Hotkey1.started -= instance.OnHotkey1;
            @Hotkey1.performed -= instance.OnHotkey1;
            @Hotkey1.canceled -= instance.OnHotkey1;
            @Hotkey2.started -= instance.OnHotkey2;
            @Hotkey2.performed -= instance.OnHotkey2;
            @Hotkey2.canceled -= instance.OnHotkey2;
            @Hotkey3.started -= instance.OnHotkey3;
            @Hotkey3.performed -= instance.OnHotkey3;
            @Hotkey3.canceled -= instance.OnHotkey3;
            @Hotkey4.started -= instance.OnHotkey4;
            @Hotkey4.performed -= instance.OnHotkey4;
            @Hotkey4.canceled -= instance.OnHotkey4;
            @Hotkey5.started -= instance.OnHotkey5;
            @Hotkey5.performed -= instance.OnHotkey5;
            @Hotkey5.canceled -= instance.OnHotkey5;
            @Hotkey6.started -= instance.OnHotkey6;
            @Hotkey6.performed -= instance.OnHotkey6;
            @Hotkey6.canceled -= instance.OnHotkey6;
            @Hotkey7.started -= instance.OnHotkey7;
            @Hotkey7.performed -= instance.OnHotkey7;
            @Hotkey7.canceled -= instance.OnHotkey7;
            @Hotkey8.started -= instance.OnHotkey8;
            @Hotkey8.performed -= instance.OnHotkey8;
            @Hotkey8.canceled -= instance.OnHotkey8;
            @Hotkey9.started -= instance.OnHotkey9;
            @Hotkey9.performed -= instance.OnHotkey9;
            @Hotkey9.canceled -= instance.OnHotkey9;
            @Hotkey0.started -= instance.OnHotkey0;
            @Hotkey0.performed -= instance.OnHotkey0;
            @Hotkey0.canceled -= instance.OnHotkey0;
            @DebugScreen.started -= instance.OnDebugScreen;
            @DebugScreen.performed -= instance.OnDebugScreen;
            @DebugScreen.canceled -= instance.OnDebugScreen;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard + Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnHotkey1(InputAction.CallbackContext context);
        void OnHotkey2(InputAction.CallbackContext context);
        void OnHotkey3(InputAction.CallbackContext context);
        void OnHotkey4(InputAction.CallbackContext context);
        void OnHotkey5(InputAction.CallbackContext context);
        void OnHotkey6(InputAction.CallbackContext context);
        void OnHotkey7(InputAction.CallbackContext context);
        void OnHotkey8(InputAction.CallbackContext context);
        void OnHotkey9(InputAction.CallbackContext context);
        void OnHotkey0(InputAction.CallbackContext context);
        void OnDebugScreen(InputAction.CallbackContext context);
    }
}

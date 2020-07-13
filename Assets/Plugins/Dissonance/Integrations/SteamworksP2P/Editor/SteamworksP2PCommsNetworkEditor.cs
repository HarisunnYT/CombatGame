using Dissonance.Editor;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.SteamworksP2P.Editor
{
    [CustomEditor(typeof(SteamworksP2PCommsNetwork))]
    public class SteamworksNetCommsNetworkEditor
        : BaseDissonnanceCommsNetworkEditor<SteamworksP2PCommsNetwork, SteamworksP2PServer, SteamworksP2PClient, SteamId, SteamId, Unit>
    {
        private int _channelToServer;
        private SerializedProperty _channelToServerProperty;

        private int _channelToClient;
        private SerializedProperty _channelToClientProperty;

        protected void OnEnable()
        {
            _channelToServerProperty = serializedObject.FindProperty("_channelToServer");
            _channelToServer = _channelToServerProperty.intValue;

            _channelToClientProperty = serializedObject.FindProperty("_channelToClient");
            _channelToClient = _channelToClientProperty.intValue;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                _channelToServer = EditorGUILayout.DelayedIntField("Steam P2P nChannel (Server)", _channelToServer);
                if (_channelToServer == 0)
                    EditorGUILayout.HelpBox("nChannel must not be zero", MessageType.Error);
                else if (_channelToServer == _channelToClient)
                    EditorGUILayout.HelpBox("nChannel for client and server must be different", MessageType.Error);
                else
                    _channelToServerProperty.intValue = _channelToServer;

                _channelToClient = EditorGUILayout.DelayedIntField("Steam P2P nChannel (Client)", _channelToClient);
                if (_channelToClient == 0)
                    EditorGUILayout.HelpBox("nChannel must not be zero", MessageType.Error);
                else if (_channelToServer == _channelToClient)
                    EditorGUILayout.HelpBox("nChannel for server and client must be different", MessageType.Error);
                else
                    _channelToClientProperty.intValue = _channelToClient;

                if (GUILayout.Button("Steamworks ISteamNetworking Documentation"))
                    Application.OpenURL("https://partner.steamgames.com/doc/api/ISteamNetworking");

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

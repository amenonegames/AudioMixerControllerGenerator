namespace Amenonegames.GenerageAudioMixerController.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Audio;

    public class GenerateCodeWindow : EditorWindow
    {
        private AudioMixer audioMixer;
        private bool requireAsyncMethod;
        private bool requireInterfaceGeneration;
        
        [MenuItem("Tools/GenerateCode/AudioMixerController")]
        private static void OpenWindow()
        {
            GetWindow<GenerateCodeWindow>("Generate Code");
        }

        private void OnGUI()
        {
            GUILayout.Label("Attach Audio Mixer", EditorStyles.boldLabel);
            audioMixer = (AudioMixer)EditorGUILayout.ObjectField("Audio Mixer", audioMixer, typeof(AudioMixer), false);
            requireAsyncMethod = EditorGUILayout.Toggle("Generate Async Method", requireAsyncMethod);
            requireInterfaceGeneration = EditorGUILayout.Toggle("Generate Interface", requireInterfaceGeneration);
            
            if (GUILayout.Button("Generate Mixer Controller") && audioMixer != null)
            {
                FetchMixerData fetchMixerData = new(audioMixer);
                
                if (fetchMixerData != null)
                {
                    var parameters =fetchMixerData.SyncToMixer();
                    
                    AudioMixerControllerGenerator generator = new(parameters,requireAsyncMethod, requireInterfaceGeneration);
                    generator.Generate();
                    Debug.Log("Complete Generation of AudioMixerController");
                }
                else
                {
                    Debug.LogError("FetchMixerData component not found in the scene.");
                }
            }
        }
    }

}
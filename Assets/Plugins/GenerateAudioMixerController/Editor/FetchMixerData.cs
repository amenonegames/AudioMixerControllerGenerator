
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.Reflection;
using System;
namespace Amenonegames.GenerageAudioMixerController.Editor
{
    public class FetchMixerData 
    {
        private AudioMixer AudMix;
        private AudioMixerGroup[] AudioMixerGroups;
        private AudioMixerSnapshot[] AudioSnapshots;
        private List<string> ExposedParams = new();
        private List<string>[] GroupEffects;
        
        public FetchMixerData(AudioMixer mixer)
        {
            AudMix = mixer;
        }
     
        public string[] SyncToMixer()
        {
            Debug.Log("----Syncing to Mixer---------------------------------------------------------------------");
            //Fetch all audio groups under MASTER
            AudioMixerGroups = AudMix.FindMatchingGroups ("Master");
       
            //Debug.Log("----AudioGroups----------------------------------------------------");
            for (int x = 0; x < AudioMixerGroups.Length; x++) {
                //Debug.Log(AudioMixerGroups[x].name);
            }
            
            GroupEffects = new List<string>[AudioMixerGroups.Length];
            for (int x = 0; x < AudioMixerGroups.Length; x++) {
                Debug.Log("AudioGroup " + AudioMixerGroups[x].name + "---------------------------");
                Debug.Log("----Effects----");
                GroupEffects[x] = new List<string>();
                Array effects = (Array)AudioMixerGroups[x].GetType().GetProperty("effects").GetValue(AudioMixerGroups[x], null);
                for(int i = 0; i< effects.Length; i++)
                {
                    var o = effects.GetValue(i);
                    string effect = (string)o.GetType().GetProperty("effectName").GetValue(o, null);
                    GroupEffects[x].Add(effect);
                    //Debug.Log(effect);
                }
            }
            
            //Exposed Params
            Array parameters = (Array)AudMix.GetType().GetProperty("exposedParameters").GetValue(AudMix, null);
       
            Debug.Log("----ExposedParams----------------------------------------------------");
            for(int i = 0; i< parameters.Length; i++)
            {
                var o = parameters.GetValue(i);
                string Param = (string)o.GetType().GetField("name").GetValue(o);
                ExposedParams.Add(Param);
                Debug.Log(Param);
            }
       
            //Snapshots
            AudioSnapshots = (AudioMixerSnapshot[])AudMix.GetType().GetProperty("snapshots").GetValue(AudMix, null);
       
            //Debug.Log("----Snapshots----------------------------------------------------");
            // for(int i = 0; i< AudioSnapshots.Length; i++)
            // {
            //     Debug.Log(AudioSnapshots[i].name);
            // }
            
            return ExposedParams.ToArray();

        }
    }
     
}
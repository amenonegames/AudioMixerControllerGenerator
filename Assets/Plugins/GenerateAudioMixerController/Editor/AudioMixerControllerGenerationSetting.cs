namespace Amenonegames.GenerageAudioMixerController.Editor
{
    public class AudioMixerControllerGenerationSetting
    {
                
        public bool requireOverrideFiles = true;

        public string GenerateClassImportNameSpace()
        {
            string classImportNameSpace = @$"
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
";

            if (requireAsyncMethod) 
                classImportNameSpace += @$"
using Cysharp.Threading.Tasks;
using DG.Tweening;
";

            if (requireInterfaceGeneration && !string.IsNullOrEmpty(interfaceNameSpace)) 
                classImportNameSpace += @$"
using {interfaceNameSpace};";

            return classImportNameSpace;
        }
        
        public string classNameSpace = "View.Sound";
        public string className = "AudioMixerController";
        public string GenerateClassFilePath() => $"Assets/Scripts/Sound/{className}.cs";
        public string additionalClasVariableDeclaration = @"";

                
        // change exposed param method Name to use method generation and interface generation
        public string GenerateChangeMethodName(string propertyName) => @$"void {propertyName}Change(float value)";
        // change exposed param method sync ver
        public string GenerateChangeMethod(string propertyName)
        {
            var changeMethodName = "public " + GenerateChangeMethodName(propertyName);
            
            return @$"
            {changeMethodName}
            {{
                _audioMixer.SetFloat(""{propertyName}"", value);
            }}
";
        }
        
        // return exposed param to default method Name to use method generation and interface generation
        public string GenerateResetMethodName(string propertyName) => @$"void {propertyName}Reset(float value)";
        // return exposed param to default method sync ver
        public string GenerateResetMethod(string propertyName)
        {
            var resetMethodName = "public " + GenerateResetMethodName(propertyName);
            return @$"
            {resetMethodName}
            {{
                _audioMixer.SetFloat(""{propertyName}"", _original{propertyName});
            }}
";
        }
        
        // change exposed param method Name to use method generation and interface generation
        public string GenerateChangeAsyncMethodName(string propertyName) => @$"UniTask {propertyName}ChangeAsync(float value, CancellationToken token, float duration)";
        // change exposed param method async ver
        public string GenerateChangeMethodAsync(string propertyName)
        {
            var changeMethodName = "public async " +  GenerateChangeAsyncMethodName(propertyName);
            return @$"
            {changeMethodName}
            {{
                await _audioMixer.DOSetFloat(""{propertyName}"", value, duration).WithCancellation(token);
            }}
";
        }
        
        // return exposed param to default method Name to use method generation and interface generation
        public string GenerateResetAsyncMethodName(string propertyName) => @$"UniTask {propertyName}ResetAsync(CancellationToken token, float duration)";
        // change exposed param method async ver
        public string GenerateResetMethodAsync(string propertyName)
        {
            var resetMethodName = "public async " + GenerateResetAsyncMethodName(propertyName);
            return @$"
            {resetMethodName}
            {{
                await _audioMixer.DOSetFloat(""{propertyName}"", _original{propertyName}, duration).WithCancellation(token);
            }}
";
        }

        public string GenerateInterfaceImportNameSpace()
        {
             string interfaceImportNamSpace = @"
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
";

             if (requireAsyncMethod)
                 interfaceImportNamSpace += @$"
using Cysharp.Threading.Tasks;
";

             return interfaceImportNamSpace;
        }
        

        public string interfaceNameSpace = "ViewRoot.Interface";
        public string interfaceName = "IAudioMixerControllable";
        public string GenerateInterfaceFilePath() => $"Assets/Scripts/Sound/Interface/{interfaceName}.cs";

        
        public bool requireAsyncMethod = true;
        public bool requireInterfaceGeneration = true;
        public AudioMixerControllerGenerationSetting(bool requireAsyncMethod, bool requireInterfaceGeneration)
        {
            this.requireAsyncMethod = requireAsyncMethod;
            this.requireInterfaceGeneration = requireInterfaceGeneration;
        }
    }
}
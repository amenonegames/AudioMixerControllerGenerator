using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text;


namespace Amenonegames.GenerageAudioMixerController.Editor
{
    public class AudioMixerControllerGenerator  
    {
        #region Settings
        
        private bool requireOverrideFiles = true;

        private string GenerateClassImportNameSpace()
        {
            string classImportNameSpace = @$"
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
";

            if (requireInterfaceGeneration && !string.IsNullOrEmpty(interfaceNameSpace)) 
                classImportNameSpace += @$"
using {interfaceNameSpace};";

            return classImportNameSpace;
        }
        private static string classNameSpace = "View.Sound";
        private static string className = "AudioMixerController";
        private static string classFilePath = $"Assets/Scripts/Sound/{className}.cs";
        private static string additionalClasVatiableDeclaration = @"";

                
        // change exposed param method Name to use method generation and interface generation
        private string GenerateChangeMethodName(string propertyName) => @$"void {propertyName}Change(float value)";
        // change exposed param method sync ver
        private string GenerateChangeMethod(string propertyName)
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
        private string GenerateResetMethodName(string propertyName) => @$"void {propertyName}Reset(float value)";
        // return exposed param to default method sync ver
        private string GenerateResetMethod(string propertyName)
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
        private string GenerateChangeAsyncMethodName(string propertyName) => @$"UniTask {propertyName}ChangeAsync(float value, CancellationToken token, float duration)";
        // change exposed param method async ver
        private string GenerateChangeMethodAsync(string propertyName)
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
        private string GenerateResetAsyncMethodName(string propertyName) => @$"UniTask {propertyName}ResetAsync(CancellationToken token, float duration)";
        // change exposed param method async ver
        private string GenerateResetMethodAsync(string propertyName)
        {
            var resetMethodName = "public async " + GenerateResetAsyncMethodName(propertyName);
            return @$"
            {resetMethodName}
            {{
                await _audioMixer.DOSetFloat(""{propertyName}"", _original{propertyName}, duration).WithCancellation(token);
            }}
";
        }
        
        private string interfaceImportNamSpace = @"
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
";
        private static string interfaceNameSpace = "ViewRoot.Interface";
        private static string interfaceName = "IAudioMixerControllable";
        private static string interfaceFilePath = $"Assets/Scripts/Sound/Interface/{interfaceName}.cs";
        
        #endregion
        
        
        #region GenerateMethods
        
        private bool requireAsyncMethod = true;
        private bool requireInterfaceGeneration = true;
        private string[] _properties;
        public AudioMixerControllerGenerator(string[] properties,bool requireAsyncMethod, bool requireInterfaceGeneration)
        {
            _properties = properties;
            this.requireAsyncMethod = requireAsyncMethod;
            this.requireInterfaceGeneration = requireInterfaceGeneration;
        }

        public void Generate()
        {
            GenerateClassAndInterface();
        }
        
        private void GenerateClassAndInterface()
        {
            string thisClassName = "";
            var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
            if (declaringType != null)
            {
                thisClassName = declaringType?.Name;
            }

            // Create Directory if not exists
            string directoryPath = "";
            directoryPath = Path.GetDirectoryName(classFilePath);
            if (!Directory.Exists(directoryPath))
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);
            directoryPath = Path.GetDirectoryName(interfaceFilePath);
            // Create Directory if not exists
            if (!Directory.Exists(directoryPath))
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);
            
            
            // if override files is not required, generate unique path.
            // if file is already exists, add number to the end of the file name.
            string classFilePathInMethod = classFilePath;
            if(!requireOverrideFiles)
                classFilePathInMethod = AssetDatabase.GenerateUniqueAssetPath(classFilePath);
            
            // if override files is not required, generate unique path.
            // if file is already exists, add number to the end of the file name.
            string interfaceFilePathInMethod = interfaceFilePath;
            if(!requireOverrideFiles)
                interfaceFilePathInMethod = AssetDatabase.GenerateUniqueAssetPath(interfaceFilePath);
            
            var classCode = GenerateClassString(className, interfaceName, thisClassName);
            File.WriteAllText(classFilePathInMethod, classCode);

            if (requireInterfaceGeneration)
            {
                var interfaceCode = GenerateInterfaceString(interfaceName, thisClassName);
                File.WriteAllText(interfaceFilePathInMethod, interfaceCode);
            }
            
            // refresh UnityEditor
            AssetDatabase.Refresh();
        }

        private string GenerateClassString(string className, string interfaceName, string thisClassName)
        {
            StringBuilder code = new();
            string classImportNameSpace = GenerateClassImportNameSpace();

            code.Append(
                @$"
{classImportNameSpace}");

            if(!string.IsNullOrEmpty(classNameSpace))
            {
                code.Append(@$"
namespace {classNameSpace}
{{
");
            }
            
            code.Append(
                @$"
        /// <summary>
        /// This class is generated by script : {thisClassName}
        /// </summary>
        public class {className}");
            
            if(requireInterfaceGeneration)
                code.Append($":{interfaceName}");

            code.Append(
                @$"
        {{
            private readonly AudioMixer _audioMixer;
"
            );

            foreach (var property in _properties)
            {
                code.Append
                (
                    @$"
            private readonly float _original{property};
"
                );
            }

            code.Append
            (
                @$"
            {additionalClasVatiableDeclaration}

            public {className}(AudioMixer audioMixer)
            {{
                _audioMixer = audioMixer;
"
            );

            foreach (var property in _properties)
            {
                code.Append
                (@$"
                _audioMixer.GetFloat(""{property}"", out _original{property});
"
                );
            }

            code.Append(@"

            }"
            );

            foreach (var property in _properties)
            {

                var changeMethodSync = GenerateChangeMethod(property);
                var resetMethodSync = GenerateResetMethod(property);
                
                code.Append(changeMethodSync);
                code.Append(resetMethodSync);
                
                if (!requireAsyncMethod) continue;
                var changeMethodAsync = GenerateChangeMethodAsync(property);
                var resetMethodAsync = GenerateResetMethodAsync(property);
                
                code.Append(changeMethodAsync);
                code.Append(resetMethodAsync);

            }

            code.Append(@"
        }");
            
            if (!string.IsNullOrEmpty(classNameSpace))
            {
                code.Append(@"
}");
            }

            return code.ToString();
        }

        private string GenerateInterfaceString(string interfaceName, string thisClassName)
        {
            // @""とすることで、複数行を書ける
            // ただ「"」は「""」として書きます
            StringBuilder code = new();

            code.Append(
                @$"
{interfaceImportNamSpace}
");
    
                if(!string.IsNullOrEmpty(interfaceNameSpace))
                {
                    code.Append(@$"
namespace {interfaceNameSpace}
{{
");
                }
            
            code.Append(
                @$"
        /// <summary>
        /// This class is generated by script : {thisClassName}
        /// </summary>
        public interface {interfaceName}
        {{
"
            );

            foreach (var property in _properties)
            {
                var changeMethodName = GenerateChangeMethodName(property);
                var resetMethodName = GenerateResetMethodName(property);

                code.Append(@"
                ");
                code.Append(changeMethodName);
                code.Append(@";
");
                
                code.Append(@"
                ");
                code.Append(resetMethodName);
                code.Append(@";
");
                
                if (!requireAsyncMethod) continue;
                var changeAsyncMethodName = GenerateChangeAsyncMethodName(property);
                var resetAsyncMethodName = GenerateResetAsyncMethodName(property);
                
                code.Append(changeAsyncMethodName);
                code.Append(@";
");
                code.Append(resetAsyncMethodName);
                code.Append(@";
");

            }

            code.Append(@"
        }");

            if (!string.IsNullOrEmpty(interfaceNameSpace))
            {
                code.Append(@"
}");
            }

            return code.ToString();
        }
        
#endregion
        
    }

}

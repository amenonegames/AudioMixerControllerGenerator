﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text;


namespace Amenonegames.GenerageAudioMixerController.Editor
{
    public class AudioMixerControllerGenerator
    {
        private AudioMixerControllerGenerationSetting settings;

        private string[] _properties;
        public AudioMixerControllerGenerator(string[] properties,bool requireAsyncMethod, bool requireInterfaceGeneration)
        {
            _properties = properties;
            settings = new AudioMixerControllerGenerationSetting(requireAsyncMethod, requireInterfaceGeneration);
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

            var classFilePath = settings.GenerateClassFilePath();
            
            // Create Directory if not exists
            string directoryPath = "";
            directoryPath = Path.GetDirectoryName(classFilePath);
            if (!Directory.Exists(directoryPath))
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);
            directoryPath = Path.GetDirectoryName(classFilePath);
            // Create Directory if not exists
            if (!Directory.Exists(directoryPath))
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);
            
            
            // if override files is not required, generate unique path.
            // if file is already exists, add number to the end of the file name.
            string classFilePathInMethod = classFilePath;
            if(!settings.requireOverrideFiles)
                classFilePathInMethod = AssetDatabase.GenerateUniqueAssetPath(classFilePath);

            var interfaceFilePath = settings.GenerateInterfaceFilePath();
            
            // if override files is not required, generate unique path.
            // if file is already exists, add number to the end of the file name.
            string interfaceFilePathInMethod = interfaceFilePath;
            if(!settings.requireOverrideFiles)
                interfaceFilePathInMethod = AssetDatabase.GenerateUniqueAssetPath(interfaceFilePath);
            
            var classCode = GenerateClassString(settings.className, settings.interfaceName, thisClassName);
            File.WriteAllText(classFilePathInMethod, classCode);

            if (settings.requireInterfaceGeneration)
            {
                var interfaceCode = GenerateInterfaceString(settings.interfaceName, thisClassName);
                File.WriteAllText(interfaceFilePathInMethod, interfaceCode);
            }
            
            // refresh UnityEditor
            AssetDatabase.Refresh();
        }

        private string GenerateClassString(string className, string interfaceName, string thisClassName)
        {
            StringBuilder code = new();
            string classImportNameSpace = settings.GenerateClassImportNameSpace();

            code.Append(
                @$"
{classImportNameSpace}");

            if(!string.IsNullOrEmpty(settings.classNameSpace))
            {
                code.Append(@$"
namespace {settings.classNameSpace}
{{
");
            }
            
            code.Append(
                @$"
        /// <summary>
        /// This class is generated by script : {thisClassName}
        /// </summary>
        public class {className}");
            
            if(settings.requireInterfaceGeneration)
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
            {settings.additionalClasVariableDeclaration}

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

                var changeMethodSync = settings.GenerateChangeMethod(property);
                var resetMethodSync = settings.GenerateResetMethod(property);
                
                code.Append(changeMethodSync);
                code.Append(resetMethodSync);
                
                if (!settings.requireAsyncMethod) continue;
                var changeMethodAsync = settings.GenerateChangeMethodAsync(property);
                var resetMethodAsync = settings.GenerateResetMethodAsync(property);
                
                code.Append(changeMethodAsync);
                code.Append(resetMethodAsync);

            }

            code.Append(@"
        }");
            
            if (!string.IsNullOrEmpty(settings.classNameSpace))
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
{settings.GenerateInterfaceImportNameSpace()}
");
    
                if(!string.IsNullOrEmpty(settings.interfaceNameSpace))
                {
                    code.Append(@$"
namespace {settings.interfaceNameSpace}
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
                var changeMethodName = settings.GenerateChangeMethodName(property);
                var resetMethodName = settings.GenerateResetMethodName(property);

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
                
                if (!settings.requireAsyncMethod) continue;
                var changeAsyncMethodName = settings.GenerateChangeAsyncMethodName(property);
                var resetAsyncMethodName = settings.GenerateResetAsyncMethodName(property);
                
                code.Append(changeAsyncMethodName);
                code.Append(@";
");
                code.Append(resetAsyncMethodName);
                code.Append(@";
");

            }

            code.Append(@"
        }");

            if (!string.IsNullOrEmpty(settings.interfaceNameSpace))
            {
                code.Append(@"
}");
            }

            return code.ToString();
        }
        

        
    }

}

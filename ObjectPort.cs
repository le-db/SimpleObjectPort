using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SimpleObjectPort
{
    public static class ObjectPort
    {
        /// <summary>
        /// Save a scriptable object to a physical file.<br />
        /// This method can fail by exception
        /// </summary>
        /// <param name="obj">data to save</param>
        /// <param name="saveDir">directory to save in</param>
        /// <param name="openFolderAfterSave">whether to open the system file explorer after saving</param>
        /// <returns>absolute filepath to saved file</returns>
        public static string Save(ScriptableObject obj, string saveDir = "", bool openFolderAfterSave = true)
        {
            var fileDirectory = saveDir;
            var fileName = obj.name;
            var usedFilePath = Save(
                obj,
                fileDirectory,
                fileName,
                useCachePath: true);

            if (openFolderAfterSave && !string.IsNullOrEmpty(usedFilePath))
                System.Diagnostics.Process.Start(Path.GetDirectoryName(usedFilePath));
            return usedFilePath;
        }

        /// <summary>
        /// Tries to load the data inside physical file to a new ScriptableObject instance.<br />
        /// </summary>
        /// <typeparam name="T">type of the ScriptableObject to instantiate</typeparam>
        /// <param name="filePath">filepath to physical file containing loadable data</param>
        /// <returns>ScriptableObject reference, or null on error</returns>
        public static ScriptableObject Load<T>(string filePath)
            where T : ScriptableObject
        {
            return null;
        }

        /// <summary>
        /// Uses EditorJsonUtility.ToJson() to save data to a physical file if in the editor.<br />
        /// Else uses JsonUtility.ToJson() to save the object (excluding references).<br />
        /// This method can fail by exception
        /// </summary>
        /// <typeparam name="T">type of the data</typeparam>
        /// <param name="data">data to save</param>
        /// <param name="saveDir">directory to save in</param>
        /// <param name="fileName">filename of to be saved file</param>
        /// <param name="useCachePath">whether to prefix saveDir with Application.temporaryCachePath</param>
        /// <returns>absolute filepath to saved file</returns>
        public static string Save<T>(T data, string saveDir, string fileName, bool useCachePath = true)
        {
            var usedFileDir = useCachePath
                ? Path.Combine(Application.temporaryCachePath, saveDir)
                : saveDir;
            var usedFilePath = Path.Combine(usedFileDir, fileName);

            var dirName = Path.GetDirectoryName(usedFilePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            bool prettyPrint = true;
            var jsonData
#if UNITY_EDITOR
                = EditorJsonUtility.ToJson(data, prettyPrint);
#else
                = JsonUtility.ToJson(data, prettyPrint);
#endif
            var jsonByte = Encoding.UTF8.GetBytes(jsonData);
            File.WriteAllBytes(usedFilePath, jsonByte);
            return usedFilePath;
        }
    }
}

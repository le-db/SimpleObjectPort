using Cysharp.Threading.Tasks;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleObjectPort
{
    public static class ObjectPort
    {
        /// <summary>
        /// Async loads the data from physical file.<br />
        /// This method can fail by exception.
        /// </summary>
        /// <param name="filePath">filepath to physical file containing loadable data</param>
        /// <returns>read bytes as UTF8 string</returns>
        public static async UniTask<string> Load(string filePath)
        {
            var jsonByte = await File.ReadAllBytesAsync(filePath).AsUniTask();
            return Encoding.UTF8.GetString(jsonByte);
        }

        /// <summary>
        /// Uses EditorJsonUtility.ToJson() to async save data to a physical file if in the editor.<br />
        /// Else uses JsonUtility.ToJson() to async save the object.<br />
        /// This method can fail by exception.
        /// </summary>
        /// <param name="obj">data to save</param>
        /// <param name="saveDir">directory to save in</param>
        /// <param name="openFolderAfterSave">whether to open the system file explorer after saving</param>
        /// <returns>absolute filepath to saved file</returns>
        public static async UniTask<string> Save(UnityEngine.Object obj, string saveDir = "", bool openFolderAfterSave = true)
        {
            var fileName = $"{obj.name}.json";
            var usedFilePath = await Save(
                obj,
                saveDir,
                fileName,
                useCachePath: true);

            if (openFolderAfterSave && !string.IsNullOrEmpty(usedFilePath))
                System.Diagnostics.Process.Start(Path.GetDirectoryName(usedFilePath));
            return usedFilePath;
        }

        /// <summary>
        /// Uses EditorJsonUtility.ToJson() to async save data to a physical file if in the editor.<br />
        /// Else uses JsonUtility.ToJson() to async save the object.<br />
        /// This method can fail by exception.
        /// </summary>
        /// <typeparam name="T">type of the data</typeparam>
        /// <param name="data">data to save</param>
        /// <param name="saveDir">directory to save in</param>
        /// <param name="fileName">filename of to be saved file</param>
        /// <param name="useCachePath">whether to prefix saveDir with Application.temporaryCachePath</param>
        /// <returns>absolute filepath to saved file</returns>
        public static async UniTask<string> Save<T>(T data, string saveDir, string fileName, bool useCachePath = true)
        {
            var usedFileDir = useCachePath
                ? Path.Combine(UnityEngine.Application.temporaryCachePath, saveDir)
                : saveDir;
            var usedFilePath = Path.Combine(usedFileDir, fileName);

            var dirName = Path.GetDirectoryName(usedFilePath);
            if (!Directory.Exists(dirName))
                throw new System.ArgumentException($"{dirName} does not exist");

            bool prettyPrint = true;
            var jsonData
#if UNITY_EDITOR
                = EditorJsonUtility.ToJson(data, prettyPrint);
#else
                = UnityEngine.JsonUtility.ToJson(data, prettyPrint);
#endif
            var jsonByte = Encoding.UTF8.GetBytes(jsonData);
            await File.WriteAllBytesAsync(usedFilePath, jsonByte).AsUniTask();
            return usedFilePath;
        }
    }
}

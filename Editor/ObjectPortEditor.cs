using Cysharp.Threading.Tasks;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleObjectPort.Editor
{
    public class ObjectPortEditor : EditorWindow
    {
        Button exportButton;
        Toggle explorerOnExportToggle;
        Toggle useJsonAssetNameToggle;
        TextField customTargetPathField;

        Object objectValue;

        [MenuItem("Window/SimpleObjectPort/ObjectPort")]
        public static void Entry()
        {
            var window = GetWindow<ObjectPortEditor>();
            window.titleContent = new GUIContent("ObjectPort");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            #region export

            CreateUIElement<Label>(root, (label)
                => label.text = "\n<b>Export</b>\n");

            CreateUIElement<ObjectField>(root, field =>
            {
                field.label = "Object";
                field.RegisterValueChangedCallback(HandleObjectSelection);
            });

            explorerOnExportToggle = CreateUIElement<Toggle>(root, toggle =>
            {
                toggle.label = "Open system explorer after export";
                toggle.value = true;
            });

            exportButton = CreateUIElement<Button>(root, button =>
            {
                button.text = "Export";
                button.clicked += () => _ = HandleExportAsync();
                if (objectValue == null)
                    button.SetEnabled(false);
            });

            #endregion export


            #region import

            CreateUIElement<Label>(root, (label)
                => label.text = "\n<b>Import</b>\n");

            useJsonAssetNameToggle = CreateUIElement<Toggle>(root, toggle =>
            {
                toggle.label = "Use m_Name inside JSON as asset name";
                toggle.value = true;
            });

            customTargetPathField = CreateTextField(root, "Custom asset path");

            CreateUIElement<Button>(root, button =>
            {
                button.text = "Import";
                button.clicked += () => _ = HandleImportAsync();
            });

            #endregion import
        }

        async UniTask HandleExportAsync()
        {
            if (!EditorUtility.DisplayDialog("Export Object", "This will create a file and open the explorer. Continue?", "Yes", "No"))
                return;

            bool openFolderAfterSave = explorerOnExportToggle.value;

            var savedFilePath = "";
            try
            {
                savedFilePath = await ObjectPort.Save(objectValue, openFolderAfterSave: openFolderAfterSave);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not save file");
                Debug.LogException(e);
            }

            if (openFolderAfterSave && !string.IsNullOrEmpty(savedFilePath))
                System.Diagnostics.Process.Start(Path.GetDirectoryName(savedFilePath));
        }

        async UniTask HandleImportAsync()
        {
            if (!EditorUtility.DisplayDialog(
                "Import Object",
                "This will prompt you to select a previously exported file and will then create an asset in the Asset/ directory. Continue?",
                "Yes", "No"))
            {
                return;
            }

            var filePath = EditorUtility.OpenFilePanelWithFilters(
                "Import Object",
                directory: "",
                new string[] { "JSON", "json", "All files", "*"});

            string jsonContent;
            JToken jsonRoot;
            try
            {
                jsonContent = await ObjectPort.Load(filePath);
                jsonRoot = jsonContent.Contains("MonoBehaviour")
                    ? JObject.Parse(jsonContent)["MonoBehaviour"]
                    : JObject.Parse(jsonContent).Root;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Could not load selected file");
                Debug.LogException(e);
                return;
            }

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogWarning("Read file was empty");
                return;
            }

            var className = jsonRoot["m_EditorClassIdentifier"].ToString()
                .Split('.').Last();
            if (string.IsNullOrEmpty(className))
            {
                Debug.LogWarning("Missing classname in read file");
                return;
            }
            var objectToOverwrite = ScriptableObject.CreateInstance(className);

            // this is what actually sets the data inside the object
            EditorJsonUtility.FromJsonOverwrite(jsonContent, objectToOverwrite);

            var fileName = useJsonAssetNameToggle.value
                ? jsonRoot["m_Name"].ToString()
                : Path.GetFileName(filePath);
            var targetPath = Path.Combine("Assets", customTargetPathField.value, $"{fileName}.asset");
            AssetDatabase.CreateAsset(objectToOverwrite, targetPath);
        }

        void HandleObjectSelection(ChangeEvent<Object> selectedEvent)
        {
            objectValue = selectedEvent.newValue;
            if (objectValue == null)
            {
                exportButton.SetEnabled(false);
                return;
            }
            exportButton.SetEnabled(true);
        }

        T CreateUIElement<T>(VisualElement root, System.Action<T> callback = null)
            where T : VisualElement, new()
        {
            var newElement = new T();
            callback?.Invoke(newElement);
            root.Add(newElement);
            return newElement;
        }

        TextField CreateTextField(VisualElement root, string label)
        {
            // text field constructor with label, setting field.label has no effect
            var textField = new TextField(label);
            root.Add(textField);
            return textField;
        }
    }
}

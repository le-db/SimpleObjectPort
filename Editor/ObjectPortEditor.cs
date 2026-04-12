using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleObjectPort.Editor
{
    public class ObjectPortEditor : EditorWindow
    {
        ObjectField objectField;
        Button exportButton;
        Button importButton;
        Toggle explorerOnExportToggle;

        ScriptableObject objectValue;

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

            objectField = CreateUIElement<ObjectField>(root, field =>
            {
                field.label = "ScriptableObject\n";
                field.objectType = typeof(ScriptableObject);
                objectField.RegisterValueChangedCallback(HandleObjectSelection);
            });

            explorerOnExportToggle = CreateUIElement<Toggle>(root, toggle =>
            {
                toggle.label = "Open system explorer after export";
                //toggle.name = "toggleExplorer";
                toggle.value = true;
            });

            exportButton = CreateUIElement<Button>(root, button =>
            {
                button.text = "Export";
                //button.name = "buttonExport";
                button.clicked += ExportObject;
                if (objectValue == null)
                    button.SetEnabled(false);
            });

            #endregion export


            #region import

            CreateUIElement<Label>(root, (label)
                => label.text = "\n<b>Import</b>\n");

            importButton = CreateUIElement<Button>(root, button =>
            {
                button.text = "Import";
                //button.name = "buttonImport";
                button.clicked += ImportObject;
                //if (objectValue == null)
                //    button.SetEnabled(false);
            });

            #endregion import
        }

        T CreateUIElement<T>(VisualElement root, System.Action<T> callback = null)
            where T : VisualElement, new()
        {
            var newElement = new T();
            callback?.Invoke(newElement);
            root.Add(newElement);
            return newElement;
        }

        void HandleObjectSelection(ChangeEvent<Object> selectedEvent)
        {
            try
            {
                objectValue = (ScriptableObject)selectedEvent.newValue;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not cast selected objectField to ScriptableObject.");
                Debug.LogException(ex);
                return;
            }

            if (objectValue == null)
            {
                exportButton.SetEnabled(false);
                return;
            }
            exportButton.SetEnabled(true);
        }

        void ExportObject()
        {
            if (!EditorUtility.DisplayDialog("Export Object", "This will create a file and open the explorer. Continue?", "Yes", "No"))
                return;

            var savedFilePath = "";
            bool openFolderAfterSave = explorerOnExportToggle.value;
            try
            {
                savedFilePath = ObjectPort.Save(objectValue, openFolderAfterSave: openFolderAfterSave);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to save file");
                Debug.LogWarning(e.Message);
            }

            if (openFolderAfterSave && !string.IsNullOrEmpty(savedFilePath))
                System.Diagnostics.Process.Start(Path.GetDirectoryName(savedFilePath));
        }

        void ImportObject()
        {
            if (!EditorUtility.DisplayDialog("Import Object", "This will prompt you to select a file and will then create an asset. Continue?", "Yes", "No"))
                return;

            var filePath = EditorUtility.OpenFilePanelWithFilters(
                "Import Object",
                directory: "",
                new string[] { "JSON", "json", "All files", "*"});

            // TODO
        }
    }
}

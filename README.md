# SimpleObjectPort

A simple implementation of an object serialization system for Unity3D. Serializes Unity objects to JSON files using EditorJsonUtility (in editor) or JsonUtility (runtime). Includes a custom editor window for exporting and importing ScriptableObject assets via GUI.

Last tested on Unity 6.4

## Dependencies
- [UniTask](https://github.com/cysharp/UniTask)

## Usage

```C#
var savedPath = await ObjectPort.Save(object);
var json = await ObjectPort.Load(savedPath);
```

Or simply use the editor: Window > SimpleObjectPort > ObjectPort

## Thoughts
- exception handling is the responsibility of the caller
- ObjectPortEditor.HandleImportAsync() uses ScriptableObject.CreateInstance(className)
	- loading non-SOs in the custom editor will not work
- loading asset or object references at runtime will not work
	- if you have to, create a lookup table
- you don't have to use ScriptableObject like the custom editor, any serializable C# class will work
	- probably a good idea to save or load only value types ?

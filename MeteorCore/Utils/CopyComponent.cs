using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace MeteorCore.Utils;

public static partial class MeteorUtils {
    // https://discussions.unity.com/t/copy-a-component-at-runtime/71172
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        if(!dst)
            dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach(var field in fields) {
            if(field.IsStatic)
                continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach(var prop in props) {
            if(!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
                continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

    /// <summary>
    /// Copies fields from original to destination.
    /// Also copies if the propery is settable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <param name="destination"></param>
    /// <param name="copyPrivate">If true copies private members only if they are in the provided type (ie. not in parent classes)</param>
    /// <param name="ignoreFields"></param>
    /// <param name="ignoreProperties"></param>
    /// <returns></returns>
    public static T CopyComponentValues<T>(T original, T destination, bool copyPrivate, ICollection<string> ignoreFields = null, ICollection<string> ignoreProperties = null) where T : Component {
        if(destination == null) {
            Plugin.Logger.LogError("CopyComponent destination is null");
        }
        if(original == null) {
            Plugin.Logger.LogError("CopyComponent original is null");
        }

        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        if(copyPrivate)
            flags |= BindingFlags.NonPublic;

        System.Type type = typeof(T);
        var dst = destination;
        var fields = type.GetFields(flags);
        foreach(var field in fields) {
            try {
                if(field.IsStatic)
                    continue;
                if(ignoreFields != null && ignoreFields.Contains(field.Name))
                    continue;
                field.SetValue(dst, field.GetValue(original));
            } catch(System.Exception e) {
                Plugin.Logger.LogError($"Could not copy field \"{field.Name}\" from \"{original.GetType().Name}\" to \"{destination.GetType().Name}\"");
                Plugin.Logger.LogError(e);
                throw e;
            }
        }
        var props = type.GetProperties(flags);
        foreach(var prop in props) {
            try {
                if(!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
                    continue;
                if(ignoreProperties != null && ignoreProperties.Contains(prop.Name))
                    continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            } catch(System.Exception e) {
                Plugin.Logger.LogError($"Could not copy property {prop.Name} from {original.GetType().Name} to {destination.GetType().Name}");
                Plugin.Logger.LogError(e);
                throw e;
            }
        }
        return dst as T;
    }


    /// <summary>
    /// Copies fields from original to destination.
    /// Also copies if the propery is settable.
    /// </summary>
    /// <typeparam name="T">Type of the component to copy</typeparam>
    /// <param name="original">Original component to copy from</param>
    /// <param name="destination">Destination component to copy to</param>
    /// <param name="deep">If true, copies all members from the original to the destination, else only copies members from the T type not from parent classes</param>
    /// <returns>Destination component</returns>
    /*public static T CopyComponentValues<T>(T original, T destination, bool deep) where T : Component {
        var flags = BindingFlags.Public | BindingFlags.Instance;
        if (!deep) {
            flags |= BindingFlags.DeclaredOnly;
        }
        System.Type type = typeof(T);
        var fields = type.GetFields(flags);
        for (int i = 0; i < fields.Length; i++){
            try {
            fields[i].SetValue(destination, fields[i].GetValue(original));
            } catch(System.Exception e) {
                Plugin.Logger.LogError($"Could not copy field {fields[i].Name}");
                Plugin.Logger.LogError(e);
            }
        }

        var properties = type.GetProperties(flags);
        for (int i = 0; i < properties.Length; i++){
            if (properties[i].CanWrite && properties[i].CanRead) {
                try {
                    properties[i].SetValue(destination, properties[i].GetValue(original));
                } catch(System.Exception e) {
                    Plugin.Logger.LogError($"Could not copy property {properties[i].Name}");
                    Plugin.Logger.LogError(e);
                }
            }
        }
        return destination;
    }*/
}
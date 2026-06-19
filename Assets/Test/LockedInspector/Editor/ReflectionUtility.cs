using System;
using System.Reflection;
using UnityEditor;

public static class ReflectionUtility
{
    public static FieldInfo GetFieldInfo(SerializedProperty property)
    {
        Type type = property.serializedObject.targetObject.GetType();
        FieldInfo field = null;

        string[] parts = property.propertyPath.Split('.');

        foreach (string part in parts)
        {
            if (part == "Array")
                continue;

            if (part.StartsWith("data["))
                continue;

            field = type.GetField(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
                return null;

            type = field.FieldType;

            if (type.IsArray)
                type = type.GetElementType();

            if (type.IsGenericType)
            {
                Type[] args = type.GetGenericArguments();
                if (args.Length == 1)
                    type = args[0];
            }
        }

        return field;
    }
}
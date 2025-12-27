using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Helpers
{
    [Serializable]
    public class UDictionary
    {
        public class SplitAttribute : PropertyAttribute
        {
            public float Key { get; protected set; }
            public float Value { get; protected set; }

            public SplitAttribute(float key, float value)
            {
                Key = key;
                Value = value;
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SplitAttribute), true)]
        [CustomPropertyDrawer(typeof(UDictionary), true)]
        public class Drawer : PropertyDrawer
        {
            SerializedProperty property;

            bool IsExpanded
            {
                get => property.isExpanded;
                set => property.isExpanded = value;
            }

            SerializedProperty keys;
            SerializedProperty values;

            bool IsAligned => keys.arraySize == values.arraySize;

            ReorderableList list;
            GUIContent label;
            SplitAttribute split;

            float KeySplit => split?.Key ?? 30f;
            float ValueSplit => split?.Value ?? 70f;

            const float ElementHeightPadding = 6f;
            const float ElementSpacing = 10f;
            const float TopPadding = 5f;
            const float BottomPadding = 5f;

            void Init(SerializedProperty value)
            {
                if (SerializedProperty.EqualContents(value, property)) return;

                property = value;

                keys = property.FindPropertyRelative(nameof(keys));
                // We now look for the wrapped list
                values = property.FindPropertyRelative("values"); 

                split = attribute as SplitAttribute;

                list = new ReorderableList(property.serializedObject, keys, true, true, true, true)
                {
                    drawHeaderCallback = DrawHeader,
                    onAddCallback = Add,
                    onRemoveCallback = Remove,
                    elementHeightCallback = GetElementHeight,
                    drawElementCallback = DrawElement
                };

                list.onReorderCallbackWithDetails += Reorder;
            }

            public override float GetPropertyHeight(SerializedProperty p, GUIContent l)
            {
                Init(p);

                float height = TopPadding + BottomPadding;

                if (IsAligned)
                    height += IsExpanded ? list.GetHeight() : list.headerHeight;
                else
                    height += EditorGUIUtility.singleLineHeight;

                return height;
            }

            public override void OnGUI(Rect rect, SerializedProperty p, GUIContent l)
            {
                l.text = $" {l.text}";
                label = l;

                Init(p);

                rect = EditorGUI.IndentedRect(rect);
                rect.y += TopPadding;
                rect.height -= TopPadding + BottomPadding;

                if (!IsAligned)
                {
                    DrawAlignmentWarning(ref rect);
                    return;
                }

                if (IsExpanded)
                    DrawList(ref rect);
                else
                    DrawCompleteHeader(ref rect);
            }

            void DrawList(ref Rect rect)
            {
                EditorGUIUtility.labelWidth = 80f;
                EditorGUIUtility.fieldWidth = 80f;
                list.DoList(rect);
            }

            void DrawAlignmentWarning(ref Rect rect)
            {
                float width = 80f;
                float spacing = 5f;

                rect.width -= width;
                EditorGUI.HelpBox(rect, "  Misalignment Detected", MessageType.Error);

                rect.x += rect.width + spacing;
                rect.width = width - spacing;

                if (GUI.Button(rect, "Fix"))
                {
                    if (keys.arraySize > values.arraySize)
                    {
                        int difference = keys.arraySize - values.arraySize;
                        for (int i = 0; i < difference; i++)
                            keys.DeleteArrayElementAtIndex(keys.arraySize - 1);
                    }
                    else if (keys.arraySize < values.arraySize)
                    {
                        int difference = values.arraySize - keys.arraySize;
                        for (int i = 0; i < difference; i++)
                            values.DeleteArrayElementAtIndex(values.arraySize - 1);
                    }
                }
            }

            #region Draw Header
            void DrawHeader(Rect rect)
            {
                rect.x += 10f;
                IsExpanded = EditorGUI.Foldout(rect, IsExpanded, label, true);
            }

            void DrawCompleteHeader(ref Rect rect)
            {
                ReorderableList.defaultBehaviours.DrawHeaderBackground(rect);
                rect.x += 6;
                DrawHeader(rect);
            }
            #endregion

            float GetElementHeight(int index)
            {
                if (index >= keys.arraySize || index >= values.arraySize) return EditorGUIUtility.singleLineHeight;

                SerializedProperty key = keys.GetArrayElementAtIndex(index);
                
                // Get Wrapper, then get the actual value 'v' inside it
                SerializedProperty valueWrapper = values.GetArrayElementAtIndex(index);
                SerializedProperty value = valueWrapper.FindPropertyRelative("v");

                float kHeight = EditorGUI.GetPropertyHeight(key, true);
                float vHeight = EditorGUI.GetPropertyHeight(value, true);

                float max = Mathf.Max(kHeight, vHeight);

                return max + ElementHeightPadding;
            }

            #region Draw Element
            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index >= keys.arraySize || index >= values.arraySize) return;

                rect.height -= ElementHeightPadding;
                rect.y += ElementHeightPadding / 2;

                Rect[] areas = Split(rect, KeySplit, ValueSplit);

                DrawKey(areas[0], index);
                DrawValue(areas[1], index);
            }

            void DrawKey(Rect rect, int index)
            {
                SerializedProperty p = keys.GetArrayElementAtIndex(index);
                rect.x += ElementSpacing / 2f;
                rect.width -= ElementSpacing;
                DrawField(rect, p);
            }

            void DrawValue(Rect rect, int index)
            {
                // Access the internal wrapper, then the 'v' field
                SerializedProperty wrapper = values.GetArrayElementAtIndex(index);
                SerializedProperty p = wrapper.FindPropertyRelative("v");

                rect.x += ElementSpacing / 2f;
                rect.width -= ElementSpacing;
                DrawField(rect, p);
            }

            void DrawField(Rect rect, SerializedProperty p)
            {
                EditorGUI.PropertyField(rect, p, GUIContent.none, true);
            }
            #endregion

            void Reorder(ReorderableList l, int oldIndex, int newIndex)
            {
                values.MoveArrayElement(oldIndex, newIndex);
            }

            void Add(ReorderableList l)
            {
                int index = keys.arraySize;
                keys.InsertArrayElementAtIndex(index);
                values.InsertArrayElementAtIndex(index);
            }

            void Remove(ReorderableList l)
            {
                keys.DeleteArrayElementAtIndex(l.index);
                values.DeleteArrayElementAtIndex(l.index);
            }

            static Rect[] Split(Rect source, params float[] cuts)
            {
                Rect[] rects = new Rect[cuts.Length];
                float x = 0f;

                for (int i = 0; i < cuts.Length; i++)
                {
                    rects[i] = new Rect(source);
                    rects[i].x += x;
                    rects[i].width *= cuts[i] / 100;
                    x += rects[i].width;
                }
                return rects;
            }
        }
#endif
    }

    [Serializable]
    public class UDictionary<TKey, TValue> : UDictionary, IDictionary<TKey, TValue>
    {
        // --- NEW: Internal Wrapper to fix Unity's List<List> serialization limit ---
        [Serializable]
        public struct ValueWrapper
        {
            public TValue v;
            public ValueWrapper(TValue value) => v = value;
        }
        // --------------------------------------------------------------------------

        [SerializeField]
        List<TKey> keys = new List<TKey>();

        // We now serialize a list of Wrappers, not TValue directly
        [SerializeField]
        List<ValueWrapper> values = new List<ValueWrapper>();

        public ICollection<TKey> Keys => keys;
        public ICollection<TValue> Values => Dictionary.Values;

        public int Count => keys.Count;
        public bool IsReadOnly => false;

        Dictionary<TKey, TValue> cache;
        public bool Cached => cache != null;

        public Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                if (cache == null)
                {
                    cache = new Dictionary<TKey, TValue>();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        if (keys[i] == null) continue;
                        if (cache.ContainsKey(keys[i])) continue;
                        
                        // Unwrap the value here
                        cache.Add(keys[i], values[i].v);
                    }
                }
                return cache;
            }
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                int index = keys.IndexOf(key);
                if (index < 0)
                {
                    Add(key, value);
                }
                else
                {
                    // Wrap the value when setting
                    values[index] = new ValueWrapper(value);
                    if (Cached) Dictionary[key] = value;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            keys.Add(key);
            // Wrap the value when adding
            values.Add(new ValueWrapper(value));
            
            if (Cached) Dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index < 0) return false;

            keys.RemoveAt(index);
            values.RemoveAt(index);

            if (Cached) Dictionary.Remove(key);
            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public void Clear()
        {
            keys.Clear();
            values.Clear();
            if (Cached) Dictionary.Clear();
        }

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (Dictionary as IDictionary).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
    }
}
using UnityEditor;
using UnityEngine;
namespace Lesson10.Examples
{
    [CustomPropertyDrawer(typeof(Ingredient))]
    public class IngredientDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Использование BeginProperty / EndProperty определяет блок кода, который будет
            // применим только для текущего SerializedProperty
            EditorGUI.BeginProperty(position, label, property);

            // Название переменной
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Настройка отступа для дочерних объектов
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Расчет поля размеров границ поля визуализации
            var amountRect = new Rect(position.x, position.y, 30, position.height);
            var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
            var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

            // Визуализация полей исходного класса
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("Amount"), GUIContent.none);
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("Unit"), GUIContent.none);
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("Name"), GUIContent.none);

            // Возвращение отступа к исходному значению
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
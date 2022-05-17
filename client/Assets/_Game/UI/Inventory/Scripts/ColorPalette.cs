using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Color Palette", menuName = "ScriptableObjects/Color Palette")]
public class ColorPalette : ScriptableObject
{
    public List<Color> colors;
}

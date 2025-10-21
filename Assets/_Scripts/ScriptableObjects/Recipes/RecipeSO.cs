using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Mixing/Recipe", fileName = "NewRecipe")]
public class RecipeSO : ScriptableObject
{
    [Tooltip("Boxes required for this recipe (order doesn't matter)")]
    public List<VialType> Ingredients;
    
    [Tooltip("What box this recipe produces if successful")]
    public VialType[] Results;
}
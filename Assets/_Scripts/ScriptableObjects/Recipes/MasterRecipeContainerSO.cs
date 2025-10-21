using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Mixing/MasterRecipeContainer", fileName = "RecipeContainer")]
public class MasterRecipeContainerSO : ScriptableObject
{
    public List<RecipeSO> Recipes;
}
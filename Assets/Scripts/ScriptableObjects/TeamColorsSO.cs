using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TeamColorsSO : ScriptableObject
{
   public List<Color> TeamColors = new List<Color>();

    /// <summary>
    /// The game allows a total of 4 teams, so there must always be 4 team colors defined
    /// </summary>
    private void OnValidate()
    {
        while(TeamColors.Count < 4)
        {
            TeamColors.Add(Color.white);
        }
    }
}

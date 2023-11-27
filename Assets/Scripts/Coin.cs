using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Tooltip("To which team does this coin belong to?")]
    private Team team;
    public Team Team { get { return team; } }

    [Header("Rendering")]
    [SerializeField, Tooltip("Mesh renderer of the coin, used for setting the color and making it glow")]
    private MeshRenderer meshRenderer;


    private bool isGlowing = false;

    public void SetTeam(Team newTeam)
    {
        team = newTeam;
        SetColor(team.TeamColor);
    }

    private void SetColor(Color newColor)
    {
        meshRenderer.material.SetColor("_BaseColor", newColor);
    }

    public void StartGlow()
    {
        isGlowing = true;
        StartCoroutine(GlowLerp());
    }

    public void MoveTo(Vector3 newTargetPos)
    {
        targetPos = newTargetPos;
    }

    private Vector3 targetPos;

    private void Update()
    {
        if (targetPos != null)
        {
            // Calculate the direction and distance to the target
            Vector3 direction = targetPos - transform.position;
            float distanceToMove = 10 * Time.deltaTime;

            // Move the GameObject towards the target position
            transform.Translate(direction.normalized * Mathf.Min(distanceToMove, direction.magnitude), Space.World);
        }
    }


    private IEnumerator GlowLerp()
    {
        Material targetMaterial = meshRenderer.material;
        float emissiveIntensityChangeSpeed = 0.4f;
        Color originalEmissiveColor = targetMaterial.GetColor("_EmissionColor");
        Color targetEmissiveColor = Color.white;
        targetMaterial.EnableKeyword("_EMISSION");

        float t = 0f;

        while (isGlowing)
        {
            t += Time.deltaTime * emissiveIntensityChangeSpeed;

            // Use Color.Lerp to interpolate between original and target colors
            targetMaterial.SetColor("_EmissionColor", Color.Lerp(originalEmissiveColor, targetEmissiveColor, Mathf.PingPong(t, 0.3f)));

            yield return null;
        }
    }

}

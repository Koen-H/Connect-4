using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 
/// Note: Coins are not networked objects, their position is not synchronized through a networkworkTransform because I wanted to do a physics drop where the coins fall on the table and it is not necessary to network it.
/// </summary>
public class Coin : MonoBehaviour
{
    [Tooltip("To which team does this coin belong to?")]
    private Team team;
    public Team Team { get { return team; } }

    [Header("Rendering")]
    [SerializeField, Tooltip("Mesh renderer of the coin, used for setting the color and making it glow on win")]
    private MeshRenderer meshRenderer;
    private bool isGlowing = false;

    [Header("Movement")]
    [SerializeField]
    private Rigidbody rb;

    private Vector3 targetPos;
    private bool moveToTargetPos = false;

    [SerializeField, Tooltip("The min and max of how fast the coin can shrink")]
    private Vector2 shrinkSpeedRange = new Vector2(0.1f,0.2f);

    [SerializeField]
    private float movementSpeed = 10f;
    private Coroutine movementCoroutine;

    public void SetTeam(Team newTeam)
    {
        team = newTeam;
        SetColor(team.TeamColor);
    }

    /// <summary>
    /// Set the color of the material of the coin.
    /// </summary>
    /// <param name="newColor"></param>
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
        moveToTargetPos = true;
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToTarget());
    }


    private IEnumerator MoveToTarget()
    {
        while (moveToTargetPos)
        {
            Vector3 direction = targetPos - transform.position;
            float distanceToMove = movementSpeed * Time.deltaTime;

            transform.Translate(direction.normalized * Mathf.Min(distanceToMove, direction.magnitude), Space.World);

            if (direction.magnitude < 0.01f)
            {
                transform.position = targetPos;
                moveToTargetPos = false;
                break;
            }

            yield return null; // Wait for the next frame
        }
    }

    /// <summary>
    /// Disables the kinematic property of the coin, allowing it to drop down to the bottom of the board just like real life.
    /// Starts the shrinking coroutine, a unique way for the coins to dissapear.
    /// </summary>
    public void EnableDropPhysics()
    {
        rb.isKinematic = false;
        moveToTargetPos = false;
        StartCoroutine(Shrink());
    }


    private IEnumerator Shrink()
    {
        float destroyValue = 0.05f;
        float shrinkSpeed = Random.Range(shrinkSpeedRange.x, shrinkSpeedRange.y);
        Vector3 shrinkVector = new Vector3(shrinkSpeed, shrinkSpeed, shrinkSpeed);
        while( transform.localScale.x > destroyValue) {

            transform.localScale -= shrinkVector * Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
        yield return null;
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

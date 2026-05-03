using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChecker : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public LayerMask wallLayer;

    public bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    public bool IsTouchingWall => Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

    public bool IsFacingWall(float facingDirection)
    {
        if (wallCheck == null) return false;
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }
}
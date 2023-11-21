// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnJoinedInstantiate.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities, 
// </copyright>
// <summary>
// Very basic component to move a GameObject by WASD and Space.
// </summary>
// <remarks>
// Requires a PhotonView. 
// Disables itself on GameObjects that are not owned on Start.
// 
// Speed affects movement-speed. 
// JumpForce defines how high the object "jumps". 
// JumpTimeout defines after how many seconds you can jump again.
// </remarks>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Very basic component to move a GameObject by WASD and Space.
    /// </summary>
    /// <remarks>
    ///     Requires a PhotonView.
    ///     Disables itself on GameObjects that are not owned on Start.
    ///     Speed affects movement-speed.
    ///     JumpForce defines how high the object "jumps".
    ///     JumpTimeout defines after how many seconds you can jump again.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    public class MoveByKeys : MonoBehaviourPun
    {
        public float Speed = 10f;
        public float JumpForce = 200f;
        public float JumpTimeout = 0.5f;
        private Rigidbody body;
        private Rigidbody2D body2d;

        private bool isSprite;
        private float jumpingTime;

        public void Start()
        {
            //enabled = photonView.isMine;
            isSprite = GetComponent<SpriteRenderer>() != null;

            body2d = GetComponent<Rigidbody2D>();
            body = GetComponent<Rigidbody>();
        }


        // Update is called once per frame
        public void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            if (Input.GetAxisRaw("Horizontal") < -0.1f || Input.GetAxisRaw("Horizontal") > 0.1f)
                transform.position += Vector3.right * (Speed * Time.deltaTime) * Input.GetAxisRaw("Horizontal");

            // jumping has a simple "cooldown" time but you could also jump in the air
            if (jumpingTime <= 0.0f)
            {
                if (body != null || body2d != null)
                    // obj has a Rigidbody and can jump (AddForce)
                    if (Input.GetKey(KeyCode.Space))
                    {
                        jumpingTime = JumpTimeout;

                        var jump = Vector2.up * JumpForce;
                        if (body2d != null)
                            body2d.AddForce(jump);
                        else if (body != null) body.AddForce(jump);
                    }
            }
            else
            {
                jumpingTime -= Time.deltaTime;
            }

            // 2d objects can't be moved in 3d "forward"
            if (!isSprite)
                if (Input.GetAxisRaw("Vertical") < -0.1f || Input.GetAxisRaw("Vertical") > 0.1f)
                    transform.position += Vector3.forward * (Speed * Time.deltaTime) * Input.GetAxisRaw("Vertical");
        }
    }
}
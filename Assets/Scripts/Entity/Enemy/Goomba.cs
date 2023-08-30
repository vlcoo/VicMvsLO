using UnityEngine;

using Fusion;
using NSMB.Game;
using NSMB.Utils;

namespace NSMB.Entities.Enemies {
    public class Goomba : KillableEntity {

        //---Serialized Variables
        [SerializeField] private Sprite deadSprite;
        [SerializeField] private float speed, terminalVelocity = -8;

        public override void Spawned() {
            base.Spawned();
            body.velocity = new(speed * (FacingRight ? 1 : -1), body.velocity.y);
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();
            if (!Object)
                return;

            if (!IsActive) {
                body.velocity = Vector2.zero;
                return;
            }

            if (GameData.Instance.GameEnded) {
                body.velocity = Vector2.zero;
                AngularVelocity = 0;
                legacyAnimation.enabled = false;
                body.freeze = true;
                return;
            }

            if (IsDead && !WasSpecialKilled) {
                gameObject.layer = Layers.LayerEntity;
                return;
            }

            HandleWallCollisions();

            body.velocity = new(speed * (FacingRight ? 1 : -1), Mathf.Max(terminalVelocity, body.velocity.y));
        }

        private void HandleWallCollisions() {
            PhysicsDataStruct data = body.data;

            if (data.HitLeft || data.HitRight)
                FacingRight = data.HitLeft;
        }

        //---KillableEntity overrides
        public override void Kill() {
            IsDead = true;

            body.velocity = Vector2.zero;
            body.freeze = true;

            DespawnTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
        }

        public override void OnIsDeadChanged() {
            base.OnIsDeadChanged();

            if (IsDead) {
                if (!WasSpecialKilled) {
                    legacyAnimation.enabled = false;
                    sRenderer.sprite = deadSprite;
                }
            } else {
                legacyAnimation.enabled = true;
            }
        }

        //---BasicEntity overrides
        public override void OnFacingRightChanged() {
            sRenderer.flipX = FacingRight;
        }
    }
}
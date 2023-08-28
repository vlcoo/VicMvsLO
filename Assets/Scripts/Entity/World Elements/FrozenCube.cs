using UnityEngine;

using Fusion;
using NSMB.Entities.Collectable;
using NSMB.Entities.Player;
using NSMB.Extensions;
using NSMB.Game;
using NSMB.Utils;

namespace NSMB.Entities {

    [OrderAfter(typeof(NetworkPhysicsSimulation2D), typeof(BasicEntity), typeof(FreezableEntity), typeof(PlayerController))]
    public class FrozenCube : HoldableEntity {

        //---Networked Variables
        [Networked] private FreezableEntity FrozenEntity { get; set; }
        [Networked] private Vector2 EntityPositionOffset { get; set; }
        [Networked] private Vector2 CubeSize { get; set; }
        [Networked] private NetworkBool FastSlide { get; set; }
        [Networked] public TickTimer AutoBreakTimer { get; set; }
        [Networked] private byte Combo { get; set; }
        [Networked] private NetworkBool Fallen { get; set; }
        [Networked] public UnfreezeReason KillReason { get; set; }

        //---Serialized Variables
        [SerializeField] private float shakeSpeed = 1f, shakeAmount = 0.1f, autoBreak = 3f;

        public void OnBeforeSpawned(FreezableEntity entityToFreeze, Vector2 size, Vector2 offset) {
            FrozenEntity = entityToFreeze;
            CubeSize = size;
            EntityPositionOffset = offset;
        }

        public override void Spawned() {
            base.Spawned();
            holderOffset = Vector2.one;

            if (!FrozenEntity) {
                Kill();
                return;
            }

            sRenderer.size = CubeSize;
            hitbox.size = CubeSize - (Vector2.one * 0.05f);
            hitbox.offset = CubeSize * Vector2.up * 0.5f;

            AutoBreakTimer = TickTimer.CreateFromSeconds(Runner, autoBreak);
            flying = FrozenEntity.IsFlying;
            ApplyConstraints();

            FrozenEntity.Freeze(this);

            // Move entity inside us
            if (!FrozenEntity.IsCarryable) {
                dieWhenInsideBlock = false;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            Instantiate(PrefabList.Instance.Particle_IceBreak, transform.position, Quaternion.identity);
        }

        public void LateUpdate() {
            if (FrozenEntity && FrozenEntity.IsCarryable && FrozenEntity.body.interpolationTarget && body.interpolationTarget) {
                Transform target = FrozenEntity.body.interpolationTarget.transform;
                Vector2 newPos = (Vector2) body.interpolationTarget.position + EntityPositionOffset;
                Utils.Utils.WrapWorldLocation(ref newPos);
                target.position = new(newPos.x, newPos.y, target.position.z);
            }
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            if (!Object)
                return;

            gameObject.layer = (Holder || FastSlide) ? Layers.LayerEntity : Layers.LayerGroundEntity;

            if (body.position.y + hitbox.size.y < GameManager.Instance.LevelMinY) {
                Kill();
                return;
            }

            if (Holder && Utils.Utils.IsAnyTileSolidBetweenWorldBox(body.position + hitbox.offset, hitbox.size * transform.lossyScale * 0.75f)) {
                KillWithReason(UnfreezeReason.HitWall);
                return;
            }

            // Handle interactions with tiles
            if (FrozenEntity.IsCarryable) {
                if (!HandleTile())
                    return;

                FrozenEntity.body.position = body.position + EntityPositionOffset;
                FrozenEntity.body.velocity = Vector2.zero;
                FrozenEntity.body.freeze = true;
            }

            if (FrozenEntity is PlayerController || (!Holder && !FastSlide)) {

                if (AutoBreakTimer.Expired(Runner)) {
                    if (!FastSlide)
                        KillReason = UnfreezeReason.Timer;

                    if (flying)
                        Fallen = true;
                    else {
                        KillWithReason(UnfreezeReason.Timer);
                        return;
                    }
                }
            }

            if (Holder)
                return;

            if (!FastSlide) {
                float remainingTime = AutoBreakTimer.RemainingTime(Runner) ?? 0f;
                if (remainingTime < 1f)
                    body.position = new(body.position.x + Mathf.Sin(remainingTime * shakeSpeed) * shakeAmount * Runner.DeltaTime, transform.position.y);
            }


            // Our entity despawned. remove.
            if (!FrozenEntity) {
                Runner.Despawn(Object);
                return;
            }

            // Handle interactions with tiles
            if (FrozenEntity.IsCarryable) {
                if (!HandleTile())
                    return;
            }

            if (FastSlide) {
                //if (body.data.OnGround && body.data.FloorAngle != 0) {
                //    RaycastHit2D ray = Runner.GetPhysicsScene2D().BoxCast(body.position + Vector2.up * hitbox.size * 0.5f, hitbox.size, 0, Vector2.down, 0.2f, Layers.MaskSolidGround);
                //    if (ray) {
                //        body.position = new(body.position.x, ray.point.y + Physics2D.defaultContactOffset);
                //        if (ray.distance < 0.1f)
                //            body.velocity = new(body.velocity.x, Mathf.Min(0, body.velocity.y));
                //    }
                //}

                body.velocity = new(throwSpeed * (FacingRight ? 1 : -1), body.velocity.y);
            }

            ApplyConstraints();
        }

        private bool HandleTile() {

            if ((FastSlide && (body.data.HitLeft || body.data.HitRight))
                || (flying && Fallen && body.data.OnGround && !Holder)
                || ((Holder || body.data.OnGround) && body.data.HitRoof)) {

                Kill();
                return false;
            }

            return true;
        }

        private void ApplyConstraints() {
            //body.mass = Holder ? 0 : 1;
            body.freeze = !FrozenEntity.IsCarryable;

            if (!Holder) {
                //if (!FastSlide)
                //    body.constraints |= RigidbodyConstraints2D.FreezePositionX;

                //if (flying && !Fallen)
                //    body.constraints |= RigidbodyConstraints2D.FreezePositionY;
            }
        }

        public void KillWithReason(UnfreezeReason reason) {
            KillReason = reason;
            Kill();
        }

        //---IPlayerInteractable overrides
        public override void InteractWithPlayer(PlayerController player) {

            // Don't interact with our lovely holder
            if (Holder == player)
                return;

            // Don't interact with other frozen players
            if (player.IsFrozen)
                return;

            // Temporary invincibility
            if (PreviousHolder == player && ThrowInvincibility.IsActive(Runner))
                return;

            Utils.Utils.UnwrapLocations(body.position, player.body.position, out Vector2 ourPos, out Vector2 playerPos);

            bool attackedFromAbove = Vector2.Dot((playerPos - ourPos).normalized, Vector2.up) > 0.4f;
            bool attackedFromBelow = playerPos.y < ourPos.y
                && playerPos.x - (player.MainHitbox.size.x * 0.5f) < (ourPos.x + hitbox.size.x * 0.5f)
                && playerPos.x + (player.MainHitbox.size.x * 0.5f) > (ourPos.x - hitbox.size.x * 0.5f);

            // Player should instakill
            if (!Holder && player.InstakillsEnemies) {
                Kill();
                return;
            }

            if (player.HasGroundpoundHitbox && attackedFromAbove && player.State != Enums.PowerupState.MiniMushroom) {
                // Groundpounded by player
                player.body.velocity = new(0, 38.671875f * Runner.DeltaTime);
                player.GroundpoundAnimCounter++;
                KillWithReason(UnfreezeReason.Groundpounded);
                return;

            } else if (attackedFromBelow && player.State != Enums.PowerupState.MiniMushroom) {
                // Bumped from below
                KillWithReason(UnfreezeReason.BlockBump);
                return;

            } else if (FastSlide) {
                // Do damage
                player.DoKnockback(ourPos.x > playerPos.x, 1, false, Object);
                Kill();
                return;

            } else if (!Fallen) {
                if (FrozenEntity.IsCarryable && !Holder && !IsDead && player.CanPickupItem && player.IsOnGround && !player.IsSwimming) {
                    // Pickup
                    Fallen = true;
                    Pickup(player);
                }
            }
        }

        //---IFireballInteractable overrides
        public override bool InteractWithFireball(Fireball fireball) {
            if (!fireball.IsIceball)
                Kill();

            return true;
        }

        public override bool InteractWithIceball(Fireball iceball) {
            return true;
        }

        //---IThrowableEntity overrides
        public override void Pickup(PlayerController player) {
            base.Pickup(player);
            Physics2D.IgnoreCollision(hitbox, player.MainHitbox);
            AutoBreakTimer = TickTimer.CreateFromSeconds(Runner, (AutoBreakTimer.RemainingTime(Runner) ?? 0f) + 1f);
            body.velocity = Vector2.zero;
        }

        public override void Throw(bool toRight, bool crouch) {
            base.Throw(toRight, false);

            Fallen = false;
            flying = false;
            FastSlide = true;
            ThrowInvincibility = TickTimer.CreateFromSeconds(Runner, 3f);

            if (FrozenEntity.IsFlying) {
                Fallen = true;
                body.freeze = false;
            }
            ApplyConstraints();
        }

        public override void Kick(PlayerController kicker, bool fromLeft, float kickFactor, bool groundpound) {
            // Kicking does nothing.
        }

        //---IKillableEntity overrides
        protected override void CheckForEntityCollisions() {
            if (Holder || !FastSlide)
                return;

            // Only run when fastsliding...
            int count = Runner.GetPhysicsScene2D().OverlapBox(body.position + hitbox.offset, hitbox.size, 0, CollisionBuffer, Layers.MaskEntities);

            for (int i = 0; i < count; i++) {
                GameObject obj = CollisionBuffer[i].gameObject;

                if (obj.transform.IsChildOf(transform))
                    continue;

                if (PreviousHolder && obj.TryGetComponent(out Coin coin)) {
                    coin.InteractWithPlayer(PreviousHolder);
                    continue;
                }

                if (obj.TryGetComponent(out KillableEntity killable)) {
                    if (killable.IsDead || killable == FrozenEntity)
                        continue;

                    if (Holder == killable || PreviousHolder == killable || FrozenEntity == killable)
                        continue;

                    // Kill entity we ran into
                    killable.SpecialKill(killable.body.position.x > body.position.x, false, Combo++);

                    // Kill ourselves if we're being held too
                    if (Holder)
                        SpecialKill(killable.body.position.x < body.position.x, false, 0);

                    continue;
                }
            }
        }

        public override void Kill() {
            if (Holder) {
                if (FrozenEntity is PlayerController pc) {
                    bool dropStars = pc.Data.Team != Holder.Data.Team;
                    Holder.DoKnockback(Holder.FacingRight, dropStars ? 1 : 0, false, FrozenEntity.Object);
                }
                Holder.SetHeldEntity(null);
            }

            if (FrozenEntity) {
                FrozenEntity.Unfreeze(KillReason);
            }

            IsDead = true;
            Runner.Despawn(Object);
        }

        public override void SpecialKill(bool right, bool groundpound, int combo) {
            Kill();
        }

        //---OnChangeds
        public override void OnIsDeadChanged() {
            base.OnIsDeadChanged();

            sRenderer.enabled = !IsDead;
        }

        //---Static
        public static void FreezeEntity(NetworkRunner runner, FreezableEntity entity) {
            Vector2 entityPosition = entity.body ? entity.body.position : entity.transform.position;
            Vector2 spawnPosition = entityPosition - entity.FrozenOffset;

            runner.Spawn(PrefabList.Instance.Obj_FrozenCube, spawnPosition, onBeforeSpawned: (runner, obj) => {
                FrozenCube cube = obj.GetComponent<FrozenCube>();
                cube.OnBeforeSpawned(entity, entity.FrozenSize, entity.FrozenOffset);
            });
        }
    }
}

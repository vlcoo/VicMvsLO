using NSMB.Utilities.Components;
using NSMB.Utilities.Extensions;
using Quantum;
using System.Collections;
using UnityEngine;
using static NSMB.Utilities.QuantumViewUtils;

namespace NSMB.Entities.Enemies {
    public unsafe class BulletBillAnimator : QuantumEntityViewComponent {

        //---Serialized Variables
        [SerializeField] private GameObject mesh;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private AudioSource sfx;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject specialKillParticles;

        [SerializeField] private float fireballScaleSize = 0.075f;

        //---Private Variables
        private float fireballScaleTimer;

        public void OnValidate() {
            this.SetIfNull(ref sfx);
            this.SetIfNull(ref animator, UnityExtensions.GetComponentType.Children);
        }

        public void Start() {
            QuantumEvent.Subscribe<EventEnemyKilled>(this, OnEnemyKilled, FilterOutReplayFastForward);
            QuantumEvent.Subscribe<EventPlayComboSound>(this, OnPlayComboSound, FilterOutReplayFastForward);
            QuantumEvent.Subscribe<EventBulletBillHitByProjectile>(this, OnBulletBillHitByProjectile, FilterOutReplayFastForward);
        }

        public override void OnActivate(Frame f) {
            if (!IsReplayFastForwarding) {
                sfx.Play();
            }
            animator.enabled = true;
            StartCoroutine(ChangeSpriteSortingOrder());
            trailParticles.Play();
        }

        public override void OnUpdateView() {
            Frame f = PredictedFrame;

            if (!f.Exists(EntityRef)) {
                return;
            }

            var enemy = f.Unsafe.GetPointer<Enemy>(EntityRef);
            var freezable = f.Unsafe.GetPointer<Freezable>(EntityRef);
            bool frozen = freezable->IsFrozen(f);

            // sRenderer.enabled = enemy->IsActive;
            mesh.SetActive(enemy->IsActive);

            var emission = trailParticles.emission;
            emission.enabled = enemy->IsActive && !frozen;
            animator.enabled = !frozen;

            if (enemy->IsDead) {
                transform.rotation *= Quaternion.Euler(0, 0, 400f * (enemy->FacingRight ? -1 : 1) * Time.deltaTime);
            } else {
                transform.rotation = Quaternion.identity;
            }

            float scale = 1 + Mathf.Abs(Mathf.Sin(fireballScaleTimer * 10 * Mathf.PI)) * fireballScaleSize;
            transform.localScale = Vector3.one * scale;
            fireballScaleTimer = Mathf.Max(0, fireballScaleTimer - Time.deltaTime);

            mesh.FlipX(enemy->FacingRight, false);
            Vector2 pos = trailParticles.transform.localPosition;
            pos.x = Mathf.Abs(pos.x) * (enemy->FacingRight ? -1 : 1);
            trailParticles.transform.localPosition = pos;
        }

        private static WaitForSeconds wait = new(0.33f);
        private IEnumerator ChangeSpriteSortingOrder() {
            var originalTransform = mesh.transform.localPosition;
            mesh.transform.localPosition = originalTransform + new Vector3(0, 0, 1);
            yield return wait;
            mesh.transform.localPosition = originalTransform;
        }

        private void OnBulletBillHitByProjectile(EventBulletBillHitByProjectile e) {
            if (e.Entity != EntityRef) {
                return;
            }

            fireballScaleTimer = 0.3f;
        }

        private void OnEnemyKilled(EventEnemyKilled e) {
            if (e.Enemy != EntityRef) {
                return;
            }

            if (e.KillReason == KillReason.Special || e.KillReason == KillReason.Groundpounded) {
                Instantiate(specialKillParticles, transform.position, Quaternion.identity);
            } else {
                // sfx.PlayOneShot(SoundEffect.Enemy_Generic_Stomp);
            }
        }

        private void OnPlayComboSound(EventPlayComboSound e) {
            if (e.Entity != EntityRef) {
                return;
            }

            sfx.PlayOneShot(QuantumUtils.GetComboSoundEffect(e.Combo));
        }
    }
}

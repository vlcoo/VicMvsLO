using NSMB.Sound;
using NSMB.Utilities;
using NSMB.Utilities.Extensions;
using Quantum;
using UnityEngine;
using static NSMB.Utilities.QuantumViewUtils;

namespace NSMB.Entities.Enemies {
    public unsafe class BooAnimator : QuantumEntityViewComponent {

        //---Static Variables
        private static readonly int ParamFacingRight = Animator.StringToHash("FacingRight");
        private static readonly int ParamScared = Animator.StringToHash("Scared");

        //---Serialized Variables
        [SerializeField] private Transform bobber;
        [SerializeField] private SpriteRenderer sRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private SoundEffectPlayer sfx;
        [SerializeField] private float sinSpeed = 1f, sinAmplitude = 0.0875f;

        public void OnValidate() {
            this.SetIfNull(ref sRenderer, UnityExtensions.GetComponentType.Children);
            this.SetIfNull(ref animator, UnityExtensions.GetComponentType.Children);
            this.SetIfNull(ref sfx, UnityExtensions.GetComponentType.Children);
            if (!bobber) {
                bobber = sRenderer.transform;
            }
        }

        public void Start() {
            QuantumEvent.Subscribe<EventBooBecomeActive>(this, OnBooBecameActive, FilterOutReplayFastForward);
            QuantumEvent.Subscribe<EventPlayComboSound>(this, OnPlayComboSound, FilterOutReplayFastForward);
        }

        public override void OnUpdateView() {
            Frame f = PredictedFrame;
            float time = (f.Number + Game.InterpolationFactor) * f.DeltaTime.AsFloat;

            bobber.localPosition = new(0, Mathf.Sin(2 * Mathf.PI * time * sinSpeed) * sinAmplitude);

            if (!f.Unsafe.TryGetPointer(EntityRef, out Boo* boo)
                || !f.Unsafe.TryGetPointer(EntityRef, out Enemy* enemy)) {

                return;
            }

            animator.SetBool(ParamFacingRight, enemy->FacingRight);
            animator.SetBool(ParamScared, boo->UnscaredFrames > 0);
            sRenderer.enabled = enemy->IsActive;

            if (enemy->IsDead) {
                transform.rotation *= Quaternion.Euler(0, 0, 400f * (enemy->FacingRight ? -1 : 1) * Time.deltaTime);
            } else {
                transform.rotation = Quaternion.identity;
            }
            
            if (GlobalController.Instance.challenger.CurrentChallenge == "Invisible Enemies & Powerups") sRenderer.transform.localScale = Vector3.zero;
        }

        private void OnBooBecameActive(EventBooBecomeActive e) {
            if (e.Entity != EntityRef) {
                return;
            }

            sfx.PlayOneShot(SoundEffect.Enemy_Boo_Laugh);
        }

        private void OnPlayComboSound(EventPlayComboSound e) {
            if (e.Entity != EntityRef) {
                return;
            }

            sfx.PlayOneShot(QuantumViewUtils.GetComboSoundEffect(e.Combo));
        }
    }
}

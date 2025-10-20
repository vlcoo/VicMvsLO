using NSMB.Utilities.Extensions;
using Quantum;
using System;
using UnityEngine;

namespace NSMB.UI.Game.Track {
    public unsafe class StarTrackIcon : TrackIcon {

        //---Static Variables
        private static readonly Vector3 ThreeFourths = new(0.75f, 0.75f, 1f);

        //---Serialized Variables
        [SerializeField] private Animator animator;

        public override void OnValidate() {
            base.OnValidate();
            this.SetIfNull(ref animator);
        }

        public void Start() {
            Frame f = Updater.ObservedGame.Frames.Predicted;
            image.enabled = f.Global->Rules.HStars;
        }

        public override void OnUpdateView() {
            base.OnUpdateView();
            Frame f = Updater.ObservedGame.Frames.Predicted;

            if (PredictedFrame.Unsafe.TryGetPointer(targetEntity, out BigStar* star) && f.Global->Rules.HStars) {
                if (star->IsStationary) {
                    animator.enabled = true;
                    transform.localScale = Vector3.zero;
                } else {
                    animator.enabled = false;
                    transform.localScale = ThreeFourths;
                }
                image.enabled = true;
            } else {
                image.enabled = false;
                upArrow.enabled = false;
                downArrow.enabled = false;
            }
        }
    }
}

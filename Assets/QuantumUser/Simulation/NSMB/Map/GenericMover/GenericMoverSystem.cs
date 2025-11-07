using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum {
    [UnityEngine.Scripting.Preserve]
    public unsafe class GenericMoverSystem : SystemMainThreadEntityFilter<GenericMover, GenericMoverSystem.Filter> {
        public struct Filter {
            public EntityRef Entity;
            public Transform2D* Transform;
            public GenericMover* GenericMover;
            public MovingPlatform* Platform;
        }

        public override void Update(Frame f, ref Filter filter, VersusStageData stage) {
            if (f.Global->GameState is not (GameState.Starting or GameState.Playing)) {
                return;
            }

            var platform = filter.Platform;
            var genericMover = filter.GenericMover;
            var transform = filter.Transform;
            // var asset = f.FindAsset(genericMover->MoverAsset);
            var moverPathList = f.ResolveList(genericMover->Path);

            FP currentTime = ((f.Number - f.Global->StartFrame) * f.DeltaTime) + genericMover->StartOffset;
            FP nextTime = ((f.Number - f.Global->StartFrame + 1) * f.DeltaTime) + genericMover->StartOffset;

            FPVector2 currentPos = SamplePosition(moverPathList, currentTime, genericMover->LoopingMode, genericMover->DurationIsSpeedInstead);
            FPVector2 nextPos = SamplePosition(moverPathList, nextTime, genericMover->LoopingMode, genericMover->DurationIsSpeedInstead);
            FPVector2 velocity = nextPos - currentPos;

            // This doesnt work.
            if (velocity.SqrMagnitude > 1) {
                transform->Teleport(f, transform->Position + velocity);
                velocity = FPVector2.Zero;
            }

            platform->Velocity = velocity * f.UpdateRate;
        }

        private static FPVector2 SamplePosition(QList<PathNode> positions, FP sample, LoopingMode loopMode, bool durationIsSpeed = false) {
            FP totalDuration = 0;
            for (int i = 0; i < positions.Count; i++) {
                if (durationIsSpeed) {
                    totalDuration += FPVector2.Distance(positions[i].Position, positions[(i + 1) % positions.Count].Position) / positions[i].TravelDuration;
                } else {
                    totalDuration += positions[i].TravelDuration;
                }
            }

            if (loopMode == LoopingMode.Loop) {
                sample %= totalDuration;
            } else if (loopMode == LoopingMode.Clamp) {
                sample = FPMath.Clamp(sample, 0, totalDuration);
            } else if (loopMode == LoopingMode.PingPong) {
                sample %= (totalDuration * 2); 
                if (sample > totalDuration) {
                    sample = (totalDuration * 2) - sample;
                }
            }

            for (int i = 0; i < positions.Count; i++) {
                PathNode current = positions[i];
                PathNode next = positions[(i + 1) % positions.Count];
                FP currentDuration = current.TravelDuration;
                if (durationIsSpeed) currentDuration = FPVector2.Distance(current.Position, next.Position) / currentDuration;
                
                if (sample > currentDuration) {
                    sample -= currentDuration;
                } else {
                    FP alpha = sample / currentDuration;
                    if (next.EaseIn && next.EaseOut) {
                        alpha = QuantumUtils.EaseInOut(alpha);
                    } else if (next.EaseIn) {
                        alpha = QuantumUtils.EaseIn(alpha);
                    } else if (next.EaseOut) {
                        alpha = QuantumUtils.EaseOut(alpha);
                    }
                    return FPVector2.Lerp(current.Position, next.Position, alpha);
                }
            }

            return default;
        }

    }
}

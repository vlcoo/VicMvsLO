using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum {
    public unsafe class GenericMoverSystem : SystemMainThreadFilter<GenericMoverSystem.Filter>, ISignalOnComponentAdded<GenericMover>, ISignalOnComponentRemoved<GenericMover> {
        public struct Filter {
            public EntityRef Entity;
            public Transform2D* Transform;
            public GenericMover* GenericMover;
            public MovingPlatform* Platform;
        }

        public override void Update(Frame f, ref Filter filter) {
            if (f.Global->GameState != GameState.Playing) {
                return;
            }

            var platform = filter.Platform;
            var genericMover = filter.GenericMover;
            var transform = filter.Transform;
            // var asset = f.FindAsset(genericMover->MoverAsset);
            var moverPathList = f.ResolveList(genericMover->Path);

            FP currentTime = ((f.Number - f.Global->StartFrame) * f.DeltaTime) + genericMover->StartOffset;
            FP nextTime = ((f.Number - f.Global->StartFrame + 1) * f.DeltaTime) + genericMover->StartOffset;

            FPVector2 currentPos = SamplePosition(moverPathList, currentTime, genericMover->LoopingMode);
            FPVector2 nextPos = SamplePosition(moverPathList, nextTime, genericMover->LoopingMode);
            FPVector2 velocity = nextPos - currentPos;

            // This doesnt work.
            if (velocity.SqrMagnitude > 1) {
                transform->Teleport(f, transform->Position + velocity);
                velocity = FPVector2.Zero;
            }

            platform->Velocity = velocity * f.UpdateRate;
        }

        private static FPVector2 SamplePosition(QList<PathNode> positions, FP sample, LoopingMode loopMode) {
            FP totalDuration = 0;
            for (int i = 0; i < positions.Count; i++) {
                totalDuration += positions[i].TravelDuration;
            }

            switch (loopMode)
            {
            case LoopingMode.Loop:
                sample %= totalDuration;
                break;
            case LoopingMode.Clamp:
                sample = FPMath.Clamp(sample, 0, totalDuration);
                break;
            case LoopingMode.PingPong:
            {
                sample %= (totalDuration * 2); 
                if (sample > totalDuration) {
                    sample = (totalDuration * 2) - sample;
                }
                break;
            }
            }

            for (int i = 0; i < positions.Count; i++) {
                PathNode current = positions[i];
                PathNode next = positions[(i + 1) % positions.Count];

                if (sample > current.TravelDuration) {
                    sample -= current.TravelDuration;
                } else {
                    FP alpha = sample / current.TravelDuration;
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

        public void OnAdded(Frame f, EntityRef entity, GenericMover* component) {
            // component->Path = f.AllocateList<PathNode>(10);
        }
        
        public void OnRemoved(Frame f, EntityRef entity, GenericMover* component) {
            // f.FreeList(component->Path);
            // component->Path = default;
        }
    }
}

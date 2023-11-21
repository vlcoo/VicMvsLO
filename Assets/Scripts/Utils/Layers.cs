using UnityEngine;

namespace NSMB.Utils
{
    public static class Layers
    {
        private static int? _maskAnyGround, _maskOnlyGround;

        private static int? _layerGround,
            _layerHitsNothing,
            _layerDefault,
            _layerPassthrough,
            _layerLooseCoin,
            _layerEntity;

        public static int MaskAnyGround => LazyLoadMask(ref _maskAnyGround, "Ground", "Semisolids", "IceBlock");
        public static int MaskOnlyGround => LazyLoadMask(ref _maskOnlyGround, "Ground");
        public static int LayerGround => LazyLoadLayer(ref _layerGround, "Ground");
        public static int LayerHitsNothing => LazyLoadLayer(ref _layerHitsNothing, "HitsNothing");
        public static int LayerDefault => LazyLoadLayer(ref _layerDefault, "Default");
        public static int LayerPassthrough => LazyLoadLayer(ref _layerPassthrough, "PlayerPassthrough");
        public static int LayerLooseCoin => LazyLoadLayer(ref _layerLooseCoin, "LooseCoin");
        public static int LayerEntity => LazyLoadLayer(ref _layerEntity, "Entity");

        private static int LazyLoadMask(ref int? variable, params string[] layers)
        {
            if (variable != null)
                return (int)variable;

            return (int)(variable = LayerMask.GetMask(layers));
        }

        private static int LazyLoadLayer(ref int? variable, string layer)
        {
            if (variable != null)
                return (int)variable;

            return (int)(variable = LayerMask.NameToLayer(layer));
        }
    }
}
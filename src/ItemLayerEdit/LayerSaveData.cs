using MessagePack;

namespace ItemLayerEdit
{
    [MessagePackObject(true)]
    internal class LayerSaveData
    {
        public int DefaultLayer;
        public int NewLayer;
        public int ObjectId;
    }
}

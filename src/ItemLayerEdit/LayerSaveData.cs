using MessagePack;

namespace ItemLayerEdit
{
    [MessagePackObject(true)]
    public class LayerSaveData
    {
        public int DefaultLayer;
        public int NewLayer;
        public int ObjectId;
    }
}

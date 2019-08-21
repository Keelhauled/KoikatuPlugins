using ExtensibleSaveFormat;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using MessagePack;
using Studio;
using System.Collections.Generic;

namespace LightManager
{
    public class SceneDataController : SceneCustomFunctionController
    {
        const string SavedLights = "SavedLights";

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            var data = GetExtendedData();
            if(data == null)
                return;

            if(operation == SceneOperationKind.Load || operation == SceneOperationKind.Import)
            {
                if(data.data.TryGetValue(SavedLights, out var savedLightData))
                {
                    var savedLights = MessagePackSerializer.Deserialize<List<SavedLight>>((byte[])savedLightData);

                    foreach(var savedLight in savedLights)
                    {
                        if(loadedItems.TryGetValue(savedLight.LightId, out var lightInfo))
                        {
                            if(lightInfo is OCILight ocilight)
                            {
                                if(loadedItems.TryGetValue(savedLight.TargetId, out var targetInfo))
                                {
                                    var tracker = ocilight.light.gameObject.AddComponent<TrackTransform>();
                                    tracker.target = (targetInfo as OCIChar).charInfo.objBody.transform;
                                    tracker.targetKey = targetInfo.objectInfo.dicKey;
                                    tracker.rotationSpeed = savedLight.RotationSpeed;
                                    tracker.targetName = savedLight.TargetName;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnSceneSave()
        {
            var studio = Studio.Studio.Instance;
            var savedLights = new List<SavedLight>();

            foreach(var objectCtrlInfo in studio.dicObjectCtrl.Values)
            {
                if(objectCtrlInfo is OCILight light)
                {
                    var tracker = light.light.gameObject.GetComponent<TrackTransform>();
                    if(tracker && studio.dicObjectCtrl.TryGetValue(tracker.targetKey, out ObjectCtrlInfo info))
                    {
                        var saveData = new SavedLight
                        {
                            LightId = objectCtrlInfo.objectInfo.dicKey,
                            TargetId = tracker.targetKey,
                            RotationSpeed = tracker.rotationSpeed,
                            TargetName = tracker.targetName
                        };

                        savedLights.Add(saveData);
                    }
                }
            }

            if(savedLights.Count > 0)
            {
                var data = new PluginData();
                data.data.Add(SavedLights, MessagePackSerializer.Serialize(savedLights));
                SetExtendedData(data);
            }
        }

        [MessagePackObject(true)]
        public class SavedLight
        {
            public int LightId;
            public int TargetId;
            public float RotationSpeed;
            public string TargetName;
        }
    }
}

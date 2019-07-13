namespace TogglePOVKK
{
    public static class CharaUtils
    {
        public static void SetNeckLook(this ChaInfo body, NECK_LOOK_TYPE_VER2 necktype)
        {
            for(int i = 0; i < body.neckLookCtrl.neckLookScript.neckTypeStates.Length; i++)
            {
                if(body.neckLookCtrl.neckLookScript.neckTypeStates[i].lookType == necktype)
                    body.neckLookCtrl.ptnNo = i;
            }
        }

        public static void SetEyeLook(this ChaInfo body, EYE_LOOK_TYPE eyetype)
        {
            for(int i = 0; i < body.eyeLookCtrl.eyeLookScript.eyeTypeStates.Length; i++)
            {
                if(body.eyeLookCtrl.eyeLookScript.eyeTypeStates[i].lookType == eyetype)
                    body.eyeLookCtrl.ptnNo = i;
            }
        }

        public static NECK_LOOK_TYPE_VER2 GetNeckLook(this ChaInfo body)
        {
            return body.neckLookCtrl.neckLookScript.neckTypeStates[body.neckLookCtrl.ptnNo].lookType;
        }

        public static EYE_LOOK_TYPE GetEyeLook(this ChaInfo body)
        {
            return body.eyeLookCtrl.eyeLookScript.eyeTypeStates[body.eyeLookCtrl.ptnNo].lookType;
        }
    }
}

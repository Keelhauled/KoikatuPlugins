using BepInEx.Logging;
using IllusionUtility.GetUtility;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace TogglePOVKK
{
    abstract class CommonView : MonoBehaviour
    {
        protected abstract bool CameraEnabled { get; set; }
        protected abstract bool DepthOfField { get; set; }
        protected abstract bool Shield { get; set; }
        protected abstract ChaControl GetChara();

        float sensitivityX = 0.5f;
        float sensitivityY = 0.5f;
        float MAXFOV = 120f;
        float MALE_OFFSET;
        float FEMALE_OFFSET;
        bool SHOW_HAIR;
        bool clampRotation = true;

        float currentfov;
        bool currentHairState = true;
        ChaControl currentBody;
        Vector2 angle;
        Vector2 rot;
        float offset;
        Transform neckBone;
        Transform leftEye;
        Transform rightEye;
        float nearClip = 0.005f;
        float lastFOV;
        float lastNearClip;
        Quaternion lastRotation;
        Vector3 lastPosition;
        bool lastDOF;
        bool hideObstacle;
        NECK_LOOK_TYPE_VER2 lastNeck;
        DragManager dragManager;

        public bool povActive = false;
        public static CommonView instance;

        void Awake()
        {
            instance = this;
            dragManager = gameObject.AddComponent<DragManager>();
            currentfov = TogglePOV.DefaultFov.Value;
            SHOW_HAIR = TogglePOV.ShowHair.Value;
            MALE_OFFSET = TogglePOV.MaleOffset.Value;
            FEMALE_OFFSET = TogglePOV.FemaleOffset.Value;
        }

        void OnDestroy()
        {
            Destroy(dragManager);

            if(currentBody != null)
                Restore();
        }

        void LateUpdate()
        {
            if(TogglePOV.POVKey.IsDown())
                SetPOV();

            if(currentBody == null && povActive)
            {
                Restore();
                Logger.Log(LogLevel.Info, "TogglePOV force reset");
            }
            else if(povActive)
            {
                currentBody.SetNeckLook(NECK_LOOK_TYPE_VER2.ANIMATION);
                UpdateCamera();
            }
        }

        public void SetPOV()
        {
            if(currentBody == null)
            {
                var chara = GetChara();
                if(chara)
                    Apply(chara);
            }
            else
            {
                Restore();
            }
        }

        public void Restore()
        {
            if(currentBody)
            {
                currentBody.SetNeckLook(lastNeck);

                if(!currentHairState)
                    ShowHair(true);

                currentBody = null;
            }

            if(Camera.main)
            {
                Camera.main.fieldOfView = lastFOV;
                Camera.main.nearClipPlane = lastNearClip;
                Camera.main.transform.rotation = lastRotation;
                Camera.main.transform.position = lastPosition;
            }

            DepthOfField = lastDOF;
            Shield = hideObstacle;
            CameraEnabled = true;
            povActive = false;
            currentHairState = true;
        }

        void ToggleHair()
        {
            SHOW_HAIR = !SHOW_HAIR;
            if(currentBody != null && currentHairState != SHOW_HAIR)
                ShowHair(SHOW_HAIR);
        }

        void UpdateCamera()
        {
            if(Input.GetMouseButton(1))
            {
                GameCursor.Instance.SetCursorLock(true);
                currentfov = Mathf.Clamp(currentfov + Input.GetAxis("Mouse X") * Time.deltaTime * 30f, 1f, MAXFOV);
            }
            else if(Input.GetMouseButton(0) && DragManager.allowCamera)
            {
                GameCursor.Instance.SetCursorLock(true);
                float rateaddspeed = 2.5f;
                float mx = Input.GetAxis("Mouse X") * rateaddspeed;
                float my = Input.GetAxis("Mouse Y") * rateaddspeed;
                rot += new Vector2(-my, mx) * new Vector2(sensitivityX, sensitivityY).magnitude;
            }
            else
            {
                GameCursor.Instance.SetCursorLock(false);
            }

            if(Input.GetKeyDown(KeyCode.Semicolon))
                currentfov = TogglePOV.DefaultFov.Value;

            if(Input.GetKey(KeyCode.Equals))
                currentfov = Mathf.Max(currentfov - Time.deltaTime * 15f, 5f);
            else if(Input.GetKey(KeyCode.RightBracket))
                currentfov = Mathf.Min(currentfov + Time.deltaTime * 15f, 120f);

            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                offset = Mathf.Min(offset + 0.0005f, 2f);

                if(currentBody.sex == 0)
                    MALE_OFFSET = offset;
                else
                    FEMALE_OFFSET = offset;
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                offset = Mathf.Max(offset - 0.0005f, -2f);

                if(currentBody.sex == 0)
                    MALE_OFFSET = offset;
                else
                    FEMALE_OFFSET = offset;
            }

            Camera.main.fieldOfView = currentfov;
            Camera.main.nearClipPlane = nearClip;
            DepthOfField = false;
            Shield = false;

            var neckLookCtrl = currentBody.neckLookCtrl;
            var neckLookScript = neckLookCtrl.neckLookScript;
            var param = neckLookScript.neckTypeStates[neckLookCtrl.ptnNo];
            angle = new Vector2(rot.x, rot.y);

            var last = neckLookScript.aBones.Length - 1;
            var bone = neckLookScript.aBones[last];
            RotateToAngle(param, last, bone);

            Camera.main.transform.rotation = neckBone.rotation;
            Camera.main.transform.position = (leftEye.position + rightEye.position) / 2f;
            Camera.main.transform.Translate(Vector3.forward * offset);
            float y = Mathf.Clamp(angle.y, -40f, 40f);
            float x = Mathf.Clamp(angle.x, -60f, 60f);
            angle -= new Vector2(x, y);
            Camera.main.transform.rotation *= Quaternion.Euler(x, y, 0f);
            rot = new Vector2(rot.x - angle.x, rot.y - angle.y);
        }

        void ShowHair(bool show)
        {
            currentHairState = show;
            currentBody.transform.FindLoop("cf_j_head")?.SetActive(show);
        }

        void Apply(ChaControl body)
        {
            currentBody = body;

            lastFOV = Camera.main.fieldOfView;
            lastNearClip = Camera.main.nearClipPlane;
            lastRotation = Camera.main.transform.rotation;
            lastPosition = Camera.main.transform.position;
            lastDOF = DepthOfField;
            hideObstacle = Shield;
            lastNeck = currentBody.GetNeckLook();

            if(!currentHairState)
                ShowHair(true);

            FindBones();
            angle = Vector2.zero;
            rot = Vector3.zero;
            offset = currentBody.sex == 0 ? MALE_OFFSET : FEMALE_OFFSET;

            if(!SHOW_HAIR)
                ShowHair(false);

            CameraEnabled = false;
            povActive = true;
        }

        void RotateToAngle(NeckTypeStateVer2 param, int boneNum, NeckObjectVer2 bone)
        {
            Vector2 b = default(Vector2);
            b.x = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.x);
            b.y = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.y);
            angle += b;

            float x, y;
            if(clampRotation)
            {
                var neckParam = param.aParam[boneNum];
                y = Mathf.Clamp(angle.y, neckParam.minBendingAngle, neckParam.maxBendingAngle);
                x = Mathf.Clamp(angle.x, neckParam.upBendingAngle, neckParam.downBendingAngle);
            }
            else
            {
                y = angle.y;
                x = angle.x;
            }

            angle -= new Vector2(x, y);
            float z = bone.neckBone.localEulerAngles.z;
            bone.neckBone.localRotation = Quaternion.Euler(x, y, z);
        }

        void FindBones()
        {
            foreach(NeckObjectVer2 neckObjectVer in currentBody.neckLookCtrl.neckLookScript.aBones)
            {
                if(neckObjectVer.neckBone.name.ToLower().Contains("head"))
                {
                    neckBone = neckObjectVer.neckBone;
                    break;
                }
            }

            foreach(EyeObject eyeObject in currentBody.eyeLookCtrl.eyeLookScript.eyeObjs)
            {
                switch(eyeObject.eyeLR)
                {
                    case EYE_LR.EYE_L:
                        leftEye = eyeObject.eyeTransform;
                        break;
                    case EYE_LR.EYE_R:
                        rightEye = eyeObject.eyeTransform;
                        break;
                }
            }
        }
    }
}

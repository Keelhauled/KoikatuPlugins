using BepInEx.Logging;
using IllusionUtility.GetUtility;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace TogglePOVKK
{
    internal abstract class BaseMono : MonoBehaviour
    {
        protected abstract bool CameraEnabled { get; set; }
        protected abstract Vector3 CameraTargetPos { get; }
        protected abstract bool DepthOfField { get; set; }
        protected abstract bool Shield { get; set; }
        protected abstract bool CameraStopMoving();
        protected abstract ChaInfo GetChara(Vector3 targetPos);

        private float sensitivityX = 0.5f;
        private float sensitivityY = 0.5f;
        private float MAXFOV = 120f;
        private float MALE_OFFSET;
        private float FEMALE_OFFSET;
        private bool SHOW_HAIR;
        private bool clampRotation = true;

        private float currentfov;
        private bool currentHairState = true;
        protected ChaInfo currentBody;
        private Vector2 angle;
        private Vector2 rot;
        private float offset;
        private Transform neckBone;
        private Transform leftEye;
        private Transform rightEye;
        private float nearClip = 0.005f;
        private float lastFOV;
        private float lastNearClip;
        private Quaternion lastRotation;
        private Vector3 lastPosition;
        private bool lastDOF;
        private bool hideObstacle;
        private NECK_LOOK_TYPE_VER2 lastNeck;
        public bool povActive = false;
        private DragManager dragManager;
        public static BaseMono instance;

        protected virtual void Awake()
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

        protected virtual void Update()
        {
            if(currentBody == null && povActive)
            {
                Restore();
                Logger.Log(LogLevel.Info, "TogglePOV reset");
            }

            if(TogglePOV.POVKey.IsDown())
                SetPOV();

            if(currentBody != null)
            {
                EyesNeckUpdate();
                UpdateCamera();
                UpdateCamPos();
            }
        }

        public void SetPOV()
        {
            if(currentBody == null)
            {
                var chara = GetChara(CameraTargetPos);
                if(chara)
                {
                    Backup(chara);
                    Apply(chara);
                }
            }
            else
            {
                Restore();
            }
        }

        protected void ToggleHair()
        {
            SHOW_HAIR = !SHOW_HAIR;
            if(currentBody != null && currentHairState != SHOW_HAIR)
                ShowHair(SHOW_HAIR);
        }

        private void UpdateCamera()
        {
            if(leftEye == null || rightEye == null)
            {
                Restore();
                return;
            }

            if(!CameraEnabled)
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
            }

            if(Input.GetKeyDown(KeyCode.Semicolon))
                currentfov = TogglePOV.DefaultFov.Value;

            if(Input.GetKey(KeyCode.Equals))
                currentfov = Mathf.Max(currentfov - Time.deltaTime * 15f, 1f);
            else if(Input.GetKey(KeyCode.RightBracket))
                currentfov = Mathf.Min(currentfov + Time.deltaTime * 15f, 100f);

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
        }

        private void UpdateCamPos()
        {
            Camera.main.transform.rotation = neckBone.rotation;
            Camera.main.transform.position = (leftEye.position + rightEye.position) / 2f;
            Camera.main.transform.Translate(Vector3.forward * offset);
            float y = Mathf.Clamp(angle.y, -40f, 40f);
            float x = Mathf.Clamp(angle.x, -60f, 60f);
            angle -= new Vector2(x, y);
            Camera.main.transform.rotation *= Quaternion.Euler(x, y, 0f);
            rot = new Vector2(rot.x - angle.x, rot.y - angle.y);
        }

        private void Backup(ChaInfo body)
        {
            lastFOV = Camera.main.fieldOfView;
            lastNearClip = Camera.main.nearClipPlane;
            lastRotation = Camera.main.transform.rotation;
            lastPosition = Camera.main.transform.position;
            lastDOF = DepthOfField;
            hideObstacle = Shield;
            lastNeck = GetNeckLook(body);
        }

        public void Restore()
        {
            if(currentBody)
            {
                SetNeckLook(lastNeck);

                if(!currentHairState)
                    ShowHair(true);
            }

            currentBody = null;

            if(Camera.main)
            {
                Camera.main.fieldOfView = lastFOV;
                Camera.main.nearClipPlane = lastNearClip;
                Camera.main.transform.rotation = lastRotation;
                Camera.main.transform.position = lastPosition;
            }

            CameraEnabled = true;

            DepthOfField = lastDOF;
            Shield = hideObstacle;

            povActive = false;
            currentHairState = true;
        }

        private void ShowHair(bool show)
        {
            currentHairState = show;
            currentBody.transform.FindLoop("cf_j_head")?.SetActive(show);
            //currentBody.transform.FindLoop("cf_J_FaceBase")?.SetActive(show);
            //currentBody.transform.FindLoop("cf_O_mayuge")?.SetActive(show);
        }

        private void Apply(ChaInfo body)
        {
            CameraEnabled = false;

            if(!currentHairState)
                ShowHair(true);

            currentBody = body;
            FindNLR();
            angle = Vector2.zero;
            rot = Vector3.zero;
            offset = currentBody.sex == 0 ? MALE_OFFSET : FEMALE_OFFSET;
            //UpdateCamPos();

            if(!SHOW_HAIR)
                ShowHair(false);

            povActive = true;
        }

        private void RotateToAngle(NeckTypeStateVer2 param, int boneNum, NeckObjectVer2 bone)
        {
            Vector2 b = default(Vector2);
            b.x = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.x);
            b.y = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.y);
            angle += b;

            float x, y;
            if(clampRotation)
            {
                y = Mathf.Clamp(angle.y, param.aParam[boneNum].minBendingAngle, param.aParam[boneNum].maxBendingAngle);
                x = Mathf.Clamp(angle.x, param.aParam[boneNum].upBendingAngle, param.aParam[boneNum].downBendingAngle);
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

        private void EyesNeckUpdate()
        {
            //SetEyeLook(EYE_LOOK_TYPE.FORWARD);
            SetNeckLook(NECK_LOOK_TYPE_VER2.ANIMATION);
        }

        private void SetEyeLook(EYE_LOOK_TYPE eyetype)
        {
            for(int i = 0; i < currentBody.eyeLookCtrl.eyeLookScript.eyeTypeStates.Length; i++)
            {
                if(currentBody.eyeLookCtrl.eyeLookScript.eyeTypeStates[i].lookType == eyetype)
                {
                    currentBody.eyeLookCtrl.ptnNo = i;
                    return;
                }
            }
        }

        private void SetNeckLook(NECK_LOOK_TYPE_VER2 necktype)
        {
            for(int i = 0; i < currentBody.neckLookCtrl.neckLookScript.neckTypeStates.Length; i++)
            {
                if(currentBody.neckLookCtrl.neckLookScript.neckTypeStates[i].lookType == necktype)
                    currentBody.neckLookCtrl.ptnNo = i;
            }
        }

        private NECK_LOOK_TYPE_VER2 GetNeckLook(ChaInfo body)
        {
            return body.neckLookCtrl.neckLookScript.neckTypeStates[body.neckLookCtrl.ptnNo].lookType;
        }

        private void FindNLR()
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

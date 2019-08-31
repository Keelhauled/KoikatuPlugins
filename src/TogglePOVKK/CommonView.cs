using BepInEx.Logging;
using UnityEngine;

namespace TogglePOVKK
{
    internal abstract class CommonView : MonoBehaviour
    {
        public const float MinFov = 5f;
        public const float MaxFov = 120f;

        protected abstract bool CameraEnabled { get; set; }
        protected abstract bool DepthOfField { get; set; }
        protected abstract bool Shield { get; set; }
        protected abstract ChaControl GetChara();

        private float backupFov;
        private float backupNearClip;
        private Quaternion backupRot;
        private Vector3 backupPos;
        private bool backupShield;
        private bool backupDOF;
        private NECK_LOOK_TYPE_VER2 backupNeck;

        private float currentFov;
        private ChaControl currentChara;
        private Transform neckBone;
        private Transform leftEye;
        private Transform rightEye;
        private Vector2 angle;
        private Vector2 rot;
        private DragManager dragManager;
        public bool povActive = false;
        public static CommonView instance;

        private void Awake()
        {
            instance = this;
            dragManager = gameObject.AddComponent<DragManager>();
            currentFov = TogglePOV.DefaultFov.Value;
        }

        private void OnDestroy()
        {
            Destroy(dragManager);

            if(currentChara != null)
                Restore();
        }

        private void LateUpdate()
        {
            if(TogglePOV.POVKey.IsDown())
                SetPOV();

            if(currentChara == null && povActive)
            {
                Restore();
                Logger.Log(LogLevel.Info, "TogglePOV force reset");
            }
            else if(povActive)
            {
                currentChara.SetNeckLook(NECK_LOOK_TYPE_VER2.ANIMATION);
                UpdateCamera();
            }
        }

        public void SetPOV()
        {
            if(currentChara == null)
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

        private void Apply(ChaControl chara)
        {
            currentChara = chara;

            backupFov = Camera.main.fieldOfView;
            backupNearClip = Camera.main.nearClipPlane;
            backupRot = Camera.main.transform.rotation;
            backupPos = Camera.main.transform.position;
            backupDOF = DepthOfField;
            backupShield = Shield;
            backupNeck = currentChara.GetNeckLook();

            FindBones();
            angle = Vector2.zero;
            rot = Vector3.zero;

            CameraEnabled = false;
            povActive = true;
        }

        public void Restore()
        {
            if(currentChara)
            {
                currentChara.SetNeckLook(backupNeck);
                currentChara = null;
            }

            if(Camera.main)
            {
                Camera.main.fieldOfView = backupFov;
                Camera.main.nearClipPlane = backupNearClip;
                Camera.main.transform.rotation = backupRot;
                Camera.main.transform.position = backupPos;
            }

            DepthOfField = backupDOF;
            Shield = backupShield;
            CameraEnabled = true;
            povActive = false;
        }

        private void UpdateCamera()
        {
            if(Input.GetMouseButton(1))
            {
                GameCursor.Instance.SetCursorLock(true);
                currentFov = Mathf.Clamp(currentFov + Input.GetAxis("Mouse X") * Time.deltaTime * 30f, MinFov, MaxFov);
            }
            else if(Input.GetMouseButton(0) && DragManager.allowCamera)
            {
                GameCursor.Instance.SetCursorLock(true);
                var mouseDir = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * 2.5f;
                var mouseSens = new Vector2(TogglePOV.HorizontalSensitivity.Value, TogglePOV.VerticalSensitivity.Value).magnitude;
                rot += mouseDir * mouseSens;
            }
            else
            {
                GameCursor.Instance.SetCursorLock(false);
            }

            if(Input.GetKeyDown(KeyCode.Semicolon))
                currentFov = TogglePOV.DefaultFov.Value;
            else if(Input.GetKey(KeyCode.Equals))
                currentFov = Mathf.Max(currentFov - Time.deltaTime * 15f, 5f);
            else if(Input.GetKey(KeyCode.RightBracket))
                currentFov = Mathf.Min(currentFov + Time.deltaTime * 15f, 120f);

            if(Input.GetKeyDown(KeyCode.UpArrow))
                TogglePOV.ViewOffset.Value = Mathf.Min(TogglePOV.ViewOffset.Value + 0.0005f, 2f);
            else if(Input.GetKeyDown(KeyCode.DownArrow))
                TogglePOV.ViewOffset.Value = Mathf.Max(TogglePOV.ViewOffset.Value - 0.0005f, -2f);

            Camera.main.fieldOfView = currentFov;
            Camera.main.nearClipPlane = TogglePOV.NearClipPov.Value;
            DepthOfField = false;
            Shield = false;

            var neckLookCtrl = currentChara.neckLookCtrl;
            var neckLookScript = neckLookCtrl.neckLookScript;
            var param = neckLookScript.neckTypeStates[neckLookCtrl.ptnNo];
            angle = new Vector2(rot.x, rot.y);

            var last = neckLookScript.aBones.Length - 1;
            var bone = neckLookScript.aBones[last];
            RotateToAngle(param, last, bone);

            Camera.main.transform.rotation = neckBone.rotation;
            Camera.main.transform.position = (leftEye.position + rightEye.position) / 2f;
            Camera.main.transform.Translate(Vector3.forward * TogglePOV.ViewOffset.Value);
            float y = Mathf.Clamp(angle.y, -40f, 40f);
            float x = Mathf.Clamp(angle.x, -60f, 60f);
            angle -= new Vector2(x, y);
            Camera.main.transform.rotation *= Quaternion.Euler(x, y, 0f);
            rot = new Vector2(rot.x - angle.x, rot.y - angle.y);
        }

        private void RotateToAngle(NeckTypeStateVer2 param, int boneNum, NeckObjectVer2 bone)
        {
            var delta = new Vector2();
            delta.x = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.x);
            delta.y = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.y);
            angle += delta;

            float x, y;
            if(TogglePOV.ClampRotation.Value)
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

        private void FindBones()
        {
            foreach(var neckObjectVer in currentChara.neckLookCtrl.neckLookScript.aBones)
            {
                if(neckObjectVer.neckBone.name.ToLower().Contains("head"))
                {
                    neckBone = neckObjectVer.neckBone;
                    break;
                }
            }

            foreach(var eyeObject in currentChara.eyeLookCtrl.eyeLookScript.eyeObjs)
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

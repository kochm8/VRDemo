//using UnityEngine;
//using UnityEditor;
//using RootMotion;
//using RootMotion.FinalIK;
//using System.Collections.Generic;
//using System;
//using Leap.Unity;


//namespace CpvrLab.VirtualTable
//{

//    [CustomEditor(typeof(AvatarConfigurationHelper))]
//    public class AvatarConfigurationHelperEditor : Editor
//    {

//        private AvatarConfigurationHelper script { get { return target as AvatarConfigurationHelper; } }

//        private GameObject modelRoot { get { return script.modelRootObject; } }
//        private bool settingsVisible = false;

//        // struct containing data needed during auto setup
//        class AutoSetupData
//        {
//            public FullBodyBipedIK fbbik;
//            public RiggedHand leapRiggedHandLeft;
//            public RiggedHand leapRiggedHandRight;

//            public Transform leapPalmLeft;
//            public Transform leapPalmRight;

//            public Transform leapHandLeft;
//            public Transform leapHandRight;

//            public Transform relaxedHandLeft;
//            public Transform relaxedHandRight;

//            public HandPoseLerp handPoseLerpLeft;
//            public HandPoseLerp handPoseLerpRight;

//            // exact finger data for leap hand
//            public Dictionary<HumanBodyBones, Transform> leapFingerCopies = new Dictionary<HumanBodyBones, Transform>();
//        }

//        private AutoSetupData setupData = new AutoSetupData();

//        public override void OnInspectorGUI()
//        {
//            settingsVisible = EditorGUILayout.Foldout(settingsVisible, "Settings");

//            if (settingsVisible)
//                DrawDefaultInspector();

//            DropAreaGUI();
//        }

//        private void AutoConfigModel(GameObject model)
//        {
//            // 1. create an instance of the model and add it to model root
//            GameObject modelInstance = Instantiate(model);
//            modelInstance.transform.position = Vector3.zero;
//            modelInstance.transform.rotation = Quaternion.identity;
//            modelInstance.transform.SetParent(modelRoot.transform, false);

//            // 2. set up finalIK on the model
//            SetupFBBIK();

//            // 3. set up the different hand goals needed 
//            SetupHandGoals();

//            // 4. set up ik goals (pole targets for elbows and knees etc.)
//            SetupIKGoals();

//            DestroyImmediate(script);
//        }

//        private void DropAreaGUI()
//        {
//            var evt = Event.current;

//            var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
//            GUI.Box(dropArea, "Drop your custom humanoid model here");

//            switch (evt.type)
//            {
//                case EventType.DragUpdated:
//                case EventType.DragPerform:
//                    if (!dropArea.Contains(evt.mousePosition))
//                        break;

//                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

//                    if (evt.type == EventType.DragPerform)
//                    {
//                        DragAndDrop.AcceptDrag();

//                        if (DragAndDrop.objectReferences.Length > 1)
//                        {

//                            Debug.LogError("Please drop only one object that is a valid humanoid model.");
//                            return;
//                        }

//                        var go = DragAndDrop.objectReferences.GetValue(0) as GameObject;

//                        if (!go || (go.GetComponent<Animator>() == null))
//                        {
//                            Debug.LogError("Please drop a valid humanoid model in the drop zone.");
//                            return;
//                        }

//                        AutoConfigModel(go);

//                    }
//                    break;
//            }
//        }

//        // returns bend direction of a three transform chain
//        Vector3 GetChainBendDirection(HumanBodyBones b0, HumanBodyBones b1, HumanBodyBones b2)
//        {
//            Transform t0 = animator.GetBoneTransform(b0);
//            Transform t1 = animator.GetBoneTransform(b1);
//            Transform t2 = animator.GetBoneTransform(b2);

//            Vector3 cross = Vector3.Cross((t1.transform.position - t0.transform.position).normalized, (t2.transform.position - t0.transform.position).normalized);
//            return -Vector3.Cross(cross.normalized, (t2.transform.position - t0.transform.position).normalized);
//        }

//        void SetupIKGoals()
//        {
//            Transform goalContainer = transform.FindChild(script.ikGoalsContainerName);
//            // destroy it if it already exists, just in case auto setup is run multiple times
//            if (goalContainer != null)
//            {
//                // @todo maybe prompt the user to decide to delete or abort here
//                DestroyImmediate(goalContainer.gameObject);
//                Debug.LogWarning("Gameobject with the name " + script.ikGoalsContainerName + " already exists, cleaning it up.");
//            }
//            // 1. Add a container object for our hand goals below modelRoot
//            goalContainer = CopyTransform(modelRoot.transform, script.ikGoalsContainerName);
//            goalContainer.SetParent(transform);

//            // Add pole targets for the knees
//            Transform elbowGoalLeft = CopyTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), "elbow_L").transform;
//            Transform elbowGoalRight = CopyTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), "elbow_R").transform;

//            elbowGoalLeft.SetParent(goalContainer);
//            elbowGoalRight.SetParent(goalContainer);

//            // offset the goals in the bend direction of the arm
//            elbowGoalLeft.position = elbowGoalLeft.position + GetChainBendDirection(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
//            elbowGoalRight.position = elbowGoalRight.position + GetChainBendDirection(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);



//            setupData.fbbik.solver.leftArmChain.bendConstraint.bendGoal = elbowGoalLeft;
//            setupData.fbbik.solver.leftArmChain.bendConstraint.weight = 0.5f;
//            setupData.fbbik.solver.rightArmChain.bendConstraint.bendGoal = elbowGoalRight;
//            setupData.fbbik.solver.rightArmChain.bendConstraint.weight = 0.5f;


//            Transform kneeGoalLeft = CopyTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), "knee_L").transform;
//            Transform kneeGoalRight = CopyTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), "knee_R").transform;

//            kneeGoalLeft.SetParent(goalContainer);
//            kneeGoalRight.SetParent(goalContainer);

//            // offset the goals in the bend direction of the arm
//            kneeGoalLeft.position = kneeGoalLeft.position + GetChainBendDirection(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
//            kneeGoalRight.position = kneeGoalRight.position + GetChainBendDirection(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);

//            setupData.fbbik.solver.leftLegChain.bendConstraint.bendGoal = kneeGoalLeft;
//            setupData.fbbik.solver.leftLegChain.bendConstraint.weight = 0.5f;
//            setupData.fbbik.solver.rightLegChain.bendConstraint.bendGoal = kneeGoalRight;
//            setupData.fbbik.solver.rightLegChain.bendConstraint.weight = 0.5f;

//            //
//            Transform footGoalLeft = CopyTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), "foot_L");
//            Transform footGoalRight = CopyTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), "foot_R");

//            footGoalLeft.SetParent(goalContainer);
//            footGoalRight.SetParent(goalContainer);

//            // We set the target in the FBBIK but we leave the weights at zero for the feet.
//            setupData.fbbik.solver.SetEffectorWeights(FullBodyBipedEffector.LeftFoot, 0.0f, 0.0f);
//            setupData.fbbik.solver.leftFootEffector.target = footGoalLeft;
//            setupData.fbbik.solver.SetEffectorWeights(FullBodyBipedEffector.RightFoot, 0.0f, 0.0f);
//            setupData.fbbik.solver.rightFootEffector.target = footGoalRight;

//            // Setup head effector
//            Transform headEffector = CopyTransform(animator.GetBoneTransform(HumanBodyBones.Head).transform, "headEffector");
//            headEffector.gameObject.AddComponent<FBBIKHeadEffector>().ik = setupData.fbbik;

//            headEffector.SetParent(script.headMount);
//            // @todo    for the test models the value below worked okay but it wont work for most others I guess. So expose this initial head effector setting or something...
//            headEffector.localPosition = new Vector3(0.0f, -0.215f, -0.197f);

//        }

//        void SetupHandGoals()
//        {
//            Transform handContainer = transform.FindChild(script.handContainerName);
//            // destroy it if it already exists, just in case auto setup is run multiple times
//            if (handContainer != null)
//            {
//                // @todo maybe prompt the user to decide to delete or abort here
//                DestroyImmediate(handContainer.gameObject);
//                Debug.LogWarning("Gameobject with the name " + script.handContainerName + " already exists, cleaning it up.");
//            }

//            // 1. Add a container object for our hand goals below modelRoot
//            handContainer = CopyTransform(modelRoot.transform, script.handContainerName);
//            handContainer.SetParent(transform);

//            // 2. Get the references for left and right hand in the models skeleton       
//            Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
//            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

//            // 3. create leap hand object by making a copy of the models hand       
//            setupData.leapHandLeft = CopyTransformTree(leftHand, script.leapHandName + "_L", LeapFingerCheckAndSet);
//            setupData.leapHandRight = CopyTransformTree(rightHand, script.leapHandName + "_R", LeapFingerCheckAndSet);

//            // 4. Call the special leap hand setup function to configure them for the leap controller
//            SetupLeapHand(setupData.leapHandLeft, HumanBodyBones.LeftHand);
//            SetupLeapHand(setupData.leapHandRight, HumanBodyBones.RightHand);

//            // 5. copy the hand again for the user to use as a relaxed pose
//            setupData.relaxedHandLeft = CopyTransformTree(leftHand, script.relaxedHandName + "_L");
//            setupData.relaxedHandRight = CopyTransformTree(rightHand, script.relaxedHandName + "_R");

//            // 6. And again this is our final hand that acts as IK goal and is able to lerp between leap and relax state
//            Transform lerpedHandGoalLeft = CopyTransformTree(leftHand, script.handGoalName + "_L");
//            Transform lerpedHandGoalRight = CopyTransformTree(rightHand, script.handGoalName + "_R");


//            // add the debug transform tree to visualize the invisible hands in the editor
//            setupData.leapPalmLeft.gameObject.AddComponent<DebugTransformTree>().color = script.leapHandDebugColor;
//            setupData.leapPalmRight.gameObject.AddComponent<DebugTransformTree>().color = script.leapHandDebugColor;

//            setupData.relaxedHandLeft.gameObject.AddComponent<DebugTransformTree>().color = script.relaxedHandDebugColor;
//            setupData.relaxedHandRight.gameObject.AddComponent<DebugTransformTree>().color = script.relaxedHandDebugColor;

//            lerpedHandGoalLeft.gameObject.AddComponent<DebugTransformTree>().color = script.handGoalDebugColor;
//            lerpedHandGoalRight.gameObject.AddComponent<DebugTransformTree>().color = script.handGoalDebugColor;

//            // set handContainer as parent for the hands
//            setupData.leapPalmLeft.SetParent(handContainer);
//            setupData.leapPalmRight.SetParent(handContainer);

//            setupData.relaxedHandLeft.SetParent(handContainer);
//            setupData.relaxedHandRight.SetParent(handContainer);

//            lerpedHandGoalLeft.SetParent(handContainer);
//            lerpedHandGoalRight.SetParent(handContainer);


//            // Add a hand poser script to the actual skeleton hand bones to match the current hand goal
//            leftHand.gameObject.AddComponent<HandPoser>().poseRoot = lerpedHandGoalLeft;
//            rightHand.gameObject.AddComponent<HandPoser>().poseRoot = lerpedHandGoalRight;

//            // The handGoal hands need to be able to lerp between relaxed and leap hand state
//            setupData.handPoseLerpLeft = lerpedHandGoalLeft.gameObject.AddComponent<HandPoseLerp>();
//            setupData.handPoseLerpLeft.poseRootA = setupData.relaxedHandLeft;
//            setupData.handPoseLerpLeft.poseRootB = setupData.leapHandLeft;

//            setupData.handPoseLerpRight = lerpedHandGoalRight.gameObject.AddComponent<HandPoseLerp>();
//            setupData.handPoseLerpRight.poseRootA = setupData.relaxedHandRight;
//            setupData.handPoseLerpRight.poseRootB = setupData.leapHandRight;
//        }

//        void SetupFBBIK()
//        {

//            // 2. add finalIK to the model and set its reference
//            FullBodyBipedIK fbbik = transform.GetComponentInChildren<FullBodyBipedIK>();

//            if (fbbik == null)
//            {
//                // There is no model ready for setup
//                if (transform.childCount < 1)
//                {
//                    Debug.LogError("Can't auto configure avatar: Add your model as a child object to 'modelRoot' to proceed.");
//                    EditorGUILayout.HelpBox("Can't auto configure avatar: Add your model as a child object to 'modelRoot' to proceed.", MessageType.Error);
//                    return;
//                }
//                else if (transform.childCount > 1)
//                {
//                    Debug.LogError("Can't auto configure avatar: There are too many child objects under 'modelRoot'.");
//                    EditorGUILayout.HelpBox("Can't auto configure avatar: There are too many child objects under 'modelRoot'.", MessageType.Error);
//                    return;
//                }

//                // we only get here if there is only one child which must be our model
//                Transform child = transform.GetChild(0);
//                fbbik = child.gameObject.AddComponent<FullBodyBipedIK>();

//                // force auto detect of fbbik
//                if (fbbik.references.isEmpty)
//                {
//                    BipedReferences.AutoDetectReferences(ref fbbik.references, fbbik.transform, new BipedReferences.AutoDetectParams(true, false));
//                    fbbik.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(fbbik.references);
//                    fbbik.solver.SetToReferences(fbbik.references, fbbik.solver.rootNode);
//                }
//            }

//            setupData.fbbik = fbbik;
//        }

//        void LeapFingerCheckAndSet(Transform original, Transform copy)
//        {
//            // currently the layout of HumanBody bones for the fingers uses a sequence from left thumb to right little finger
//            // we go through all of those and see if our current bone is a finger bone. then we put the copy in our reference
//            // to identify him at a later stage
//            int start = (int)(HumanBodyBones.LeftThumbProximal);
//            int end = (int)(HumanBodyBones.RightLittleDistal);
//            for (int i = start; i <= end; i++)
//            {
//                HumanBodyBones key = (HumanBodyBones)i;
//                if (animator.GetBoneTransform(key) == original)
//                    setupData.leapFingerCopies.Add(key, copy);
//            }
//        }

//        void SetupLeapHand(Transform hand, HumanBodyBones bone)
//        {
//            bool isLeft = (bone == HumanBodyBones.LeftHand);

//            // 1. Add an extra transform as the new root of the leap hand copy
//            //      this transform can be manipulated by the user later to change the 
//            //      leap hand position
//            Transform palm = CopyTransform(hand, script.leapPalmName + ((isLeft) ? "_L" : "_R"));
//            hand.SetParent(palm);



//            // calculate palmFacing and fingerPointing properties for the leap stuff
//            Vector3 indexProx = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position;
//            Vector3 ringProx = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal).position;
//            bool invert = script.invertLeftHandPalmVector;
//            if (!isLeft)
//            {
//                indexProx = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position;
//                ringProx = animator.GetBoneTransform(HumanBodyBones.RightRingProximal).position;

//                invert = script.invertRightHandPalmVector;
//            }

//            Vector3 planeFacing = calculatePlaneNormal(palm.position, indexProx, ringProx, invert);

//            // calculate finger pointing vector for palm by using the directional vector from palm to average finger pos
//            Vector3 averageFingerPos = (indexProx + ringProx) * 0.5f;
//            Vector3 palmFingerPointing = (averageFingerPos - palm.position).normalized;


//            // Add the leap hand component to the palm
//            RiggedHand rh = palm.gameObject.AddComponent<RiggedHand>();

//            // in local space of course
//            rh.modelFingerPointing = Quaternion.Inverse(palm.rotation) * palmFingerPointing;
//            rh.modelPalmFacing = Quaternion.Inverse(palm.rotation) * planeFacing;

//            // set the palm reference in the riggedhand script
//            rh.palm = palm;


//            // we expect the HumanBodyBones to have the finger definitions layed out from LeftThumbProximal to RightLittleDistal
//            // furthermore we expect the finger triples to be layed out in the correct order after each other (proximal -> intermediate -> distal)
//            // if unity changes something in the future this might break. So it would be advisable to map the finger bones
//            // from unity to some that are layed out like we expect
//            // @todo    make this future proof by mapping the human body bones to our own enum that is layed out as we expect and need it to be.
//            int start = (int)(HumanBodyBones.LeftThumbProximal);
//            int end = (int)(HumanBodyBones.LeftLittleProximal);
//            if (!isLeft)
//            {
//                start = (int)(HumanBodyBones.RightThumbProximal);
//                end = (int)(HumanBodyBones.RightLittleProximal);
//            }

//            int fingerIndex = 0;
//            for (int i = start; i <= end; i += 3)
//            {
//                Transform proximal = null;
//                Transform intermediate = null;
//                Transform distal = null;

//                if (!setupData.leapFingerCopies.TryGetValue((HumanBodyBones)i, out proximal))
//                    continue;
//                setupData.leapFingerCopies.TryGetValue((HumanBodyBones)i + 1, out intermediate);
//                setupData.leapFingerCopies.TryGetValue((HumanBodyBones)i + 2, out distal);

//                RiggedFinger rf = proximal.gameObject.AddComponent<RiggedFinger>();

//                // calculate finger pointing
//                Vector3 fingerPointing = (intermediate.position - proximal.position).normalized;


//                // set the default finger pointing and palm facing vectors
//                rf.modelFingerPointing = Quaternion.Inverse(proximal.rotation) * fingerPointing;
//                rf.modelPalmFacing = Quaternion.Inverse(proximal.rotation) * planeFacing;

//                // we only ever set up the front 3 finger bones
//                rf.bones[1] = proximal;
//                rf.bones[2] = intermediate;
//                rf.bones[3] = distal;

//                // assign the rigged fingers to the rigged hand script
//                rh.fingers[fingerIndex] = rf;
//                fingerIndex++;
//            }


//            // set the data
//            if (isLeft)
//            {
//                setupData.leapRiggedHandLeft = rh;
//                setupData.leapPalmLeft = palm;
//            }
//            else
//            {
//                setupData.leapRiggedHandRight = rh;
//                setupData.leapPalmRight = palm;
//            }
//        }

//        // calculate plane normal given three world positions
//        private Vector3 calculatePlaneNormal(Vector3 a, Vector3 b, Vector3 c, bool invert = false)
//        {
//            Vector3 ab = b - a;
//            Vector3 ac = c - a;

//            Vector3 cross = Vector3.Cross(ab, ac);
//            if (invert)
//                cross *= -1;

//            return cross.normalized;
//        }
//        struct TransformPair
//        {
//            public TransformPair(Transform o, Transform c)
//            {
//                orig = o;
//                copy = c;
//            }

//            public Transform orig;
//            public Transform copy;
//        }

//        Transform CopyTransformTree(Transform otherRoot, string newName = null, Action<Transform, Transform> copyMade = null)
//        {
//            string name = (newName != null) ? newName : otherRoot.name;

//            Stack<TransformPair> stack = new Stack<TransformPair>();

//            Transform root = CopyTransform(otherRoot, name);
//            stack.Push(new TransformPair(otherRoot, root));

//            while (stack.Count > 0)
//            {
//                TransformPair parentPair = stack.Pop();

//                for (int i = 0; i < parentPair.orig.childCount; i++)
//                {
//                    Transform child = parentPair.orig.GetChild(i);
//                    Transform childCopy = CopyTransform(child);
//                    childCopy.SetParent(parentPair.copy);

//                    // notify callback about the copy
//                    if (copyMade != null)
//                        copyMade(child, childCopy);

//                    stack.Push(new TransformPair(child, childCopy));
//                }
//            }

//            return root;
//        }

//        Transform CopyTransform(Transform other, string newName = null)
//        {
//            string name = (newName != null) ? newName : other.name;
//            GameObject go = new GameObject(name);
//            go.transform.rotation = other.rotation;
//            go.transform.position = other.position;
//            go.transform.localScale = other.localScale;

//            return go.transform;
//        }
//    }
//}
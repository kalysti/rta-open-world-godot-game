using Godot;
using System.Collections.Generic;
using System.Linq;

namespace UMA.DNA
{
    public class HumanMaleConverter : HumanBaseConverter
    {
        public void Adjust(UMADnaHumanoid umaDna)
        {
            SetBoneScale("headAdjust",
                              new Vector3(
                Mathf.Clamp(1, 1, 1),
                Mathf.Clamp(1 + (umaDna.headWidth - 0.5f) * 0.30f, 0.5f, 1.6f),
                Mathf.Clamp(1, 1, 1)));

            SetBoneScale("neckAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.neckThickness - 0.5f) * 0.80f, 0.5f, 1.6f),
                Mathf.Clamp(1 + (umaDna.neckThickness - 0.5f) * 1.2f, 0.5f, 1.6f)));

            SetBoneScale("leftOuterBreast",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f),
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f),
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f)));
            SetBoneScale("rightOuterBreast",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f),
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f),
                Mathf.Clamp(1 + (umaDna.breastSize - 0.5f) * 1.50f + (umaDna.upperWeight - 0.5f) * 0.10f, 0.6f, 1.5f)));

            SetBoneScale("leftEye",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f),
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f),
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f)));
            SetBoneScale("rightEye",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f),
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f),
                Mathf.Clamp(1 + (umaDna.eyeSize - 0.5f) * 0.3f, 0.7f, 1.4f)));

            SetBoneRotation("leftEyeAdjust",
                                new Quat(new Vector3((umaDna.eyeRotation - 0.5f) * 20, 0, 0)));
            SetBoneRotation("rightEyeAdjust",
                                 new Quat(new Vector3(-(umaDna.eyeRotation - 0.5f) * 20, 0, 0)));

            SetBoneScale("spine1Adjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(0.9f + (umaDna.upperWeight - 0.5f) * 0.10f + (umaDna.upperMuscle - 0.5f) * 0.5f, 0.45f, 1.50f),
                Mathf.Clamp(0.7f + (umaDna.upperWeight - 0.5f) * 0.45f + (umaDna.upperMuscle - 0.5f) * 0.45f, 0.55f, 1.15f)));

            SetBoneScale("spineAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(0.9f + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.45f, 0.75f, 1.350f),
                Mathf.Clamp(0.8f + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.25f, 0.75f, 1.350f)));

            SetBoneScale("lowerBackBelly",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.belly - 0.5f) * 1.0f, 0.35f, 1.75f),
                Mathf.Clamp(1 + (umaDna.belly - 0.5f) * 0.35f, 0.35f, 1.75f),
                Mathf.Clamp(1 + (umaDna.belly - 0.5f) * 1.25f, 0.35f, 1.75f)));

            SetBoneScale("lowerBackAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.25f + (umaDna.lowerWeight - 0.5f) * 0.15f, 0.85f, 1.5f),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.25f + (umaDna.lowerWeight - 0.5f) * 0.15f + (umaDna.waist - 0.5f) * 0.45f, 0.65f, 1.75f),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.25f + (umaDna.lowerWeight - 0.5f) * 0.15f, 0.85f, 1.5f)));

            SetBoneScale("leftTrapezius",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f),
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f),
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f)));
            SetBoneScale("rightTrapezius",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f),
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f),
                Mathf.Clamp(1 + (umaDna.upperMuscle - 0.5f) * 1.35f, 0.65f, 1.35f)));

            SetBoneScale("leftArmAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.armWidth - 0.5f) * 0.65f, 0.65f, 1.65f),
                Mathf.Clamp(1 + (umaDna.armWidth - 0.5f) * 0.65f, 0.65f, 1.65f)));
            SetBoneScale("rightArmAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.armWidth - 0.5f) * 0.65f, 0.65f, 1.65f),
                Mathf.Clamp(1 + (umaDna.armWidth - 0.5f) * 0.65f, 0.65f, 1.65f)));

            SetBoneScale("leftForeArmAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.65f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.65f, 0.75f, 1.25f)));
            SetBoneScale("rightForeArmAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.65f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.65f, 0.75f, 1.25f)));

            SetBoneScale("leftForeArmTwistAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.35f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.35f, 0.75f, 1.25f)));
            SetBoneScale("rightForeArmTwistAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.35f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.forearmWidth - 0.5f) * 0.35f, 0.75f, 1.25f)));

            SetBoneScale("leftShoulderAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.55f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.55f, 0.75f, 1.25f)));
            SetBoneScale("rightShoulderAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.55f, 0.75f, 1.25f),
                Mathf.Clamp(1 + (umaDna.upperWeight - 0.5f) * 0.35f + (umaDna.upperMuscle - 0.5f) * 0.55f, 0.75f, 1.25f)));

            SetBoneScale("leftUpLegAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.45f + (umaDna.lowerMuscle - 0.5f) * 0.15f - (umaDna.legsSize - 0.5f), 0.45f, 1.15f),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.45f + (umaDna.lowerMuscle - 0.5f) * 0.15f - (umaDna.legsSize - 0.5f), 0.45f, 1.15f)));
            SetBoneScale("rightUpLegAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.45f + (umaDna.lowerMuscle - 0.5f) * 0.15f - (umaDna.legsSize - 0.5f), 0.45f, 1.15f),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.45f + (umaDna.lowerMuscle - 0.5f) * 0.15f - (umaDna.legsSize - 0.5f), 0.45f, 1.15f)));

            SetBoneScale("leftLegAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.15f + (umaDna.lowerMuscle - 0.5f) * 0.95f - (umaDna.legsSize - 0.5f), 0.65f, 1.45f),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.15f + (umaDna.lowerMuscle - 0.5f) * 0.75f - (umaDna.legsSize - 0.5f), 0.65f, 1.45f)));
            SetBoneScale("rightLegAdjust",
                              new Vector3(
                Mathf.Clamp(1, 0.6f, 2),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.15f + (umaDna.lowerMuscle - 0.5f) * 0.95f - (umaDna.legsSize - 0.5f), 0.65f, 1.45f),
                Mathf.Clamp(1 + (umaDna.lowerWeight - 0.5f) * 0.15f + (umaDna.lowerMuscle - 0.5f) * 0.75f - (umaDna.legsSize - 0.5f), 0.65f, 1.45f)));

            SetBoneScale("leftGluteus",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f),
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f),
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f)));
            SetBoneScale("rightGluteus",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f),
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f),
                Mathf.Clamp(1 + (umaDna.gluteusSize - 0.5f) * 1.35f, 0.25f, 2.35f)));

            SetBoneScale("leftEarAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f),
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f),
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f)));

            SetBoneScale("rightEarAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f),
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f),
                Mathf.Clamp(1 + (umaDna.earsSize - 0.5f) * 1.0f, 0.75f, 1.5f)));

            SetBonePositionRelative("leftEarAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.headWidth - 0.5f) * -0.01f, -0.01f, 0.01f),
                Mathf.Clamp(0 + (umaDna.headWidth - 0.5f) * -0.03f, -0.03f, 0.03f),
                Mathf.Clamp(0 + (umaDna.earsPosition - 0.5f) * 0.02f, -0.02f, 0.02f)));

            SetBonePositionRelative("rightEarAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.headWidth - 0.5f) * -0.01f, -0.01f, 0.01f),
                Mathf.Clamp(0 + (umaDna.headWidth - 0.5f) * 0.03f, -0.03f, 0.03f),
                Mathf.Clamp(0 + (umaDna.earsPosition - 0.5f) * 0.02f, -0.02f, 0.02f)));

            SetBoneRotation("leftEarAdjust",
                                new Quat(new Vector3(
                Mathf.Clamp(0, -30, 80),
                Mathf.Clamp(0, -30, 80),
                Mathf.Clamp((umaDna.earsRotation - 0.5f) * 40, -15, 40))));
            SetBoneRotation("rightEarAdjust",
                                 new Quat(new Vector3(
                Mathf.Clamp(0, -30, 80),
                Mathf.Clamp(0, -30, 80),
                Mathf.Clamp((umaDna.earsRotation - 0.5f) * -40, -40, 15))));

            SetBoneScale("noseBaseAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.noseSize - 0.5f) * 1.5f, 0.25f, 3.0f),
                Mathf.Clamp(1 + (umaDna.noseSize - 0.5f) * 0.15f + (umaDna.noseWidth - 0.5f) * 1.0f, 0.25f, 3.0f),
                Mathf.Clamp(1 + (umaDna.noseSize - 0.5f) * 0.15f + (umaDna.noseFlatten - 0.5f) * 0.75f, 0.25f, 3.0f)));
            SetBoneScale("noseMiddleAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.noseCurve - 0.5f) * 1.5f + (umaDna.noseSize - 0.5f) * 1.0f, 0.5f, 3.0f),
                Mathf.Clamp(1 + (umaDna.noseCurve - 0.5f) * 0.15f + (umaDna.noseSize - 0.5f) * 0.25f + (umaDna.noseWidth - 0.5f) * 0.5f, 0.5f, 3.0f),
                Mathf.Clamp(1 + (umaDna.noseCurve - 0.5f) * 0.15f + (umaDna.noseSize - 0.5f) * 0.10f, 0.5f, 3.0f)));
            SetBoneRotation("noseBaseAdjust",
                                 new Quat(new Vector3(
                Mathf.Clamp(0, -30, 80),
                Mathf.Clamp((umaDna.noseInclination - 0.5f) * 60, -60, 30),
                Mathf.Clamp(0, -30, 80))));
            SetBonePositionRelative("noseBaseAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.nosePronounced - 0.5f) * -0.025f, -0.025f, 0.025f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.nosePosition - 0.5f) * 0.025f, -0.025f, 0.025f)));
            SetBonePositionRelative("noseMiddleAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.nosePronounced - 0.5f) * -0.012f, -0.012f, 0.012f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.nosePosition - 0.5f) * 0.015f, -0.015f, 0.015f)));

            SetBonePositionRelative("leftNoseAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.nosePronounced - 0.5f) * -0.025f, -0.025f, 0.025f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.nosePosition - 0.5f) * 0.025f, -0.025f, 0.025f)));
            SetBonePositionRelative("rightNoseAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.nosePronounced - 0.5f) * -0.025f, -0.025f, 0.025f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.nosePosition - 0.5f) * 0.025f, -0.025f, 0.025f)));

            SetBonePositionRelative("upperLipsAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.nosePosition - 0.5f) * 0.0045f, -0.0045f, 0.0045f)));

            SetBoneScale("mandibleAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.chinPronounced - 0.5f) * 0.18f, 0.55f, 1.75f),
                Mathf.Clamp(1 + (umaDna.chinSize - 0.5f) * 1.3f, 0.75f, 1.3f),
                Mathf.Clamp(1, 0.4f, 1.5f)));
            SetBonePositionRelative("mandibleAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.0125f, 0.0125f),
                Mathf.Clamp(0, -0.0125f, 0.0125f),
                Mathf.Clamp(0 + (umaDna.chinPosition - 0.5f) * 0.0075f, -0.0075f, 0.0075f)));

            SetBonePositionRelative("leftLowMaxilarAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.jawsSize - 0.5f) * 0.025f, -0.025f, 0.025f),
                Mathf.Clamp(0 + (umaDna.jawsPosition - 0.5f) * 0.03f, -0.03f, 0.03f)));
            SetBonePositionRelative("rightLowMaxilarAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.jawsSize - 0.5f) * -0.025f, -0.025f, 0.025f),
                Mathf.Clamp(0 + (umaDna.jawsPosition - 0.5f) * 0.03f, -0.03f, 0.03f)));

            SetBoneScale("leftCheekAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f),
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f),
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f)));
            SetBoneScale("rightCheekAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f),
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f),
                Mathf.Clamp(1 + (umaDna.cheekSize - 0.5f) * 1.05f, 0.35f, 2.05f)));
            SetBonePositionRelative("leftCheekAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.cheekPosition - 0.5f) * 0.03f, -0.03f, 0.03f)));
            SetBonePositionRelative("rightCheekAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.cheekPosition - 0.5f) * 0.03f, -0.03f, 0.03f)));

            SetBonePositionRelative("leftLowCheekAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.lowCheekPronounced - 0.5f) * -0.07f, -0.07f, 0.07f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.lowCheekPosition - 0.5f) * 0.06f, -0.06f, 0.06f)));
            SetBonePositionRelative("rightLowCheekAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.lowCheekPronounced - 0.5f) * -0.07f, -0.07f, 0.07f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.lowCheekPosition - 0.5f) * 0.06f, -0.06f, 0.06f)));

            SetBonePositionRelative("noseTopAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.025f + (umaDna.foreheadSize - 0.5f) * -0.0015f, -0.015f, 0.0025f)));

            SetBonePositionRelative("leftEyebrowLowAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.02f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.015f, 0.005f)));
            SetBonePositionRelative("leftEyebrowMiddleAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.05f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.025f, 0.005f)));
            SetBonePositionRelative("leftEyebrowUpAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.007f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.010f, 0.005f)));

            SetBonePositionRelative("rightEyebrowLowAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.02f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.015f, 0.005f)));
            SetBonePositionRelative("RightEyebrowMiddleAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.05f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.025f, 0.005f)));
            SetBonePositionRelative("RightEyebrowUpAdjust",
                                 new Vector3(
                Mathf.Clamp(0 + (umaDna.foreheadSize - 0.5f) * -0.015f, -0.025f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.foreheadPosition - 0.5f) * -0.007f + (umaDna.foreheadSize - 0.5f) * -0.005f, -0.010f, 0.005f)));

            SetBoneScale("lipsSuperiorAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f)));
            SetBoneScale("lipsInferiorAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 1.0f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 1.0f, 0.65f, 1.5f)));

            SetBoneScale("leftLipsSuperiorMiddleAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f)));
            SetBoneScale("rightLipsSuperiorMiddleAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f)));
            SetBoneScale("leftLipsInferiorAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f)));
            SetBoneScale("rightLipsInferiorAdjust",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.05f, 1.0f, 1.05f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f),
                Mathf.Clamp(1 + (umaDna.lipsSize - 0.5f) * 0.9f, 0.65f, 1.5f)));

            SetBonePositionRelative("lipsInferiorAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.lipsSize - 0.5f) * -0.008f, -0.1f, 0.1f)));

            SetBonePositionRelative("leftLipsAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * 0.03f, -0.02f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f)));
            SetBonePositionRelative("rightLipsAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * -0.03f, -0.005f, 0.02f),
                Mathf.Clamp(0, -0.05f, 0.05f)));

            SetBonePositionRelative("leftLipsSuperiorMiddleAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * 0.007f, -0.02f, 0.005f),
                Mathf.Clamp(0, -0.05f, 0.05f)));
            SetBonePositionRelative("rightLipsSuperiorMiddleAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * -0.007f, -0.005f, 0.02f),
                Mathf.Clamp(0, -0.05f, 0.05f)));
            SetBonePositionRelative("leftLipsInferiorAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * 0.007f, -0.02f, 0.005f),
                Mathf.Clamp(0 + (umaDna.lipsSize - 0.5f) * -0.008f, -0.1f, 0.1f)));
            SetBonePositionRelative("rightLipsInferiorAdjust",
                                 new Vector3(
                Mathf.Clamp(0, -0.05f, 0.05f),
                Mathf.Clamp(0 + (umaDna.mouthSize - 0.5f) * -0.007f, -0.005f, 0.02f),
                Mathf.Clamp(0 + (umaDna.lipsSize - 0.5f) * -0.008f, -0.1f, 0.1f)));


            ////Bone structure change	
            float overallScale = 0.88f + (umaDna.height - 0.5f) * 1.0f + (umaDna.legsSize - 0.5f) * 1.0f;
            overallScale = Mathf.Clamp(overallScale, 0.5f, 2f);

            SetBoneScale("position", new Vector3(overallScale, overallScale, overallScale));
/*
            SetBonePositionRelative("position",
                                 new Vector3(
                Mathf.Clamp((umaDna.feetSize - 0.5f) * -0.20f, -0.15f, 0.0675f),
                Mathf.Clamp(0, -10, 10),
                Mathf.Clamp(0, -10, 10)));
                */

            float lowerBackScale = Mathf.Clamp(1 - (umaDna.legsSize - 0.5f) * 1.0f, 0.5f, 3.0f);
            SetBoneScale("lowerBack", new Vector3(lowerBackScale, lowerBackScale, lowerBackScale));

            SetBoneScale("head",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.headSize - 0.5f) * 2.0f, 0.5f, 2),
                Mathf.Clamp(1 + (umaDna.headSize - 0.5f) * 2.0f, 0.5f, 2),
                Mathf.Clamp(1 + (umaDna.headSize - 0.5f) * 2.0f, 0.5f, 2)));

            SetBoneScale("leftArm",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f)));
            SetBoneScale("rightArm",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.armLength - 0.5f) * 2.0f, 0.5f, 2.0f)));

            SetBoneScale("leftForeArm",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f)));
            SetBoneScale("rightForeArm",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.forearmLength - 0.5f) * 2.0f, 0.5f, 2.0f)));

            SetBoneScale("leftHand",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f)));
            SetBoneScale("rightHand",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.handsSize - 0.5f) * 2.0f, 0.5f, 2.0f)));

            SetBoneScale("leftFoot",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f)));
            SetBoneScale("rightFoot",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f),
                Mathf.Clamp(1 + (umaDna.feetSize - 0.5f) * 2.0f, 0.5f, 2.0f)));

            SetBonePositionRelative("leftUpLeg",
                                 new Vector3(
                Mathf.Clamp(0, -10, 10),
                Mathf.Clamp((umaDna.legSeparation - 0.5f) * -0.15f + (umaDna.lowerWeight - 0.5f) * -0.035f + (umaDna.legsSize - 0.5f) * 0.1f, -0.055f, 0.055f),
                Mathf.Clamp(0, -10, 10)));
            SetBonePositionRelative("rightUpLeg",
                                 new Vector3(
                Mathf.Clamp(0, -10, 10),
                Mathf.Clamp((umaDna.legSeparation - 0.5f) * 0.15f + (umaDna.lowerWeight - 0.5f) * 0.035f + (umaDna.legsSize - 0.5f) * -0.1f, -0.055f, 0.055f),
                Mathf.Clamp(0, -10, 10)));

            SetBonePositionRelative("leftShoulder",
                                 new Vector3(
                Mathf.Clamp(0, -10, 10),
                Mathf.Clamp(-0.003f + (umaDna.upperMuscle - 0.5f) * -0.265f, -0.085f, 0.015f),
                Mathf.Clamp(0, -10, 10)));
            SetBonePositionRelative("rightShoulder",
                                 new Vector3(
                Mathf.Clamp(0, -10, 10),
                Mathf.Clamp(0.003f + (umaDna.upperMuscle - 0.5f) * 0.265f, -0.015f, 0.085f),
                Mathf.Clamp(0, -10, 10)));

            SetBoneScale("mandible",
                              new Vector3(
                Mathf.Clamp(1 + (umaDna.mandibleSize - 0.5f) * 0.35f, 0.35f, 1.35f),
                Mathf.Clamp(1 + (umaDna.mandibleSize - 0.5f) * 0.35f, 0.35f, 1.35f),
                Mathf.Clamp(1 + (umaDna.mandibleSize - 0.5f) * 0.35f, 0.35f, 1.35f)));

            /*
                        float raceHeight = umaData.umaRecipe.raceData.raceHeight;
                        float raceRadius = umaData.umaRecipe.raceData.raceRadius;
                        float raceMass = umaData.umaRecipe.raceData.raceMass;
                        umaData.characterHeight = raceHeight * overallScale * (0.425f + 0.6f * lowerBackScale) + ((umaDna.feetSize - 0.5f) * 0.25f);
                        umaData.characterRadius = raceRadius + ((umaDna.height - 0.5f) * 0.35f) + Mathf.Clamp01((umaDna.upperMuscle - 0.5f) * 0.19f);
                        umaData.characterMass = raceMass * overallScale + 26f * umaDna.upperWeight + 26f * umaDna.lowerWeight;
                        */
        }
    }
}
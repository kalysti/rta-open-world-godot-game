using Godot;

using Newtonsoft.Json;
namespace UMA
{

    [Tool]
    [JsonObject(MemberSerialization.OptIn)]
    public class UMADnaHumanoid : Resource
    {
        [Signal]
        public delegate void ValueChanged();

        private float _height = 0.5f;

        private float _headSize = 0.5f;


        private float _headWidth = 0.5f;


        private float _neckThickness = 0.5f;

        private float _armLength = 0.5f;

        private float _forearmLength = 0.5f;

        private float _armWidth = 0.5f;

        private float _forearmWidth = 0.5f;
        private float _handsSize = 0.5f;

        private float _feetSize = 0.5f;

        private float _legSeparation = 0.5f;

        private float _upperMuscle = 0.5f;


        private float _lowerMuscle = 0.5f;

        private float _upperWeight = 0.5f;

        private float _lowerWeight = 0.5f;

        private float _legsSize = 0.5f;

        private float _belly = 0.5f;

        private float _waist = 0.5f;

        private float _gluteusSize = 0.5f;
        private float _earsSize = 0.5f;

        private float _earsPosition = 0.5f;

        private float _earsRotation = 0.5f;

        private float _noseSize = 0.5f;

        private float _noseCurve = 0.5f;

        private float _noseWidth = 0.5f;

        private float _noseInclination = 0.5f;

        private float _nosePosition = 0.5f;

        private float _nosePronounced = 0.5f;

        private float _noseFlatten = 0.5f;
        private float _chinSize = 0.5f;

        private float _chinPronounced = 0.5f;

        private float _chinPosition = 0.5f;
        private float _mandibleSize = 0.5f;

        private float _jawsSize = 0.5f;

        private float _jawsPosition = 0.5f;
        private float _cheekSize = 0.5f;

        private float _cheekPosition = 0.5f;

        private float _lowCheekPronounced = 0.5f;

        private float _lowCheekPosition = 0.5f;
        private float _foreheadSize = 0.5f;

        private float _foreheadPosition = 0.5f;

        private float _lipsSize = 0.5f;

        private float _mouthSize = 0.5f;

        private float _eyeRotation = 0.5f;

        private float _eyeSize = 0.5f;
        private float _breastSize = 0.5f;

        [Export]
        [JsonProperty]        
        [UMACategoryAttribute(UMASlotCategory.Body, "Height")]

        public float height { get { return _height; } set { _height = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Size")]
        public float headSize { get { return _headSize; } set { _headSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Width")]
        public float headWidth { get { return _headWidth; } set { _headWidth = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Neck Thickness")]
        public float neckThickness { get { return _neckThickness; } set { _neckThickness = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Arm Length")]
        public float armLength { get { return _armLength; } set { _armLength = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Forearm Length")]
        public float forearmLength { get { return _forearmLength; } set { _forearmLength = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Arm Width")]
        public float armWidth { get { return _armWidth; } set { _armWidth = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Forearm Width")]
        public float forearmWidth { get { return _forearmWidth; } set { _forearmWidth = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Hand size")]

        public float handsSize { get { return _handsSize; } set { _handsSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Feets, "Size")]
        public float feetSize { get { return _feetSize; } set { _feetSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Legs, "Seperation")]
        public float legSeparation { get { return _legSeparation; } set { _legSeparation = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Muscle")]
        public float upperMuscle { get { return _upperMuscle; } set { _upperMuscle = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Legs, "Muscle")]
        public float lowerMuscle { get { return _lowerMuscle; } set { _lowerMuscle = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Weight")]
        public float upperWeight { get { return _upperWeight; } set { _upperWeight = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Legs, "Weight")]
        public float lowerWeight { get { return _lowerWeight; } set { _lowerWeight = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Legs, "Size")]
        public float legsSize { get { return _legsSize; } set { _legsSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Belly size")]
        public float belly { get { return _belly; } set { _belly = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Body, "Waist")]
        public float waist { get { return _waist; } set { _waist = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Body, "Gluteus")]
        public float gluteusSize { get { return _gluteusSize; } set { _gluteusSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Ear Size")]

        public float earsSize { get { return _earsSize; } set { _earsSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Ear Position")]
        public float earsPosition { get { return _earsPosition; } set { _earsPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Ear Rotation")]
        public float earsRotation { get { return _earsRotation; } set { _earsRotation = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Size")]
        public float noseSize { get { return _noseSize; } set { _noseSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Curve")]
        public float noseCurve { get { return _noseCurve; } set { _noseCurve = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Width")]
        public float noseWidth { get { return _noseWidth; } set { _noseWidth = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Inclination")]
        public float noseInclination { get { return _noseInclination; } set { _noseInclination = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Position")]
        public float nosePosition { get { return _nosePosition; } set { _nosePosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Pronounced")]
        public float nosePronounced { get { return _nosePronounced; } set { _nosePronounced = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Nose Flatten")]
        public float noseFlatten { get { return _noseFlatten; } set { _noseFlatten = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Chin")]
        public float chinSize { get { return _chinSize; } set { _chinSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Chin Pronounced")]
        public float chinPronounced { get { return _chinPronounced; } set { _chinPronounced = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Chin Position")]
        public float chinPosition { get { return _chinPosition; } set { _chinPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Mandible Size")]
        public float mandibleSize { get { return _mandibleSize; } set { _mandibleSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Jaw Size")]
        public float jawsSize { get { return _jawsSize; } set { _jawsSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Jaw Position")]
        public float jawsPosition { get { return _jawsPosition; } set { _jawsPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Cheek Size")]

        public float cheekSize { get { return _cheekSize; } set { _cheekSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Cheek Position")]
        public float cheekPosition { get { return _cheekPosition; } set { _cheekPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Low-Cheek Pronounced")]
        public float lowCheekPronounced { get { return _lowCheekPronounced; } set { _lowCheekPronounced = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Low-Cheek Position")]
        public float lowCheekPosition { get { return _lowCheekPosition; } set { _lowCheekPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Forehead Size")]

        public float foreheadSize { get { return _foreheadSize; } set { _foreheadSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Forehead Position")]
        public float foreheadPosition { get { return _foreheadPosition; } set { _foreheadPosition = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Lips Size")]
        public float lipsSize { get { return _lipsSize; } set { _lipsSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Mouth Size")]
        public float mouthSize { get { return _mouthSize; } set { _mouthSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Eye Rotation")]
        public float eyeRotation { get { return _eyeRotation; } set { _eyeRotation = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Head, "Eye Size")]
        public float eyeSize { get { return _eyeSize; } set { _eyeSize = value; EmitSignal(nameof(ValueChanged)); } }
        [Export]
        [JsonProperty]
        [UMACategoryAttribute(UMASlotCategory.Torso, "Breast Size")]
        public float breastSize { get { return _breastSize; } set { _breastSize = value; EmitSignal(nameof(ValueChanged)); } }
    }
}
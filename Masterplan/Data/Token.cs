using System;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Interface for map tokens.
    /// </summary>
    public interface IToken
    {
    }

    /// <summary>
    ///     A map token for a creature.
    /// </summary>
    [Serializable]
    public class CreatureToken : IToken
    {
        /// <summary>
        ///     The CombatData for this creature.
        /// </summary>
        public CombatData Data;

        /// <summary>
        ///     The ID of the encounter slot.
        /// </summary>
        public Guid SlotId = Guid.Empty;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public CreatureToken()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="slot_id">The ID of the EncounterSlot.</param>
        /// <param name="data">The CombatData for this creature.</param>
        public CreatureToken(Guid slotId, CombatData data)
        {
            SlotId = slotId;
            Data = data;
        }
    }

    /// <summary>
    ///     Types of custom token.
    /// </summary>
    public enum CustomTokenType
    {
        /// <summary>
        ///     The custom token is shown as a token.
        /// </summary>
        Token,

        /// <summary>
        ///     The custom token is shown as a translucent overlay.
        /// </summary>
        Overlay
    }

    /// <summary>
    ///     Types of overlay style.
    /// </summary>
    public enum OverlayStyle
    {
        /// <summary>
        ///     A rounded, translucent overlay.
        /// </summary>
        Rounded,

        /// <summary>
        ///     A rectangular, opaque overlay.
        /// </summary>
        Block
    }

    /// <summary>
    ///     A custom map token or overlay.
    /// </summary>
    [Serializable]
    public class CustomToken : IToken
    {
        private Color _fColour = Color.DarkBlue;

        private Guid _fCreatureId = Guid.Empty;

        private CombatData _fData = new CombatData();

        private string _fDetails = "";

        private bool _fDifficultTerrain;

        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fName = "";

        private bool _fOpaque;

        private Size _fOverlaySize = new Size(3, 3);

        private OverlayStyle _fOverlayStyle = OverlayStyle.Rounded;

        private TerrainPower _fTerrainPower;

        private CreatureSize _fTokenSize = CreatureSize.Medium;

        private CustomTokenType _fType = CustomTokenType.Token;

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the token type.
        /// </summary>
        public CustomTokenType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the token name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the token details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the size of the token.
        /// </summary>
        public CreatureSize TokenSize
        {
            get => _fTokenSize;
            set => _fTokenSize = value;
        }

        /// <summary>
        ///     Gets or sets the size of the overlay.
        /// </summary>
        public Size OverlaySize
        {
            get => _fOverlaySize;
            set => _fOverlaySize = value;
        }

        /// <summary>
        ///     Gets or sets the style of the overlay.
        /// </summary>
        public OverlayStyle OverlayStyle
        {
            get => _fOverlayStyle;
            set => _fOverlayStyle = value;
        }

        /// <summary>
        ///     Gets or sets the colour of the token.
        /// </summary>
        public Color Colour
        {
            get => _fColour;
            set => _fColour = value;
        }

        /// <summary>
        ///     Gets or sets the token / overlay image.
        /// </summary>
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Gets or sets whether the overlay represents difficult terrain.
        /// </summary>
        public bool DifficultTerrain
        {
            get => _fDifficultTerrain;
            set => _fDifficultTerrain = value;
        }

        /// <summary>
        ///     Gets or sets whether the overlay obscures line of sight.
        /// </summary>
        public bool Opaque
        {
            get => _fOpaque;
            set => _fOpaque = value;
        }

        /// <summary>
        ///     Gets or sets the CombatData for this token.
        /// </summary>
        public CombatData Data
        {
            get => _fData;
            set => _fData = value;
        }

        /// <summary>
        ///     Gets or sets the terrain power for this overlay.
        /// </summary>
        public TerrainPower TerrainPower
        {
            get => _fTerrainPower;
            set => _fTerrainPower = value;
        }

        /// <summary>
        ///     The ID of the creature or hero on which the token is centred.
        /// </summary>
        public Guid CreatureId
        {
            get => _fCreatureId;
            set => _fCreatureId = value;
        }

        /// <summary>
        ///     Creates a copy of the CustomToken.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CustomToken Copy()
        {
            var ct = new CustomToken();

            ct.Id = _fId;
            ct.Type = _fType;
            ct.Name = _fName;
            ct.Details = _fDetails;
            ct.TokenSize = _fTokenSize;
            ct.OverlaySize = _fOverlaySize;
            ct.OverlayStyle = _fOverlayStyle;
            ct.Colour = _fColour;
            ct.Image = _fImage;
            ct.DifficultTerrain = _fDifficultTerrain;
            ct.Opaque = _fOpaque;
            ct.Data = _fData.Copy();
            ct.TerrainPower = _fTerrainPower?.Copy();
            ct.CreatureId = _fCreatureId;

            return ct;
        }
    }

    /// <summary>
    ///     Types of terrain power.
    /// </summary>
    public enum TerrainPowerType
    {
        /// <summary>
        ///     The terrain power can be used more than once.
        /// </summary>
        AtWill,

        /// <summary>
        ///     The terrain power can be used once.
        /// </summary>
        SingleUse
    }

    /// <summary>
    ///     A terrain power.
    /// </summary>
    [Serializable]
    public class TerrainPower
    {
        private ActionType _fAction = ActionType.Standard;

        private string _fAttack = "";

        private string _fCheck = "";

        private string _fEffect = "";

        private string _fFailure = "";

        private string _fFlavourText = "";

        private string _fHit = "";

        private Guid _fId = Guid.NewGuid();

        private string _fMiss = "";

        private string _fName = "";

        private string _fRequirement = "";

        private string _fSuccess = "";

        private string _fTarget = "";

        private TerrainPowerType _fType = TerrainPowerType.SingleUse;

        /// <summary>
        ///     Gets or sets the power's ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the power name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the power type.
        /// </summary>
        public TerrainPowerType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the flavour text
        /// </summary>
        public string FlavourText
        {
            get => _fFlavourText;
            set => _fFlavourText = value;
        }

        /// <summary>
        ///     Gets or sets the power's required action.
        /// </summary>
        public ActionType Action
        {
            get => _fAction;
            set => _fAction = value;
        }

        /// <summary>
        ///     Gets or sets the power's requirement.
        /// </summary>
        public string Requirement
        {
            get => _fRequirement;
            set => _fRequirement = value;
        }

        /// <summary>
        ///     Gets or sets the power's check details.
        /// </summary>
        public string Check
        {
            get => _fCheck;
            set => _fCheck = value;
        }

        /// <summary>
        ///     Gets or sets the power's success details.
        /// </summary>
        public string Success
        {
            get => _fSuccess;
            set => _fSuccess = value;
        }

        /// <summary>
        ///     Gets or sets the power's failure details.
        /// </summary>
        public string Failure
        {
            get => _fFailure;
            set => _fFailure = value;
        }

        /// <summary>
        ///     Gets or sets the power's target.
        /// </summary>
        public string Target
        {
            get => _fTarget;
            set => _fTarget = value;
        }

        /// <summary>
        ///     Gets or sets the power's attack details.
        /// </summary>
        public string Attack
        {
            get => _fAttack;
            set => _fAttack = value;
        }

        /// <summary>
        ///     Gets or sets the power's hit details.
        /// </summary>
        public string Hit
        {
            get => _fHit;
            set => _fHit = value;
        }

        /// <summary>
        ///     Gets or sets the power's miss details.
        /// </summary>
        public string Miss
        {
            get => _fMiss;
            set => _fMiss = value;
        }

        /// <summary>
        ///     Gets or sets the power's effect details.
        /// </summary>
        public string Effect
        {
            get => _fEffect;
            set => _fEffect = value;
        }

        /// <summary>
        ///     Creates a copy of the terrain power.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public TerrainPower Copy()
        {
            var tp = new TerrainPower();

            tp.Id = _fId;
            tp.Name = _fName;
            tp.Type = _fType;
            tp.FlavourText = _fFlavourText;
            tp.Action = _fAction;
            tp.Requirement = _fRequirement;
            tp.Check = _fCheck;
            tp.Success = _fSuccess;
            tp.Failure = _fFailure;
            tp.Target = _fTarget;
            tp.Attack = _fAttack;
            tp.Hit = _fHit;
            tp.Miss = _fMiss;
            tp.Effect = _fEffect;

            return tp;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Celeste
{
    [Tracked]
    public class Player : Actor
    {
        #region Constants

        public static ParticleType P_DashA;
        public static ParticleType P_DashB;
        public static ParticleType P_CassetteFly;
        public static ParticleType P_Split;
        public static ParticleType P_SummitLandA;
        public static ParticleType P_SummitLandB;
        public static ParticleType P_SummitLandC;

        public const float MaxFall = 160f;
        private const float Gravity = 900f;
        private const float HalfGravThreshold = 40f;

        private const float FastMaxFall = 240f;
        private const float FastMaxAccel = 300f;

        public const float MaxRun = 90f;
        public const float RunAccel = 1000f;
        private const float RunReduce = 400f;
        private const float AirMult = .65f;

        private const float HoldingMaxRun = 70f;
        private const float HoldMinTime = .35f;

        private const float BounceAutoJumpTime = .1f;

        private const float DuckFriction = 500f;
        private const int DuckCorrectCheck = 4;
        private const float DuckCorrectSlide = 50f;

        private const float DodgeSlideSpeedMult = 1.2f;
        private const float DuckSuperJumpXMult = 1.25f;
        private const float DuckSuperJumpYMult = .5f;

        private const float JumpGraceTime = 0.1f;
        private const float JumpSpeed = -105f;
        private const float JumpHBoost = 40f;
        private const float VarJumpTime = .2f;
        private const float CeilingVarJumpGrace = .05f;
        private const int UpwardCornerCorrection = 4;
        private const float WallSpeedRetentionTime = .06f;

        private const int WallJumpCheckDist = 3;
        private const float WallJumpForceTime = .16f;
        private const float WallJumpHSpeed = MaxRun + JumpHBoost;

        public const float WallSlideStartMax = 20f;
        private const float WallSlideTime = 1.2f;

        private const float BounceVarJumpTime = .2f;
        private const float BounceSpeed = -140f;
        private const float SuperBounceVarJumpTime = .2f;
        private const float SuperBounceSpeed = -185f;

        private const float SuperJumpSpeed = JumpSpeed;
        private const float SuperJumpH = 260f;
        private const float SuperWallJumpSpeed = -160f;
        private const float SuperWallJumpVarTime = .25f;
        private const float SuperWallJumpForceTime = .2f;
        private const float SuperWallJumpH = MaxRun + JumpHBoost * 2;

        private const float DashSpeed = 240f;
        private const float EndDashSpeed = 160f;
        private const float EndDashUpMult = .75f;
        private const float DashTime = .15f;
        private const float DashCooldown = .2f;
        private const float DashRefillCooldown = .1f;
        private const int DashHJumpThruNudge = 6;
        private const int DashCornerCorrection = 4;
        private const int DashVFloorSnapDist = 3;
        private const float DashAttackTime = .3f;

        private const float BoostMoveSpeed = 80f;
        public const float BoostTime = .25f;

        private const float DuckWindMult = 0f;
        private const int WindWallDistance = 3;

        private const float ReboundSpeedX = 120f;
        private const float ReboundSpeedY = -120f;
        private const float ReboundVarJumpTime = .15f;

        private const float ReflectBoundSpeed = 220f;

        private const float DreamDashSpeed = DashSpeed;
        private const int DreamDashEndWiggle = 5;
        private const float DreamDashMinTime = .1f;

        public const float ClimbMaxStamina = 110;
        private const float ClimbUpCost = 100 / 2.2f;
        private const float ClimbStillCost = 100 / 10f;
        private const float ClimbJumpCost = 110 / 4f;
        private const int ClimbCheckDist = 2;
        private const int ClimbUpCheckDist = 2;
        private const float ClimbNoMoveTime = .1f;
        public const float ClimbTiredThreshold = 20f;
        private const float ClimbUpSpeed = -45f;
        private const float ClimbDownSpeed = 80f;
        private const float ClimbSlipSpeed = 30f;
        private const float ClimbAccel = 900f;
        private const float ClimbGrabYMult = .2f;
        private const float ClimbHopY = -120f;
        private const float ClimbHopX = 100f;
        private const float ClimbHopForceTime = .2f;
        private const float ClimbJumpBoostTime = .2f;
        private const float ClimbHopNoWindTime = .3f;

        private const float LaunchSpeed = 280f;
        private const float LaunchCancelThreshold = 220f;

        private const float LiftYCap = -130f;
        private const float LiftXCap = 250f;

        private const float JumpThruAssistSpeed = -40f;

        private const float InfiniteDashesTime = 2f;
        private const float InfiniteDashesFirstTime = .5f;
        private const float FlyPowerFlashTime = .5f;

        private const float ThrowRecoil = 80f;
        private static readonly Vector2 CarryOffsetTarget = new Vector2(0, -12);

        private const float ChaserStateMaxTime = 4f;

        public const float WalkSpeed = 64f;

        public const int StNormal = 0;
        public const int StClimb = 1;
        public const int StDash = 2;
        public const int StSwim = 3;
        public const int StBoost = 4;
        public const int StRedDash = 5;
        public const int StHitSquash = 6;
        public const int StLaunch = 7;
        public const int StPickup = 8;
        public const int StDreamDash = 9;
        public const int StSummitLaunch = 10;
        public const int StDummy = 11;
        public const int StIntroWalk = 12;
        public const int StIntroJump = 13;
        public const int StIntroRespawn = 14;
        public const int StIntroWakeUp = 15;
        public const int StBirdDashTutorial = 16;
        public const int StFrozen = 17;
        public const int StReflectionFall = 18;
        public const int StStarFly = 19;
        public const int StTempleFall = 20;
        public const int StCassetteFly = 21;
        public const int StAttract = 22;

        public const string TalkSfx = "player_talk";

        #endregion

        #region Vars

        public Vector2 Speed;
        public Facings Facing;
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        public StateMachine StateMachine;
        public Vector2 CameraAnchor;
        public bool CameraAnchorIgnoreX;
        public bool CameraAnchorIgnoreY;
        public Vector2 CameraAnchorLerp;
        public bool ForceCameraUpdate;
        public Leader Leader;
        public VertexLight Light;
        public int Dashes;
        public float Stamina = ClimbMaxStamina;
        public bool StrawberriesBlocked;
        public Vector2 PreviousPosition;
        public bool DummyAutoAnimate = true;
        public Vector2 ForceStrongWindHair;
        public Vector2? OverrideDashDirection;
        public bool FlipInReflection = false;
        public bool JustRespawned;  // True if the player hasn't moved since respawning
        public bool Dead { get; private set; }

        private Level level;
        private Collision onCollideH;
        private Collision onCollideV;
        private bool onGround;
        private bool wasOnGround;
        private int moveX;
        private bool flash;
        private bool wasDucking;

        private float idleTimer;
        private static Chooser<string> idleColdOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f).Add("idleC", 1f);
        private static Chooser<string> idleNoBackpackOptions = new Chooser<string>().Add("idleA", 1f).Add("idleB", 3f).Add("idleC", 3f);
        private static Chooser<string> idleWarmOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f);

        public int StrawberryCollectIndex = 0;
        public float StrawberryCollectResetTimer = 0f;

        private Hitbox hurtbox;
        private float jumpGraceTimer;
        public bool AutoJump;
        public float AutoJumpTimer;
        private float varJumpSpeed;
        private float varJumpTimer;
        private int forceMoveX;
        private float forceMoveXTimer;
        private int hopWaitX;   // If you climb hop onto a moving solid, snap to beside it until you get above it
        private float hopWaitXSpeed;
        private Vector2 lastAim;
        private float dashCooldownTimer;
        private float dashRefillCooldownTimer;
        public Vector2 DashDir;
        private float wallSlideTimer = WallSlideTime;
        private int wallSlideDir;
        private float climbNoMoveTimer;
        private Vector2 carryOffset;
        private Vector2 deadOffset;
        private float introEase;
        private float wallSpeedRetentionTimer; // If you hit a wall, start this timer. If coast is clear within this timer, retain h-speed
        private float wallSpeedRetained;
        private int wallBoostDir;
        private float wallBoostTimer;   // If you climb jump and then do a sideways input within this timer, switch to wall jump
        private float maxFall;
        private float dashAttackTimer;
        private List<ChaserState> chaserStates;
        private bool wasTired;
        private HashSet<Trigger> triggersInside;
        private float highestAirY;
        private bool dashStartedOnGround;
        private bool fastJump;
        private int lastClimbMove;
        private float noWindTimer;
        private float dreamDashCanEndTimer;
        private Solid climbHopSolid;
        private Vector2 climbHopSolidPosition;
        private SoundSource wallSlideSfx;
        private SoundSource swimSurfaceLoopSfx;
        private float playFootstepOnLand;
        private float minHoldTimer;
        public Booster CurrentBooster;
        private Booster lastBooster;
        private bool calledDashEvents;
        private int lastDashes;
        private Sprite sweatSprite;
        private int startHairCount;
        private bool launched;
        private float launchedTimer;
        private float dashTrailTimer;
        private List<ChaserStateSound> activeSounds = new List<ChaserStateSound>();
        private FMOD.Studio.EventInstance idleSfx;

        private readonly Hitbox normalHitbox = new Hitbox(8, 11, -4, -11);
        private readonly Hitbox duckHitbox = new Hitbox(8, 6, -4, -6);
        private readonly Hitbox normalHurtbox = new Hitbox(8, 9, -4, -11);
        private readonly Hitbox duckHurtbox = new Hitbox(8, 4, -4, -6);
        private readonly Hitbox starFlyHitbox = new Hitbox(8, 8, -4, -10);
        private readonly Hitbox starFlyHurtbox = new Hitbox(6, 6, -3, -9);

        private Vector2 normalLightOffset = new Vector2(0, -8);
        private Vector2 duckingLightOffset = new Vector2(0, -3);

        private List<Entity> temp = new List<Entity>();

        // hair
        public static readonly Color NormalHairColor = Calc.HexToColor("AC3232");
        public static readonly Color FlyPowerHairColor = Calc.HexToColor("F2EB6D");
        public static readonly Color UsedHairColor = Calc.HexToColor("44B7FF");
        public static readonly Color FlashHairColor = Color.White;
        public static readonly Color TwoDashesHairColor = Calc.HexToColor("ff6def");
        private float hairFlashTimer;
        public Color? OverrideHairColor;

        private Vector2 windDirection;
        private float windTimeout;
        private float windHairTimer;

        // level-start intro
        public enum IntroTypes { Transition, Respawn, WalkInRight, WalkInLeft, Jump, WakeUp, Fall, TempleMirrorVoid, None }
        public IntroTypes IntroType;

        private MirrorReflection reflection;

        #endregion

        #region constructor / added / removed

        public Player(Vector2 position, PlayerSpriteMode spriteMode)
            : base(new Vector2((int)position.X, (int)position.Y))
        {
            Depth = Depths.Player;
            Tag = Tags.Persistent;

            // sprite
            Sprite = new PlayerSprite(spriteMode);
            Add(Hair = new PlayerHair(Sprite));
            Add(Sprite);
            Hair.Color = NormalHairColor;
            startHairCount = Sprite.HairCount;

            // sweat sprite
            sweatSprite = GFX.SpriteBank.Create("player_sweat");
            Add(sweatSprite);

            // physics
            Collider = normalHitbox;
            hurtbox = normalHurtbox;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;

            // states
            StateMachine = new StateMachine(23);
            StateMachine.SetCallbacks(StNormal, NormalUpdate, null, NormalBegin, NormalEnd);
            StateMachine.SetCallbacks(StClimb, ClimbUpdate, null, ClimbBegin, ClimbEnd);
            StateMachine.SetCallbacks(StDash, DashUpdate, DashCoroutine, DashBegin, DashEnd);
            StateMachine.SetCallbacks(StSwim, SwimUpdate, null, SwimBegin, null);
            StateMachine.SetCallbacks(StBoost, BoostUpdate, BoostCoroutine, BoostBegin, BoostEnd);
            StateMachine.SetCallbacks(StRedDash, RedDashUpdate, RedDashCoroutine, RedDashBegin, RedDashEnd);
            StateMachine.SetCallbacks(StHitSquash, HitSquashUpdate, null, HitSquashBegin, null);
            StateMachine.SetCallbacks(StLaunch, LaunchUpdate, null, LaunchBegin, null);
            StateMachine.SetCallbacks(StPickup, null, PickupCoroutine, null, null);
            StateMachine.SetCallbacks(StDreamDash, DreamDashUpdate, null, DreamDashBegin, DreamDashEnd);
            StateMachine.SetCallbacks(StSummitLaunch, SummitLaunchUpdate, null, SummitLaunchBegin, null);
            StateMachine.SetCallbacks(StDummy, DummyUpdate, null, DummyBegin, null);
            StateMachine.SetCallbacks(StIntroWalk, null, IntroWalkCoroutine, null, null);
            StateMachine.SetCallbacks(StIntroJump, null, IntroJumpCoroutine, null, null);
            StateMachine.SetCallbacks(StIntroRespawn, null, null, IntroRespawnBegin, IntroRespawnEnd);
            StateMachine.SetCallbacks(StIntroWakeUp, null, IntroWakeUpCoroutine, null, null);
            StateMachine.SetCallbacks(StTempleFall, TempleFallUpdate, TempleFallCoroutine);
            StateMachine.SetCallbacks(StReflectionFall, ReflectionFallUpdate, ReflectionFallCoroutine, ReflectionFallBegin, ReflectionFallEnd);
            StateMachine.SetCallbacks(StBirdDashTutorial, BirdDashTutorialUpdate, BirdDashTutorialCoroutine, BirdDashTutorialBegin, null);
            StateMachine.SetCallbacks(StFrozen, FrozenUpdate, null, null, null);
            StateMachine.SetCallbacks(StStarFly, StarFlyUpdate, StarFlyCoroutine, StarFlyBegin, StarFlyEnd);
            StateMachine.SetCallbacks(StCassetteFly, CassetteFlyUpdate, CassetteFlyCoroutine, CassetteFlyBegin, CassetteFlyEnd);
            StateMachine.SetCallbacks(StAttract, AttractUpdate, null, AttractBegin, AttractEnd);
            Add(StateMachine);

            // other stuff
            Add(Leader = new Leader(new Vector2(0, -8)));
            lastAim = Vector2.UnitX;
            Facing = Facings.Right;
            chaserStates = new List<ChaserState>();
            triggersInside = new HashSet<Trigger>();
            Add(Light = new VertexLight(normalLightOffset, Color.White, 1f, 32, 64));
            Add(new WaterInteraction(() => { return StateMachine.State == StDash || StateMachine.State == StReflectionFall; }));

            //Wind
            Add(new WindMover(WindMove));

            Add(wallSlideSfx = new SoundSource());
            Add(swimSurfaceLoopSfx = new SoundSource());
            
            Sprite.OnFrameChange = (anim) =>
            {
                if (Scene != null && !Dead)
                {
                    // footsteps
                    var frame = Sprite.CurrentAnimationFrame;
                    if ((anim.Equals(PlayerSprite.RunCarry) && (frame == 0 || frame == 6)) ||
                        (anim.Equals(PlayerSprite.RunFast) && (frame == 0 || frame == 6)) ||
                        (anim.Equals(PlayerSprite.RunSlow) && (frame == 0 || frame == 6)) ||
                        (anim.Equals(PlayerSprite.Walk) && (frame == 0 || frame == 6)) ||
                        (anim.Equals(PlayerSprite.RunStumble) && frame == 6) ||
                        (anim.Equals(PlayerSprite.Flip) && frame == 4) ||
                        (anim.Equals(PlayerSprite.RunWind) && (frame == 0 || frame == 6)) ||
                        (anim.Equals("idleC") && Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack && (frame == 3 || frame == 6 || frame == 8 || frame == 11)) ||
                        (anim.Equals("carryTheoWalk") && (frame == 0 || frame == 6)))
                    {
                        var landed = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                        if (landed != null)
                            Play(Sfxs.char_mad_footstep, SurfaceIndex.Param, landed.GetStepSoundIndex(this));
                    }
                    // climbing (holds)
                    else if ((anim.Equals(PlayerSprite.ClimbUp) && (frame == 5)) ||
                        (anim.Equals(PlayerSprite.ClimbDown) && (frame == 5)))
                    {
                        var holding = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.UnitX * (int)Facing, temp));
                        if (holding != null)
                            Play(Sfxs.char_mad_handhold, SurfaceIndex.Param, holding.GetWallSoundIndex(this, (int)Facing));
                    }
                    else if (anim.Equals("wakeUp") && frame == 19)
                        Play(Sfxs.char_mad_campfire_stand);
                    else if (anim.Equals("sitDown") && frame == 12)
                        Play(Sfxs.char_mad_summit_sit);
                    else if (anim.Equals("push") && (frame == 8 || frame == 15))
                        Dust.BurstFG(Position + new Vector2(-(int)Facing * 5, -1), new Vector2(-(int)Facing, -0.5f).Angle(), 1, 0);
                }
            };

            Sprite.OnLastFrame = (anim) =>
            {
                if (Scene != null && !Dead && Sprite.CurrentAnimationID == "idle" && !level.InCutscene && idleTimer > 3f)
                {
                    if (Calc.Random.Chance(0.2f))
                    {
                        var next = "";
                        if (Sprite.Mode == PlayerSpriteMode.Madeline)
                            next = (level.CoreMode == Session.CoreModes.Hot ? idleWarmOptions : idleColdOptions).Choose();
                        else
                            next = idleNoBackpackOptions.Choose();

                        if (!string.IsNullOrEmpty(next))
                        {
                            Sprite.Play(next);

                            if (Sprite.Mode == PlayerSpriteMode.Madeline)
                            {
                                if (next == "idleB")
                                    idleSfx = Play(Sfxs.char_mad_idle_scratch);
                                else if (next == "idleC")
                                    idleSfx = Play(Sfxs.char_mad_idle_sneeze);
                            }
                            else if (next == "idleA")
                                idleSfx = Play(Sfxs.char_mad_idle_crackknuckles);
                        }
                    }
                }
            };

            // cancel special idle sounds if the anim changed
            Sprite.OnChange = (last, next) =>
            {
                if ((last == "idleB" || last == "idleC") && next != null && !next.StartsWith("idle") && idleSfx != null)
                    Audio.Stop(idleSfx);
            };

            Add(reflection = new MirrorReflection());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();

            lastDashes = Dashes = MaxDashes;

            if (X > level.Bounds.Center.X && IntroType != IntroTypes.None)
                Facing = Facings.Left;

            switch (IntroType)
            {
                case IntroTypes.Respawn:
                    StateMachine.State = StIntroRespawn;
                    JustRespawned = true;
                    break;

                case IntroTypes.WalkInRight:
                    IntroWalkDirection = Facings.Right;
                    StateMachine.State = StIntroWalk;
                    break;

                case IntroTypes.WalkInLeft:
                    IntroWalkDirection = Facings.Left;
                    StateMachine.State = StIntroWalk;
                    break;

                case IntroTypes.Jump:
                    StateMachine.State = StIntroJump;
                    break;

                case IntroTypes.WakeUp:
                    Sprite.Play("asleep");
                    Facing = Facings.Right;
                    StateMachine.State = StIntroWakeUp;
                    break;

                case IntroTypes.None:
                    StateMachine.State = StNormal;
                    break;

                case IntroTypes.Fall:
                    StateMachine.State = StReflectionFall;
                    break;

                case IntroTypes.TempleMirrorVoid:
                    StartTempleMirrorVoidSleep();
                    break;
            }
            IntroType = IntroTypes.Transition;

            StartHair();
            PreviousPosition = Position;
        }

        public void StartTempleMirrorVoidSleep()
        {
            Sprite.Play("asleep");
            Facing = Facings.Right;
            StateMachine.State = StDummy;
            StateMachine.Locked = true;
            DummyAutoAnimate = false;
            DummyGravity = false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            level = null;
            Audio.Stop(conveyorLoopSfx);

            foreach (var trigger in triggersInside)
            {
                trigger.Triggered = false;
                trigger.OnLeave(this);
            }
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(conveyorLoopSfx);
        }

        #endregion

        #region Rendering

        public override void Render()
        {
            var was = Sprite.RenderPosition;
            Sprite.RenderPosition = Sprite.RenderPosition.Floor();

            if (StateMachine.State == StIntroRespawn)
            {
                DeathEffect.Draw(Center + deadOffset, Hair.Color, introEase);
            }
            else
            {
                if (StateMachine.State != StStarFly)
                {
                    if (IsTired && flash)
                        Sprite.Color = Color.Red;
                    else
                        Sprite.Color = Color.White;
                }

                // set up scale
                if (reflection.IsRendering && FlipInReflection)
                {
                    Facing = (Facings)(-(int)Facing);
                    Hair.Facing = Facing;
                }
                Sprite.Scale.X *= (int)Facing;

                // sweat scale
                if (sweatSprite.LastAnimationID == "idle")
                    sweatSprite.Scale = Sprite.Scale;
                else
                {
                    sweatSprite.Scale.Y = Sprite.Scale.Y;
                    sweatSprite.Scale.X = Math.Abs(Sprite.Scale.X) * Math.Sign(sweatSprite.Scale.X);
                }

                // draw
                base.Render();

                // star fly transform
                if (Sprite.CurrentAnimationID == PlayerSprite.StartStarFly)
                {
                    var p = (Sprite.CurrentAnimationFrame / (float)Sprite.CurrentAnimationTotalFrames);
                    var white = GFX.Game.GetAtlasSubtexturesAt("characters/player/startStarFlyWhite", Sprite.CurrentAnimationFrame);
                    white.Draw(Sprite.RenderPosition, Sprite.Origin, starFlyColor * p, Sprite.Scale, Sprite.Rotation, 0);
                }

                // revert scale
                Sprite.Scale.X *= (int)Facing;
                if (reflection.IsRendering && FlipInReflection)
                {
                    Facing = (Facings)(-(int)Facing);
                    Hair.Facing = Facing;
                }
            }

            Sprite.RenderPosition = was;
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            {
                Collider was = Collider;
                Collider = hurtbox;
                Draw.HollowRect(Collider, Color.Lime);
                Collider = was;
            }
        }

        #endregion

        #region Updating

        public override void Update()
        {
            //Infinite Stamina variant
            if (SaveData.Instance.AssistMode && SaveData.Instance.Assists.InfiniteStamina)
                Stamina = ClimbMaxStamina;
            
            PreviousPosition = Position;

            //Vars       
            {
                // strawb reset timer
                StrawberryCollectResetTimer -= Engine.DeltaTime;
                if (StrawberryCollectResetTimer <= 0)
                    StrawberryCollectIndex = 0;

                // idle timer
                idleTimer += Engine.DeltaTime;
                if (level != null && level.InCutscene)
                    idleTimer = -5;
                else if (Speed.X != 0 || Speed.Y != 0)
                    idleTimer = 0;

                //Underwater music
                if (!Dead)
                    Audio.MusicUnderwater = UnderwaterMusicCheck();

                //Just respawned
                if (JustRespawned && Speed != Vector2.Zero)
                    JustRespawned = false;

                //Get ground
                if (StateMachine.State == StDreamDash)
                    onGround = OnSafeGround = false;
                else if (Speed.Y >= 0)
                {
                    Platform first = CollideFirst<Solid>(Position + Vector2.UnitY);
                    if (first == null)
                        first = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);

                    if (first != null)
                    {
                        onGround = true;
                        OnSafeGround = first.Safe;
                    }
                    else
                        onGround = OnSafeGround = false;
                }
                else
                    onGround = OnSafeGround = false;

                if (StateMachine.State == StSwim)
                    OnSafeGround = true;

                //Safe Ground Blocked?
                if (OnSafeGround)
                {
                    foreach (SafeGroundBlocker blocker in Scene.Tracker.GetComponents<SafeGroundBlocker>())
                    {
                        if (blocker.Check(this))
                        {
                            OnSafeGround = false;
                            break;
                        }
                    }
                }

                playFootstepOnLand -= Engine.DeltaTime;

                //Highest Air Y
                if (onGround)
                    highestAirY = Y;
                else
                    highestAirY = Math.Min(Y, highestAirY);

                //Flashing
                if (Scene.OnInterval(.05f))
                    flash = !flash;

                //Wall Slide
                if (wallSlideDir != 0)
                {
                    wallSlideTimer = Math.Max(wallSlideTimer - Engine.DeltaTime, 0);
                    wallSlideDir = 0;
                }

                //Wall Boost
                if (wallBoostTimer > 0)
                {
                    wallBoostTimer -= Engine.DeltaTime;
                    if (moveX == wallBoostDir)
                    {
                        Speed.X = WallJumpHSpeed * moveX;
                        Stamina += ClimbJumpCost;
                        wallBoostTimer = 0;
                        sweatSprite.Play("idle");                       
                    }
                }

                //After Dash
                if (onGround && StateMachine.State != StClimb)
                {
                    AutoJump = false;
                    Stamina = ClimbMaxStamina;
                    wallSlideTimer = WallSlideTime;
                }

                //Dash Attack
                if (dashAttackTimer > 0)
                    dashAttackTimer -= Engine.DeltaTime;

                //Jump Grace
                if (onGround)
                {
                    dreamJump = false;
                    jumpGraceTimer = JumpGraceTime;
                }
                else if (jumpGraceTimer > 0)
                    jumpGraceTimer -= Engine.DeltaTime;

                //Dashes
                {
                    if (dashCooldownTimer > 0)
                        dashCooldownTimer -= Engine.DeltaTime;

                    if (dashRefillCooldownTimer > 0)
                        dashRefillCooldownTimer -= Engine.DeltaTime;
                    else if (SaveData.Instance.AssistMode && SaveData.Instance.Assists.DashMode == Assists.DashModes.Infinite && !level.InCutscene)
                        RefillDash();
                    else if (!Inventory.NoRefills)
                    {
                        if (StateMachine.State == StSwim)
                            RefillDash();
                        else if (onGround)
                            if (CollideCheck<Solid, NegaBlock>(Position + Vector2.UnitY) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY))
                                if (!CollideCheck<Spikes>(Position) || (SaveData.Instance.AssistMode && SaveData.Instance.Assists.Invincible))
                                    RefillDash();
                    }
                }

                //Var Jump
                if (varJumpTimer > 0)
                    varJumpTimer -= Engine.DeltaTime;

                //Auto Jump
                if (AutoJumpTimer > 0)
                {
                    if (AutoJump)
                    {
                        AutoJumpTimer -= Engine.DeltaTime;
                        if (AutoJumpTimer <= 0)
                            AutoJump = false;
                    }
                    else
                        AutoJumpTimer = 0;
                }

                //Force Move X
                if (forceMoveXTimer > 0)
                {
                    forceMoveXTimer -= Engine.DeltaTime;
                    moveX = forceMoveX;
                }
                else
                {
                    moveX = Input.MoveX.Value;
                    climbHopSolid = null;
                }

                //Climb Hop Solid Movement
                if (climbHopSolid != null && !climbHopSolid.Collidable)
                    climbHopSolid = null;
                else if (climbHopSolid != null && climbHopSolid.Position != climbHopSolidPosition)
                {
                    var move = climbHopSolid.Position - climbHopSolidPosition;
                    climbHopSolidPosition = climbHopSolid.Position;
                    MoveHExact((int)move.X);
                    MoveVExact((int)move.Y);
                }

                //Wind
                if (noWindTimer > 0)
                    noWindTimer -= Engine.DeltaTime;

                //Facing
                if (moveX != 0 && InControl
                    && StateMachine.State != StClimb && StateMachine.State != StPickup && StateMachine.State != StRedDash && StateMachine.State != StHitSquash)
                {
                    var to = (Facings)moveX;
                    if (to != Facing && Ducking)
                        Sprite.Scale = new Vector2(0.8f, 1.2f);
                    Facing = to;
                }

                //Aiming
                lastAim = Input.GetAimVector(Facing);

                //Wall Speed Retention
                if (wallSpeedRetentionTimer > 0)
                {
                    if (Math.Sign(Speed.X) == -Math.Sign(wallSpeedRetained))
                        wallSpeedRetentionTimer = 0;
                    else if (!CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(wallSpeedRetained)))
                    {
                        Speed.X = wallSpeedRetained;
                        wallSpeedRetentionTimer = 0;
                    }
                    else
                        wallSpeedRetentionTimer -= Engine.DeltaTime;
                }

                //Hop Wait X
                if (hopWaitX != 0)
                {
                    if (Math.Sign(Speed.X) == -hopWaitX || Speed.Y > 0)
                        hopWaitX = 0;
                    else if (!CollideCheck<Solid>(Position + Vector2.UnitX * hopWaitX))
                    {
                        Speed.X = hopWaitXSpeed;
                        hopWaitX = 0;
                    }
                }

                // Wind Timeout
                if (windTimeout > 0)
                    windTimeout -= Engine.DeltaTime;

                // Hair
                {
                    var windDir = windDirection;
                    if (ForceStrongWindHair.Length() > 0)
                        windDir = ForceStrongWindHair;

                    if (windTimeout > 0 && windDir.X != 0)
                    {
                        windHairTimer += Engine.DeltaTime * 8f;

                        Hair.StepPerSegment = new Vector2(windDir.X * 5f, (float)Math.Sin(windHairTimer));
                        Hair.StepInFacingPerSegment = 0f;
                        Hair.StepApproach = 128f;
                        Hair.StepYSinePerSegment = 0;
                    }
                    else if (Dashes > 1)
                    {
                        Hair.StepPerSegment = new Vector2((float)Math.Sin(Scene.TimeActive * 2) * 0.7f - (int)Facing * 3, (float)Math.Sin(Scene.TimeActive * 1f));
                        Hair.StepInFacingPerSegment = 0f;
                        Hair.StepApproach = 90f;
                        Hair.StepYSinePerSegment = 1f;

                        Hair.StepPerSegment.Y += windDir.Y * 2f;
                    }
                    else
                    {
                        Hair.StepPerSegment = new Vector2(0, 2f);
                        Hair.StepInFacingPerSegment = 0.5f;
                        Hair.StepApproach = 64f;
                        Hair.StepYSinePerSegment = 0;

                        Hair.StepPerSegment.Y += windDir.Y * 0.5f;
                    }
                }

                if (StateMachine.State == StRedDash)
                    Sprite.HairCount = 1;
                else if (StateMachine.State != StStarFly)
                    Sprite.HairCount = (Dashes > 1 ? 5 : startHairCount);

                //Min Hold Time
                if (minHoldTimer > 0)
                    minHoldTimer -= Engine.DeltaTime;

                //Launch Particles
                if (launched)
                {
                    var sq = Speed.LengthSquared();
                    if (sq < LaunchedMinSpeedSq)
                        launched = false;
                    else
                    {
                        var was = launchedTimer;
                        launchedTimer += Engine.DeltaTime;

                        if (launchedTimer >= .5f)
                        {
                            launched = false;
                            launchedTimer = 0;
                        }
                        else if (Calc.OnInterval(launchedTimer, was, .15f))
                            level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, Speed.Angle(), Color.White));
                    }
                }
                else
                    launchedTimer = 0;
            }

            if (IsTired)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                if (!wasTired)
                {
                    wasTired = true;
                }
            }
            else
                wasTired = false;

            base.Update();
            
            //Light Offset
            if (Ducking)
                Light.Position = duckingLightOffset;
            else
                Light.Position = normalLightOffset;

            //Jump Thru Assist
            if (!onGround && Speed.Y <= 0 && (StateMachine.State != StClimb || lastClimbMove == -1) && CollideCheck<JumpThru>() && !JumpThruBoostBlockedCheck())
                MoveV(JumpThruAssistSpeed * Engine.DeltaTime);

            //Dash Floor Snapping
            if (!onGround && DashAttacking && DashDir.Y == 0)
            {
                if (CollideCheck<Solid>(Position + Vector2.UnitY * DashVFloorSnapDist) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY * DashVFloorSnapDist))
                    MoveVExact(DashVFloorSnapDist);
            }

            //Falling unducking
            if (Speed.Y > 0 && CanUnDuck && Collider != starFlyHitbox && !onGround)
                Ducking = false;

            //Physics
            if (StateMachine.State != StDreamDash && StateMachine.State != StAttract)
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            if (StateMachine.State != StDreamDash && StateMachine.State != StAttract)
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

            //Swimming
            if (StateMachine.State == StSwim)
            {
                //Stay at water surface
                if (Speed.Y < 0 && Speed.Y >= SwimMaxRise)
                {
                    while (!SwimCheck())
                    {
                        Speed.Y = 0;
                        if (MoveVExact(1))
                            break;
                    }
                }
            }
            else if (StateMachine.State == StNormal && SwimCheck())
                StateMachine.State = StSwim;
            else if (StateMachine.State == StClimb && SwimCheck())
            {
                var water = CollideFirst<Water>(Position);
                if (water != null && Center.Y < water.Center.Y)
                {
                    while (SwimCheck())
                        if (MoveVExact(-1))
                            break;
                    if (SwimCheck())
                        StateMachine.State = StSwim;
                }
                else
                    StateMachine.State = StSwim;
            }

            // wall slide SFX
            {
                var isSliding = Sprite.CurrentAnimationID != null && Sprite.CurrentAnimationID.Equals(PlayerSprite.WallSlide) && Speed.Y > 0;
                if (isSliding)
                {
                    if (!wallSlideSfx.Playing)
                        Loop(wallSlideSfx, Sfxs.char_mad_wallslide);

                    var platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.UnitX * (int)Facing, temp));
                    if (platform != null)
                        wallSlideSfx.Param(SurfaceIndex.Param, platform.GetWallSoundIndex(this, (int)Facing));
                }
                else
                    Stop(wallSlideSfx);
            }

            // update sprite
            UpdateSprite();

            //Carry held item
            UpdateCarry();

            //Triggers
            if (StateMachine.State != StReflectionFall)
            {
                foreach (Trigger trigger in Scene.Tracker.GetEntities<Trigger>())
                {
                    if (CollideCheck(trigger))
                    {
                        if (!trigger.Triggered)
                        {
                            trigger.Triggered = true;
                            triggersInside.Add(trigger);
                            trigger.OnEnter(this);
                        }
                        trigger.OnStay(this);
                    }
                    else if (trigger.Triggered)
                    {
                        triggersInside.Remove(trigger);
                        trigger.Triggered = false;
                        trigger.OnLeave(this);
                    }
                }
            }
            
            //Strawberry Block
            StrawberriesBlocked = CollideCheck<BlockField>();

            // Camera (lerp by distance using delta-time)
            if (InControl || ForceCameraUpdate)
            {
                if (StateMachine.State == StReflectionFall)
                {
                    level.Camera.Position = CameraTarget;
                }
                else
                {
                    var from = level.Camera.Position;
                    var target = CameraTarget;
                    var multiplier = StateMachine.State == StTempleFall ? 8 : 1f;
                    
                    level.Camera.Position = from + (target - from) * (1f - (float)Math.Pow(0.01f / multiplier, Engine.DeltaTime));
                }
            }

            //Player Colliders
            if (!Dead && StateMachine.State != StCassetteFly)
            {
                Collider was = Collider;
                Collider = hurtbox;

                foreach (PlayerCollider pc in Scene.Tracker.GetComponents<PlayerCollider>())
                {
                    if (pc.Check(this) && Dead)
                    {
                        Collider = was;
                        return;
                    }
                }

                // If the current collider is not the hurtbox we set it to, that means a collision callback changed it. Keep the new one!
                bool keepNew = (Collider != hurtbox);

                if (!keepNew)    
                    Collider = was;
            }
            
            //Bounds
            if (InControl && !Dead && StateMachine.State != StDreamDash)
                level.EnforceBounds(this);

            UpdateChaserStates();
            UpdateHair(true);

            //Sounds on ducking state change
            if (wasDucking != Ducking)
            {
                wasDucking = Ducking;
                if (wasDucking)
                    Play(Sfxs.char_mad_duck);
                else if (onGround)
                    Play(Sfxs.char_mad_stand);
            }

            // shallow swim sfx
            if (Speed.X != 0 && ((StateMachine.State == StSwim && !SwimUnderwaterCheck()) || (StateMachine.State == StNormal && CollideCheck<Water>(Position))))
            {
                if (!swimSurfaceLoopSfx.Playing)
                    swimSurfaceLoopSfx.Play(Sfxs.char_mad_water_move_shallow);
            }
            else
                swimSurfaceLoopSfx.Stop();

            wasOnGround = onGround;
        }

        private void CreateTrail()
        {
            TrailManager.Add(this, wasDashB ? NormalHairColor : UsedHairColor);
        }

        public void CleanUpTriggers()
        {
            if (triggersInside.Count > 0)
            {
                foreach (var trigger in triggersInside)
                {
                    trigger.OnLeave(this);
                    trigger.Triggered = false;
                }

                triggersInside.Clear();
            }
        }

        private void UpdateChaserStates()
        {
            while (chaserStates.Count > 0 && Scene.TimeActive - chaserStates[0].TimeStamp > ChaserStateMaxTime)
                chaserStates.RemoveAt(0);
            chaserStates.Add(new ChaserState(this));
            activeSounds.Clear();
        }

        #endregion

        #region Hair & Sprite

        private void StartHair()
        {
            Hair.Facing = Facing;
            Hair.Start();
            UpdateHair(true);
        }

        public void UpdateHair(bool applyGravity)
        {
            // color
            if (StateMachine.State == StStarFly)
            {
                Hair.Color = Sprite.Color;
                applyGravity = false;
            }
            else if (Dashes == 0 && Dashes < MaxDashes)
                Hair.Color = Color.Lerp(Hair.Color, UsedHairColor, 6f * Engine.DeltaTime);
            else
            {
                Color color;
                if (lastDashes != Dashes)
                {
                    color = FlashHairColor;
                    hairFlashTimer = .12f;
                }
                else if (hairFlashTimer > 0)
                {
                    color = FlashHairColor;
                    hairFlashTimer -= Engine.DeltaTime;
                }
                else if (Dashes == 2)
                    color = TwoDashesHairColor;
                else
                    color = NormalHairColor;

                Hair.Color = color;
            }

            if (OverrideHairColor != null)
                Hair.Color = OverrideHairColor.Value;

            Hair.Facing = Facing;
            Hair.SimulateMotion = applyGravity;
            lastDashes = Dashes;
        }

        private void UpdateSprite()
        {
            //Tween
            Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);

            //Animation
            if (InControl && Sprite.CurrentAnimationID != PlayerSprite.Throw && StateMachine.State != StTempleFall && 
                StateMachine.State != StReflectionFall && StateMachine.State != StStarFly && StateMachine.State != StCassetteFly)
            {
                if (StateMachine.State == StAttract)
                {
                    Sprite.Play(PlayerSprite.FallFast);
                }
                else if (StateMachine.State == StSummitLaunch)
                {
                    Sprite.Play(PlayerSprite.Launch);
                }
                // picking up
                else if (StateMachine.State == StPickup)
                {
                    Sprite.Play(PlayerSprite.PickUp);
                }
                // swiming
                else if (StateMachine.State == StSwim)
                {
                    if (Input.MoveY.Value > 0)
                        Sprite.Play(PlayerSprite.SwimDown);
                    else if (Input.MoveY.Value < 0)
                        Sprite.Play(PlayerSprite.SwimUp);
                    else
                        Sprite.Play(PlayerSprite.SwimIdle);
                }
                // dream dashing
                else if (StateMachine.State == StDreamDash)
                {
                    if (Sprite.CurrentAnimationID != PlayerSprite.DreamDashIn && Sprite.CurrentAnimationID != PlayerSprite.DreamDashLoop)
                        Sprite.Play(PlayerSprite.DreamDashIn);
                }
                else if (Sprite.DreamDashing && Sprite.LastAnimationID != PlayerSprite.DreamDashOut)
                {
                    Sprite.Play(PlayerSprite.DreamDashOut);
                }
                else if (Sprite.CurrentAnimationID != PlayerSprite.DreamDashOut)
                {
                    // during dash
                    if (DashAttacking)
                    {
                        if (onGround && DashDir.Y == 0 && !Ducking && Speed.X != 0 && moveX == -Math.Sign(Speed.X))
                        {
                            if (Scene.OnInterval(.02f))
                                Dust.Burst(Position, Calc.Up, 1);
                            Sprite.Play(PlayerSprite.Skid);
                        }
                        else
                            Sprite.Play(PlayerSprite.Dash);
                    }
                    // climbing
                    else if (StateMachine.State == StClimb)
                    {
                        if (lastClimbMove < 0)
                            Sprite.Play(PlayerSprite.ClimbUp);
                        else if (lastClimbMove > 0)
                            Sprite.Play(PlayerSprite.WallSlide);
                        else if (!CollideCheck<Solid>(Position + new Vector2((int)Facing, 6)))
                            Sprite.Play(PlayerSprite.Dangling);
                        else if (Input.MoveX == -(int)Facing)
                        {
                            if (Sprite.CurrentAnimationID != PlayerSprite.ClimbLookBack)
                                Sprite.Play(PlayerSprite.ClimbLookBackStart);
                        }
                        else
                            Sprite.Play(PlayerSprite.WallSlide);
                    }
                    // ducking
                    else if (Ducking && StateMachine.State == StNormal)
                    {
                        Sprite.Play(PlayerSprite.Duck);
                    }
                    else if (onGround)
                    {
                        fastJump = false;
                        if (Holding == null && moveX != 0 && CollideCheck<Solid>(Position + Vector2.UnitX * moveX))
                        {
                            Sprite.Play("push");
                        }
                        else if (Math.Abs(Speed.X) <= RunAccel / 40f && moveX == 0)
                        {
                            if (Holding != null)
                            {
                                Sprite.Play(PlayerSprite.IdleCarry);
                            }
                            else if (!Scene.CollideCheck<Solid>(Position + new Vector2((int)Facing * 1, 2)) && !Scene.CollideCheck<Solid>(Position + new Vector2((int)Facing * 4, 2)) && !CollideCheck<JumpThru>(Position + new Vector2((int)Facing * 4, 2)))
                            {
                                Sprite.Play(PlayerSprite.FrontEdge);
                            }
                            else if (!Scene.CollideCheck<Solid>(Position + new Vector2(-(int)Facing * 1, 2)) && !Scene.CollideCheck<Solid>(Position + new Vector2(-(int)Facing * 4, 2)) && !CollideCheck<JumpThru>(Position + new Vector2(-(int)Facing * 4, 2)))
                            {
                                Sprite.Play("edgeBack");
                            }
                            else if (Input.MoveY.Value == -1)
                            {
                                if (Sprite.LastAnimationID != PlayerSprite.LookUp)
                                    Sprite.Play(PlayerSprite.LookUp);
                            }
                            else
                            {
                                if (Sprite.CurrentAnimationID != null && !Sprite.CurrentAnimationID.Contains("idle"))
                                    Sprite.Play(PlayerSprite.Idle);
                            }
                        }
                        else if (Holding != null)
                        {
                            Sprite.Play(PlayerSprite.RunCarry);
                        }
                        else if (Math.Sign(Speed.X) == -moveX && moveX != 0)
                        {
                            if (Math.Abs(Speed.X) > MaxRun)
                                Sprite.Play(PlayerSprite.Skid);
                            else if (Sprite.CurrentAnimationID != PlayerSprite.Skid)
                                Sprite.Play(PlayerSprite.Flip);
                        }
                        else if (windDirection.X != 0 && windTimeout > 0f && (int)Facing == -Math.Sign(windDirection.X))
                        {
                            Sprite.Play(PlayerSprite.RunWind);
                        }
                        else if (!Sprite.Running)
                        {
                            if (Math.Abs(Speed.X) < MaxRun * .5f)
                                Sprite.Play(PlayerSprite.RunSlow);
                            else
                                Sprite.Play(PlayerSprite.RunFast);
                        }
                    }
                    // wall sliding
                    else if (wallSlideDir != 0 && Holding == null)
                    {
                        Sprite.Play(PlayerSprite.WallSlide);
                    }
                    // jumping up
                    else if (Speed.Y < 0)
                    {
                        if (Holding != null)
                        {
                            Sprite.Play(PlayerSprite.JumpCarry);
                        }
                        else if (fastJump || Math.Abs(Speed.X) > MaxRun)
                        {
                            fastJump = true;
                            Sprite.Play(PlayerSprite.JumpFast);
                        }
                        else
                            Sprite.Play(PlayerSprite.JumpSlow);
                    }
                    // falling down
                    else
                    {
                        if (Holding != null)
                        {
                            Sprite.Play(PlayerSprite.FallCarry);
                        }
                        else if (fastJump || Speed.Y >= MaxFall || level.InSpace)
                        {
                            fastJump = true;
                            if (Sprite.LastAnimationID != PlayerSprite.FallFast)
                                Sprite.Play(PlayerSprite.FallFast);
                        }
                        else
                            Sprite.Play(PlayerSprite.FallSlow);
                    }
                }
            }

            if (StateMachine.State != Player.StDummy)
            {
                if (level.InSpace)
                    Sprite.Rate = .5f;
                else
                    Sprite.Rate = 1f;
            }
        }

        public void CreateSplitParticles()
        {
            level.Particles.Emit(P_Split, 16, Center, Vector2.One * 6);
        }

        #endregion

        #region Getters

        public Vector2 CameraTarget
        {
            get
            {
                Vector2 at = new Vector2();
                Vector2 target = new Vector2(X - Celeste.GameWidth / 2, Y - Celeste.GameHeight / 2);
                if (StateMachine.State != StReflectionFall)
                    target += new Vector2(level.CameraOffset.X, level.CameraOffset.Y);

                if (StateMachine.State == StStarFly)
                {
                    target.X += .2f * Speed.X;
                    target.Y += .2f * Speed.Y;
                }
                else if (StateMachine.State == StRedDash)
                {
                    target.X += 48 * Math.Sign(Speed.X);
                    target.Y += 48 * Math.Sign(Speed.Y);
                }
                else if (StateMachine.State == StSummitLaunch)
                {
                    target.Y -= 64;
                }
                else if (StateMachine.State == StReflectionFall)
                {
                    target.Y += 32;
                }

                if (CameraAnchorLerp.Length() > 0)
                {
                    if (CameraAnchorIgnoreX && !CameraAnchorIgnoreY)
                        target.Y = MathHelper.Lerp(target.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
                    else if (!CameraAnchorIgnoreX && CameraAnchorIgnoreY)
                        target.X = MathHelper.Lerp(target.X, CameraAnchor.X, CameraAnchorLerp.X);
                    else if (CameraAnchorLerp.X == CameraAnchorLerp.Y)
                        target = Vector2.Lerp(target, CameraAnchor, CameraAnchorLerp.X);
                    else
                    {
                        target.X = MathHelper.Lerp(target.X, CameraAnchor.X, CameraAnchorLerp.X);
                        target.Y = MathHelper.Lerp(target.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
                    }
                }

                at.X = MathHelper.Clamp(target.X, level.Bounds.Left, level.Bounds.Right - Celeste.GameWidth);
                at.Y = MathHelper.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - Celeste.GameHeight);

                if (level.CameraLockMode != Level.CameraLockModes.None)
                {
                    var locker = Scene.Tracker.GetComponent<CameraLocker>();

                    //X Snapping
                    if (level.CameraLockMode != Level.CameraLockModes.BoostSequence)
                    {
                        at.X = Math.Max(at.X, level.Camera.X);
                        if (locker != null)
                            at.X = Math.Min(at.X, Math.Max(level.Bounds.Left, locker.Entity.X - locker.MaxXOffset));
                    }

                    //Y Snapping
                    if (level.CameraLockMode == Level.CameraLockModes.FinalBoss)
                    {
                        at.Y = Math.Max(at.Y, level.Camera.Y);
                        if (locker != null)
                            at.Y = Math.Min(at.Y, Math.Max(level.Bounds.Top, locker.Entity.Y - locker.MaxYOffset));
                    }
                    else if (level.CameraLockMode == Level.CameraLockModes.BoostSequence)
                    {
                        level.CameraUpwardMaxY = Math.Min(level.Camera.Y + CameraLocker.UpwardMaxYOffset, level.CameraUpwardMaxY);
                        at.Y = Math.Min(at.Y, level.CameraUpwardMaxY);
                        if (locker != null)
                            at.Y = Math.Max(at.Y, Math.Min(level.Bounds.Bottom - 180, locker.Entity.Y - locker.MaxYOffset));
                    }
                }

                // snap above killboxes
                var killboxes = Scene.Tracker.GetEntities<Killbox>();
                foreach (var box in killboxes)
                {
                    if (!box.Collidable)
                        continue;

                    if (Top < box.Bottom && Right > box.Left && Left < box.Right)
                        at.Y = Math.Min(at.Y, box.Top - 180);
                }

                return at;
            }
        }
        
        public bool GetChasePosition(float sceneTime, float timeAgo, out ChaserState chaseState)
        {
            if (!Dead)
            {
                bool tooLongAgoFound = false;
                foreach (var state in chaserStates)
                {
                    float time = sceneTime - state.TimeStamp;
                    if (time <= timeAgo)
                    {
                        if (tooLongAgoFound || timeAgo - time < .02f)
                        {
                            chaseState = state;
                            return true;
                        }
                        else
                        {
                            chaseState = new ChaserState();
                            return false;
                        }
                    }
                    else
                        tooLongAgoFound = true;
                }
            }

            chaseState = new ChaserState();
            return false;
        }

        public bool CanRetry
        {
            get
            {
                switch (StateMachine.State)
                {
                    default:
                        return true;
                        
                    case StIntroJump:
                    case StIntroWalk:
                    case StIntroWakeUp:
                    case StIntroRespawn:
                    case StReflectionFall:
                        return false;
                }
            }
        }

        public bool TimePaused
        {
            get
            {
                if (Dead)
                    return true;

                switch (StateMachine.State)
                {
                    default:
                        return false;
                        
                    case StIntroJump:
                    case StIntroWalk:
                    case StIntroWakeUp:
                    case StIntroRespawn:
                    case StSummitLaunch:
                        return true;
                }
            }
        }

        public bool InControl
        {
            get
            {
                switch (StateMachine.State)
                {
                    default:
                        return true;
                        
                    case StIntroJump:
                    case StIntroWalk:
                    case StIntroWakeUp:
                    case StIntroRespawn:
                    case StDummy:
                    case StFrozen:
                    case StBirdDashTutorial:
                        return false;
                }
            }
        }

        public PlayerInventory Inventory
        {
            get
            {
                if (level != null && level.Session != null)
                    return level.Session.Inventory;
                return PlayerInventory.Default;
            }
        }
        
        #endregion

        #region Transitions

        public void OnTransition()
        {
            wallSlideTimer = WallSlideTime;
            jumpGraceTimer = 0;
            forceMoveXTimer = 0;
            chaserStates.Clear();
            RefillDash();
            RefillStamina();

            Leader.TransferFollowers();
        }

        public bool TransitionTo(Vector2 target, Vector2 direction)
        {
            MoveTowardsX(target.X, 60f * Engine.DeltaTime);
            MoveTowardsY(target.Y, 60f * Engine.DeltaTime);

            //Update hair because the normal update loop is not firing right now!
            UpdateHair(false);
            UpdateCarry();

            //Returns true when transition is complete
            if (Position == target)
            {
                ZeroRemainderX();
                ZeroRemainderY();
                Speed.X = (int)Math.Round(Speed.X);
                Speed.Y = (int)Math.Round(Speed.Y);
                return true;
            }
            else
                return false;
        }

        public void BeforeSideTransition()
        {
            
        }

        public void BeforeDownTransition()
        {
            if (StateMachine.State != StRedDash && StateMachine.State != StReflectionFall && StateMachine.State != StStarFly)
            {
                StateMachine.State = StNormal;

                Speed.Y = Math.Max(0, Speed.Y);
                AutoJump = false;
                varJumpTimer = 0;
            }

            foreach (var platform in Scene.Tracker.GetEntities<Platform>())
                if (!(platform is SolidTiles) && CollideCheckOutside(platform, Position + Vector2.UnitY * Height))
                    platform.Collidable = false;
        }

        public void BeforeUpTransition()
        {
            Speed.X = 0;
            
            if (StateMachine.State != StRedDash && StateMachine.State != StReflectionFall && StateMachine.State != StStarFly)
            {
                varJumpSpeed = Speed.Y = JumpSpeed;

                if (StateMachine.State == StSummitLaunch)
                    StateMachine.State = StIntroJump;
                else
                    StateMachine.State = StNormal;

                AutoJump = true;
                AutoJumpTimer = 0;
                varJumpTimer = VarJumpTime;
            }

            dashCooldownTimer = 0.2f;
        }

        #endregion

        #region Jumps 'n' Stuff
        
        public bool OnSafeGround
        {
            get; private set;
        }

        public bool LoseShards
        {
            get
            {
                return onGround;
            }
        }
        
        private const float LaunchedBoostCheckSpeedSq = 100 * 100;
        private const float LaunchedJumpCheckSpeedSq = 220 * 220;
        private const float LaunchedMinSpeedSq = 140 * 140;
        private const float LaunchedDoubleSpeedSq = 150 * 150;

        private bool LaunchedBoostCheck()
        {
            if (LiftBoost.LengthSquared() >= LaunchedBoostCheckSpeedSq && Speed.LengthSquared() >= LaunchedJumpCheckSpeedSq)
            {
                launched = true;
                return true;
            }
            else
            {
                launched = false;
                return false;
            }
        }

        public void Jump(bool particles = true, bool playSfx = true)
        {
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0;
            varJumpTimer = VarJumpTime;
            AutoJump = false;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;

            Speed.X += JumpHBoost * moveX;
            Speed.Y = JumpSpeed;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;

            LaunchedBoostCheck();

            if (playSfx)
            {
                if (launched)
                    Play(Sfxs.char_mad_jump_assisted);

                if (dreamJump)
                    Play(Sfxs.char_mad_jump_dreamblock);
                else
                    Play(Sfxs.char_mad_jump);
            }

            Sprite.Scale = new Vector2(.6f, 1.4f);
            if (particles)
                Dust.Burst(BottomCenter, Calc.Up, 4);

            SaveData.Instance.TotalJumps++;
        }

        private void SuperJump()
        {
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0;
            varJumpTimer = VarJumpTime;
            AutoJump = false;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;

            Speed.X = SuperJumpH * (int)Facing;
            Speed.Y = JumpSpeed;
            Speed += LiftBoost;

            Play(Sfxs.char_mad_jump);

            if (Ducking)
            {
                Ducking = false;
                Speed.X *= DuckSuperJumpXMult;
                Speed.Y *= DuckSuperJumpYMult;
                Play(Sfxs.char_mad_jump_superslide);
            }
            else
                Play(Sfxs.char_mad_jump_super);

            varJumpSpeed = Speed.Y;
            launched = true;

            Sprite.Scale = new Vector2(.6f, 1.4f);

            Dust.Burst(BottomCenter, Calc.Up, 4);

            SaveData.Instance.TotalJumps++;
        }

        private bool WallJumpCheck(int dir)
        {
            return ClimbBoundsCheck(dir) && CollideCheck<Solid>(Position + Vector2.UnitX * dir * WallJumpCheckDist);
        }

        private void WallJump(int dir)
        {
            Ducking = false;
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0;
            varJumpTimer = VarJumpTime;
            AutoJump = false;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;
            if (moveX != 0)
            {
                forceMoveX = dir;
                forceMoveXTimer = WallJumpForceTime;
            }

            //Get lift of wall jumped off of
            if (LiftSpeed == Vector2.Zero)
            {
                Solid wall = CollideFirst<Solid>(Position + Vector2.UnitX * WallJumpCheckDist);
                if (wall != null)
                    LiftSpeed = wall.LiftSpeed;
            }

            Speed.X = WallJumpHSpeed * dir;
            Speed.Y = JumpSpeed;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;

            LaunchedBoostCheck();

            // wall-sound?
            var pushOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * dir * 4, temp));
            if (pushOff != null)
                Play(Sfxs.char_mad_land, SurfaceIndex.Param, pushOff.GetWallSoundIndex(this, -dir));

            // jump sfx
            Play(dir < 0 ? Sfxs.char_mad_jump_wall_right : Sfxs.char_mad_jump_wall_left);
            Sprite.Scale = new Vector2(.6f, 1.4f);

            if (dir == -1)
                Dust.Burst(Center + Vector2.UnitX * 2, Calc.UpLeft, 4);
            else
                Dust.Burst(Center + Vector2.UnitX * -2, Calc.UpRight, 4);

            SaveData.Instance.TotalWallJumps++;
        }

        private void SuperWallJump(int dir)
        {
            Ducking = false;
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0;
            varJumpTimer = SuperWallJumpVarTime;
            AutoJump = false;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;

            Speed.X = SuperWallJumpH * dir;
            Speed.Y = SuperWallJumpSpeed;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;
            launched = true;

            Play(dir < 0 ? Sfxs.char_mad_jump_wall_right : Sfxs.char_mad_jump_wall_left);
            Play(Sfxs.char_mad_jump_superwall);
            Sprite.Scale = new Vector2(.6f, 1.4f);

            if (dir == -1)
                Dust.Burst(Center + Vector2.UnitX * 2, Calc.UpLeft, 4);
            else
                Dust.Burst(Center + Vector2.UnitX * -2, Calc.UpRight, 4);

            SaveData.Instance.TotalWallJumps++;
        }

        private void ClimbJump()
        {
            if (!onGround)
            {
                Stamina -= ClimbJumpCost;

                sweatSprite.Play("jump", true);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            }

            dreamJump = false;
            Jump(false, false);

            if (moveX == 0)
            {
                wallBoostDir = -(int)Facing;
                wallBoostTimer = ClimbJumpBoostTime;
            }

            if (Facing == Facings.Right)
            {
                Play(Sfxs.char_mad_jump_climb_right);
                Dust.Burst(Center + Vector2.UnitX * 2, Calc.UpLeft, 4);
            }
            else
            {
                Play(Sfxs.char_mad_jump_climb_left);
                Dust.Burst(Center + Vector2.UnitX * -2, Calc.UpRight, 4);
            }
        }

        public void Bounce(float fromY)
        {
            if (StateMachine.State == StBoost && CurrentBooster != null)
            {
                CurrentBooster.PlayerReleased();
                CurrentBooster = null;
            }

            var was = Collider;
            Collider = normalHitbox;
            {
                MoveVExact((int)(fromY - Bottom));
                if (!Inventory.NoRefills)
                    RefillDash();
                RefillStamina();
                StateMachine.State = StNormal;

                jumpGraceTimer = 0;
                varJumpTimer = BounceVarJumpTime;
                AutoJump = true;
                AutoJumpTimer = BounceAutoJumpTime;
                dashAttackTimer = 0;
                wallSlideTimer = WallSlideTime;
                wallBoostTimer = 0;

                varJumpSpeed = Speed.Y = BounceSpeed;
                launched = false;

                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                Sprite.Scale = new Vector2(.6f, 1.4f);
            }
            Collider = was;
        }

        public void SuperBounce(float fromY)
        {
            if (StateMachine.State == StBoost && CurrentBooster != null)
            {
                CurrentBooster.PlayerReleased();
                CurrentBooster = null;
            }

            var was = Collider;
            Collider = normalHitbox;

            {
                MoveV(fromY - Bottom);
                if (!Inventory.NoRefills)
                    RefillDash();
                RefillStamina();
                StateMachine.State = StNormal;

                jumpGraceTimer = 0;
                varJumpTimer = SuperBounceVarJumpTime;
                AutoJump = true;
                AutoJumpTimer = 0;
                dashAttackTimer = 0;
                wallSlideTimer = WallSlideTime;
                wallBoostTimer = 0;

                Speed.X = 0;
                varJumpSpeed = Speed.Y = SuperBounceSpeed;
                launched = false;

                level.DirectionalShake(-Vector2.UnitY, .1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Sprite.Scale = new Vector2(.5f, 1.5f);
            }

            Collider = was;
        }

        private const float SideBounceSpeed = 240;
        private const float SideBounceForceMoveXTime = .3f;

        public void SideBounce(int dir, float fromX, float fromY)
        {
            var was = Collider;
            Collider = normalHitbox;
            {
                MoveV(Calc.Clamp(fromY - Bottom, -4, 4));
                if (dir > 0)
                    MoveH(fromX - Left);
                else if (dir < 0)
                    MoveH(fromX - Right);
                if (!Inventory.NoRefills)
                    RefillDash();
                RefillStamina();
                StateMachine.State = StNormal;

                jumpGraceTimer = 0;
                varJumpTimer = BounceVarJumpTime;
                AutoJump = true;
                AutoJumpTimer = 0;
                dashAttackTimer = 0;
                wallSlideTimer = WallSlideTime;
                forceMoveX = dir;
                forceMoveXTimer = SideBounceForceMoveXTime;
                wallBoostTimer = 0;
                launched = false;

                Speed.X = SideBounceSpeed * dir;
                varJumpSpeed = Speed.Y = BounceSpeed;

                level.DirectionalShake(Vector2.UnitX * dir, .1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Sprite.Scale = new Vector2(1.5f, .5f);
            }
            Collider = was;
        }

        public void Rebound(int direction = 0)
        {
            Speed.X = direction * ReboundSpeedX;
            Speed.Y = ReboundSpeedY;
            varJumpSpeed = Speed.Y;
            varJumpTimer = ReboundVarJumpTime;
            AutoJump = true;
            AutoJumpTimer = 0;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;
            launched = false;

            dashAttackTimer = 0;
            forceMoveXTimer = 0;
            StateMachine.State = StNormal;
        }

        public void ReflectBounce(Vector2 direction)
        {
            if (direction.X != 0)
                Speed.X = direction.X * ReflectBoundSpeed;
            if (direction.Y != 0)
                Speed.Y = direction.Y * ReflectBoundSpeed;

            AutoJumpTimer = 0;
            dashAttackTimer = 0;
            wallSlideTimer = WallSlideTime;
            wallBoostTimer = 0;
            launched = false;

            dashAttackTimer = 0;
            forceMoveXTimer = 0;
            StateMachine.State = StNormal;
        }

        public int MaxDashes
        {
            get
            {
                if (SaveData.Instance.AssistMode && SaveData.Instance.Assists.DashMode != Assists.DashModes.Normal && !level.InCutscene)
                    return 2;
                else
                    return Inventory.Dashes;
            }
        }

        public bool RefillDash()
        {
            if (Dashes < MaxDashes)
            {
                Dashes = MaxDashes;
                return true;
            }
            else
                return false;
        }

        public bool UseRefill()
        {
            if (Dashes < MaxDashes || Stamina < ClimbTiredThreshold)
            {
                Dashes = MaxDashes;
                RefillStamina();
                return true;
            }
            else
                return false;
        }

        public void RefillStamina()
        {
            Stamina = ClimbMaxStamina;
        }

        public PlayerDeadBody Die(Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
        {
            var session = level.Session;
            bool invincible = (!evenIfInvincible && SaveData.Instance.AssistMode && SaveData.Instance.Assists.Invincible);

            if (!Dead && !invincible && StateMachine.State != StReflectionFall)
            {
                Stop(wallSlideSfx);

                if (registerDeathInStats)
                {
                    session.Deaths++;
                    session.DeathsInCurrentLevel++;
                    SaveData.Instance.AddDeath(session.Area);
                }

                // has gold strawb?
                Strawberry goldenStrawb = null;
                foreach (var strawb in Leader.Followers)
                    if (strawb.Entity is Strawberry && (strawb.Entity as Strawberry).Golden && !(strawb.Entity as Strawberry).Winged)
                        goldenStrawb = (strawb.Entity as Strawberry);

                Dead = true;
                Leader.LoseFollowers();
                Depth = Depths.Top;
                Speed = Vector2.Zero;
                StateMachine.Locked = true;
                Collidable = false;
                Drop();

                if (lastBooster != null)
                    lastBooster.PlayerDied();

                level.InCutscene = false;
                level.Shake();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

                var body = new PlayerDeadBody(this, direction);
                if (goldenStrawb != null)
                {
                    body.DeathAction = () =>
                    {
                        var exit = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session);
                        exit.GoldenStrawberryEntryLevel = goldenStrawb.ID.Level;
                        Engine.Scene = exit;
                    };
                }

                Scene.Add(body);
                Scene.Remove(this);

                var lookout = Scene.Tracker.GetEntity<Lookout>();
                if (lookout != null)
                    lookout.StopInteracting();

                return body;
            }

            return null;
        }

        private Vector2 LiftBoost
        {
            get
            {
                Vector2 val = LiftSpeed;

                if (Math.Abs(val.X) > LiftXCap)
                    val.X = LiftXCap * Math.Sign(val.X);

                if (val.Y > 0)
                    val.Y = 0;
                else if (val.Y < LiftYCap)
                    val.Y = LiftYCap;

                return val;
            }
        }

        #endregion

        #region Ducking

        public bool Ducking
        {
            get
            {
                return Collider == duckHitbox || Collider == duckHurtbox;
            }

            set
            {
                if (value)
                {
                    Collider = duckHitbox;
                    hurtbox = duckHurtbox;
                    
                }
                else
                {
                    Collider = normalHitbox;
                    hurtbox = normalHurtbox;
                }
            }
        }

        public bool CanUnDuck
        {
            get
            {
                if (!Ducking)
                    return true;

                Collider was = Collider;
                Collider = normalHitbox;
                bool ret = !CollideCheck<Solid>();
                Collider = was;
                return ret;
            }
        }

        public bool CanUnDuckAt(Vector2 at)
        {
            Vector2 was = Position;
            Position = at;
            bool ret = CanUnDuck;
            Position = was;
            return ret;
        }

        public bool DuckFreeAt(Vector2 at)
        {
            Vector2 oldP = Position;
            Collider oldC = Collider;
            Position = at;
            Collider = duckHitbox;

            bool ret = !CollideCheck<Solid>();

            Position = oldP;
            Collider = oldC;

            return ret;
        }

        private void Duck()
        {
            Collider = duckHitbox;
        }

        private void UnDuck()
        {
            Collider = normalHitbox;
        }

        #endregion

        #region Holding

        public Holdable Holding
        {
            get; set;
        }

        public void UpdateCarry()
        {
            if (Holding != null)
            {
                // don't hold stuff that doesn't exist anymore
                if (Holding.Scene == null)
                    Holding = null;
                else
                    Holding.Carry(Position + carryOffset + Vector2.UnitY * Sprite.CarryYOffset);
            }
        }

        public void Swat(int dir)
        {
            if (Holding != null)
            {
                Holding.Release(new Vector2(.8f * dir, -.25f));
                Holding = null;
            }
        }

        private bool Pickup(Holdable pickup)
        {
            if (pickup.Pickup(this))
            {
                Ducking = false;
                Holding = pickup;
                minHoldTimer = HoldMinTime;
                return true;
            }
            else
                return false;
        }

        private void Throw()
        {
            if (Holding != null)
            {
                if (Input.MoveY.Value == 1)
                    Drop();
                else
                {
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                    Holding.Release(Vector2.UnitX * (int)Facing);
                    Speed.X += ThrowRecoil * -(int)Facing;
                }

                Play(Sfxs.char_mad_crystaltheo_throw);
                Sprite.Play("throw");
                Holding = null;
            }
        }

        private void Drop()
        {
            if (Holding != null)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                Holding.Release(Vector2.Zero);
                Holding = null;
            }
        }

        #endregion

        #region Physics

        public void StartJumpGraceTime()
        {
            jumpGraceTimer = JumpGraceTime;
        }

        public override bool IsRiding(Solid solid)
        {
            if (StateMachine.State == StDreamDash)
                return CollideCheck(solid);

            if (StateMachine.State == StClimb || StateMachine.State == StHitSquash)
                return CollideCheck(solid, Position + Vector2.UnitX * (int)Facing);

            return base.IsRiding(solid);
        }

        public override bool IsRiding(JumpThru jumpThru)
        {
            if (StateMachine.State == StDreamDash)
                return false;

            return StateMachine.State != StClimb && Speed.Y >= 0 && base.IsRiding(jumpThru);
        }

        public bool BounceCheck(float y)
        {
            return Bottom <= y + 3;
        }

        public void PointBounce(Vector2 from)
        {
            const float BounceSpeed = 200f;
            const float MinX = 120f;

            if (StateMachine.State == StBoost && CurrentBooster != null)
                CurrentBooster.PlayerReleased();

            RefillDash();
            RefillStamina();
            Speed = (Center - from).SafeNormalize(BounceSpeed);
            Speed.X *= 1.2f;

            if (Math.Abs(Speed.X) < MinX)
            {
                if (Speed.X == 0)
                    Speed.X = -(int)Facing * MinX;
                else
                    Speed.X = Math.Sign(Speed.X) * MinX;
            }
        }

        private void WindMove(Vector2 move)
        {
            if (!JustRespawned && noWindTimer <= 0 && InControl && StateMachine.State != StBoost && StateMachine.State != StDash && StateMachine.State != StSummitLaunch)
            {
                //Horizontal
                if (move.X != 0 && StateMachine.State != StClimb)
                {
                    windTimeout = 0.2f;
                    windDirection.X = Math.Sign(move.X);

                    if (!CollideCheck<Solid>(Position + Vector2.UnitX * -Math.Sign(move.X) * WindWallDistance))
                    {
                        if (Ducking && onGround)
                            move.X *= DuckWindMult;

                        if (move.X < 0)
                            move.X = Math.Max(move.X, level.Bounds.Left - (ExactPosition.X + Collider.Left));
                        else
                            move.X = Math.Min(move.X, level.Bounds.Right - (ExactPosition.X + Collider.Right));

                        MoveH(move.X);
                    }
                }

                //Vertical
                if (move.Y != 0)
                {
                    windTimeout = 0.2f;
                    windDirection.Y = Math.Sign(move.Y);

                    if (Speed.Y < 0 || !OnGround())
                    {

                        if (StateMachine.State == StClimb)
                        {
                            if (move.Y > 0 && climbNoMoveTimer <= 0)
                                move.Y *= .4f;
                            else
                                return;
                        }

                        MoveV(move.Y);
                    }
                }
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (StateMachine.State == StStarFly)
            {
                if (starFlyTimer < StarFlyEndNoBounceTime)
                    Speed.X = 0;
                else
                {
                    Play(Sfxs.game_06_feather_state_bump);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    Speed.X *= StarFlyWallBounce;
                }
                return;
            }

            if (StateMachine.State == StDreamDash)
                return;

            //Dash Blocks
            if (DashAttacking && data.Hit != null && data.Hit.OnDashCollide != null && data.Direction.X == Math.Sign(DashDir.X))
            {
                var result = data.Hit.OnDashCollide(this, data.Direction);
                if (StateMachine.State == StRedDash)
                    result = DashCollisionResults.Ignore;

                if (result == DashCollisionResults.Rebound)
                {
                    Rebound(-Math.Sign(Speed.X));
                    return;
                }
                else if (result == DashCollisionResults.Bounce)
                {
                    ReflectBounce(new Vector2(-Math.Sign(Speed.X), 0));
                    return;
                }
                else if (result == DashCollisionResults.Ignore)
                    return;
            }

            //Dash corner correction
            if (StateMachine.State == StDash || StateMachine.State == StRedDash)
            {
                if (onGround && DuckFreeAt(Position + Vector2.UnitX * Math.Sign(Speed.X)))
                {
                    Ducking = true;
                    return;
                }
                else if (Speed.Y == 0 && Speed.X != 0)
                {
                    for (int i = 1; i <= DashCornerCorrection; i++)
                    {
                        for (int j = 1; j >= -1; j -= 2)
                        {
                            if (!CollideCheck<Solid>(Position + new Vector2(Math.Sign(Speed.X), i * j)))
                            {
                                MoveVExact(i * j);
                                MoveHExact(Math.Sign(Speed.X));
                                return;
                            }
                        }
                    }
                }
            }

            //Dream Dash
            if (DreamDashCheck(Vector2.UnitX * Math.Sign(Speed.X)))
            {
                StateMachine.State = StDreamDash;
                dashAttackTimer = 0;
                return;
            }

            //Speed retention
            if (wallSpeedRetentionTimer <= 0)
            {
                wallSpeedRetained = Speed.X;
                wallSpeedRetentionTimer = WallSpeedRetentionTime;
            }

            //Collide event
            if (data.Hit != null && data.Hit.OnCollide != null)
                data.Hit.OnCollide(data.Direction);

            Speed.X = 0;
            dashAttackTimer = 0;

            if (StateMachine.State == StRedDash)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                level.Displacement.AddBurst(Center, .5f, 8, 48, .4f, Ease.QuadOut, Ease.QuadOut);
                StateMachine.State = StHitSquash;
            }
        }

        private void OnCollideV(CollisionData data)
        {
            if (StateMachine.State == StStarFly)
            {
                if (starFlyTimer < StarFlyEndNoBounceTime)
                    Speed.Y = 0;
                else
                {
                    Play(Sfxs.game_06_feather_state_bump);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    Speed.Y *= StarFlyWallBounce;
                }
                return;
            }
            else if (StateMachine.State == StSwim)
            {
                Speed.Y = 0;
                return;
            }
            else if (StateMachine.State == StDreamDash)
                return;

            //Dash Blocks
            if (data.Hit != null && data.Hit.OnDashCollide != null)
            {
                if (DashAttacking && data.Direction.Y == Math.Sign(DashDir.Y))
                {
                    var result = data.Hit.OnDashCollide(this, data.Direction);
                    if (StateMachine.State == StRedDash)
                        result = DashCollisionResults.Ignore;

                    if (result == DashCollisionResults.Rebound)
                    {
                        Rebound(0);
                        return;
                    }
                    else if (result == DashCollisionResults.Bounce)
                    {
                        ReflectBounce(new Vector2(0, -Math.Sign(Speed.Y)));
                        return;
                    }
                    else if (result == DashCollisionResults.Ignore)
                        return;
                }
                else if (StateMachine.State == StSummitLaunch)
                {
                    data.Hit.OnDashCollide(this, data.Direction);
                    return;
                }
            }

            if (Speed.Y > 0)
            {
                //Dash corner correction
                if ((StateMachine.State == StDash || StateMachine.State == StRedDash) && !dashStartedOnGround)
                {
                    if (Speed.X <= 0)
                    {
                        for (int i = -1; i >= -DashCornerCorrection; i--)
                        {
                            if (!OnGround(Position + new Vector2(i, 0)))
                            {
                                MoveHExact(i);
                                MoveVExact(1);
                                return;
                            }
                        }
                    }

                    if (Speed.X >= 0)
                    {
                        for (int i = 1; i <= DashCornerCorrection; i++)
                        {
                            if (!OnGround(Position + new Vector2(i, 0)))
                            {
                                MoveHExact(i);
                                MoveVExact(1);
                                return;
                            }
                        }
                    }
                }

                //Dream Dash
                if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
                {
                    StateMachine.State = StDreamDash;
                    dashAttackTimer = 0;
                    return;
                }

                //Dash Slide
                if (DashDir.X != 0 && DashDir.Y > 0 && Speed.Y > 0)
                {
                    DashDir.X = Math.Sign(DashDir.X);
                    DashDir.Y = 0;
                    Speed.Y = 0;
                    Speed.X *= DodgeSlideSpeedMult;
                    Ducking = true;
                }

                //Landed on ground effects
                if (StateMachine.State != StClimb)
                {
                    float squish = Math.Min(Speed.Y / FastMaxFall, 1);
                    Sprite.Scale.X = MathHelper.Lerp(1, 1.6f, squish);
                    Sprite.Scale.Y = MathHelper.Lerp(1, .4f, squish);

                    if (Speed.Y >= MaxFall / 2)
                        Dust.Burst(Position, Calc.Angle(new Vector2(0, -1)), 8);

                    if (highestAirY < Y - 50 && Speed.Y >= MaxFall && Math.Abs(Speed.X) >= MaxRun)
                        Sprite.Play(PlayerSprite.RunStumble);

                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);

                    // landed SFX
                    var platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + new Vector2(0, 1), temp));
                    if (platform != null)
                    {
                        var surface = platform.GetLandSoundIndex(this);
                        if (surface >= 0)
                            Play(playFootstepOnLand > 0f ? Sfxs.char_mad_footstep : Sfxs.char_mad_land, SurfaceIndex.Param, surface);
                        if (platform is DreamBlock)
                            (platform as DreamBlock).FootstepRipple(Position);
                    }

                    playFootstepOnLand = 0f;
                }
            }
            else 
            {
                if (Speed.Y < 0)
                {
                    //Corner Correction
                    {
                        if (Speed.X <= 0)
                        {
                            for (int i = 1; i <= UpwardCornerCorrection; i++)
                            {
                                if (!CollideCheck<Solid>(Position + new Vector2(-i, -1)))
                                {
                                    Position += new Vector2(-i, -1);
                                    return;
                                }
                            }
                        }

                        if (Speed.X >= 0)
                        {
                            for (int i = 1; i <= UpwardCornerCorrection; i++)
                            {
                                if (!CollideCheck<Solid>(Position + new Vector2(i, -1)))
                                {
                                    Position += new Vector2(i, -1);
                                    return;
                                }
                            }
                        }
                    }

                    if (varJumpTimer < VarJumpTime - CeilingVarJumpGrace)
                        varJumpTimer = 0;
                }

                //Dream Dash
                if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
                {
                    StateMachine.State = StDreamDash;
                    dashAttackTimer = 0;
                    return;
                }
            }

            //Collide event
            if (data.Hit != null && data.Hit.OnCollide != null)
                data.Hit.OnCollide(data.Direction);

            dashAttackTimer = 0;
            Speed.Y = 0;

            if (StateMachine.State == StRedDash)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                level.Displacement.AddBurst(Center, .5f, 8, 48, .4f, Ease.QuadOut, Ease.QuadOut);
                StateMachine.State = StHitSquash;
            }
        }

        private bool DreamDashCheck(Vector2 dir)
        {
            if (Inventory.DreamDash && DashAttacking && (dir.X == Math.Sign(DashDir.X) || dir.Y == Math.Sign(DashDir.Y)))
            {
                var block = CollideFirst<DreamBlock>(Position + dir);

                if (block != null)
                {
                    if (CollideCheck<Solid, DreamBlock>(Position + dir))
                    {
                        Vector2 side = new Vector2(Math.Abs(dir.Y), Math.Abs(dir.X));

                        bool checkNegative, checkPositive;
                        if (dir.X != 0)
                        {
                            checkNegative = Speed.Y <= 0;
                            checkPositive = Speed.Y >= 0;
                        }
                        else
                        {
                            checkNegative = Speed.X <= 0;
                            checkPositive = Speed.X >= 0;
                        }

                        if (checkNegative)
                        {
                            for (int i = -1; i >= -DashCornerCorrection; i--)
                            {
                                var at = Position + dir + side * i;
                                if (!CollideCheck<Solid, DreamBlock>(at))
                                {
                                    Position += side * i;
                                    dreamBlock = block;
                                    return true;
                                }
                            }
                        }

                        if (checkPositive)
                        {
                            for (int i = 1; i <= DashCornerCorrection; i++)
                            {
                                var at = Position + dir + side * i;
                                if (!CollideCheck<Solid, DreamBlock>(at))
                                {
                                    Position += side * i;
                                    dreamBlock = block;
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                    else
                    {
                        dreamBlock = block;
                        return true;
                    }
                }        
            }

            return false;
        }

        public void OnBoundsH()
        {
            Speed.X = 0;

            if (StateMachine.State == StRedDash)
                StateMachine.State = StNormal;
        }

        public void OnBoundsV()
        {
            Speed.Y = 0;

            if (StateMachine.State == StRedDash)
                StateMachine.State = StNormal;
        }

        override protected void OnSquish(CollisionData data)
        {
            bool ducked = false;
            if (!Ducking)
            {
                ducked = true;
                Ducking = true;
                data.Pusher.Collidable = true;

                if (!CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                var was = Position;
                Position = data.TargetPosition;
                if (!CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                Position = was;
                data.Pusher.Collidable = false;
            }

            if (!TrySquishWiggle(data))
                Die(Vector2.Zero);
            else if (ducked && CanUnDuck)
                Ducking = false;
        }

        #endregion

        #region Normal State

        private void NormalBegin()
        {
            maxFall = MaxFall;
        }

        private void NormalEnd()
        {
            wallBoostTimer = 0;
            wallSpeedRetentionTimer = 0;
            hopWaitX = 0;
        }

        public bool ClimbBoundsCheck(int dir)
        {
            return Left + dir * ClimbCheckDist >= level.Bounds.Left && Right + dir * ClimbCheckDist < level.Bounds.Right;
        }

        public bool ClimbCheck(int dir, int yAdd = 0)
        {
            return ClimbBoundsCheck(dir) && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitY * yAdd + Vector2.UnitX * ClimbCheckDist * (int)Facing) && CollideCheck<Solid>(Position + new Vector2(dir * ClimbCheckDist, yAdd));
        }

        private const float SpacePhysicsMult = .6f;

        private int NormalUpdate()
        {
            //Use Lift Boost if walked off platform
            if (LiftBoost.Y < 0 && wasOnGround && !onGround && Speed.Y >= 0)
                Speed.Y = LiftBoost.Y;

            if (Holding == null)
            {
                if (Input.Grab.Check && !IsTired && !Ducking)
                {
                    //Grabbing Holdables
                    foreach (Holdable hold in Scene.Tracker.GetComponents<Holdable>())
                        if (hold.Check(this) && Pickup(hold))
                            return StPickup;

                    //Climbing
                    if (Speed.Y >= 0 && Math.Sign(Speed.X) != -(int)Facing)
                    {
                        if (ClimbCheck((int)Facing))
                        {
                            Ducking = false;
                            return StClimb;
                        }

                        if (Input.MoveY < 1 && level.Wind.Y <= 0)
                        {
                            for (int i = 1; i <= ClimbUpCheckDist; i++)
                            {
                                if (!CollideCheck<Solid>(Position + Vector2.UnitY * -i) && ClimbCheck((int)Facing, -i))
                                {
                                    MoveVExact(-i);
                                    Ducking = false;
                                    return StClimb;
                                }
                            }
                        }
                    }
                }

                //Dashing
                if (CanDash)
                {
                    Speed += LiftBoost;                   
                    return StartDash();
                }

                //Ducking
                if (Ducking)
                {
                    if (onGround && Input.MoveY != 1)
                    {
                        if (CanUnDuck)
                        {
                            Ducking = false;
                            Sprite.Scale = new Vector2(.8f, 1.2f);
                        }
                        else if (Speed.X == 0)
                        {
                            for (int i = DuckCorrectCheck; i > 0; i--)
                            {
                                if (CanUnDuckAt(Position + Vector2.UnitX * i))
                                {
                                    MoveH(DuckCorrectSlide * Engine.DeltaTime);
                                    break;
                                }
                                else if (CanUnDuckAt(Position - Vector2.UnitX * i))
                                {
                                    MoveH(-DuckCorrectSlide * Engine.DeltaTime);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if(onGround && Input.MoveY == 1 && Speed.Y >= 0)
                {
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, .6f);
                }
            }
            else
            {
                //Throw
                if (!Input.Grab.Check && minHoldTimer <= 0)
                    Throw();

                //Ducking
                if (!Ducking && onGround && Input.MoveY == 1 && Speed.Y >= 0)
                {
                    Drop();
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, .6f);
                }
            }

            //Running and Friction
            if (Ducking && onGround)
                Speed.X = Calc.Approach(Speed.X, 0, DuckFriction * Engine.DeltaTime);
            else
            {
                float mult = onGround ? 1 : AirMult;
                if (onGround && level.CoreMode == Session.CoreModes.Cold)
                    mult *= .3f;

                float max = Holding == null ? MaxRun : HoldingMaxRun;
                if (level.InSpace)
                    max *= SpacePhysicsMult;
                if (Math.Abs(Speed.X) > max && Math.Sign(Speed.X) == moveX)
                    Speed.X = Calc.Approach(Speed.X, max * moveX, RunReduce * mult * Engine.DeltaTime);  //Reduce back from beyond the max speed
                else
                    Speed.X = Calc.Approach(Speed.X, max * moveX, RunAccel * mult * Engine.DeltaTime);   //Approach the max speed
            }

            //Vertical
            {
                //Calculate current max fall speed
                {
                    float mf = MaxFall;
                    float fmf = FastMaxFall;

                    if (level.InSpace)
                    {
                        mf *= SpacePhysicsMult;
                        fmf *= SpacePhysicsMult;
                    }

                    //Fast Fall
                    if (Input.MoveY == 1 && Speed.Y >= mf)
                    {
                        maxFall = Calc.Approach(maxFall, fmf, FastMaxAccel * Engine.DeltaTime);

                        float half = mf + (fmf - mf) * .5f;
                        if (Speed.Y >= half)
                        {
                            float spriteLerp = Math.Min(1f, (Speed.Y - half) / (fmf - half));
                            Sprite.Scale.X = MathHelper.Lerp(1f, .5f, spriteLerp);
                            Sprite.Scale.Y = MathHelper.Lerp(1f, 1.5f, spriteLerp);
                        }
                    }
                    else
                        maxFall = Calc.Approach(maxFall, mf, FastMaxAccel * Engine.DeltaTime);
                }

                //Gravity
                if (!onGround)
                {
                    float max = maxFall;

                    //Wall Slide
                    if ((moveX == (int)Facing || (moveX == 0 && Input.Grab.Check)) && Input.MoveY.Value != 1)
                    {
                        if (Speed.Y >= 0 && wallSlideTimer > 0 && Holding == null && ClimbBoundsCheck((int)Facing) && CollideCheck<Solid>(Position + Vector2.UnitX * (int)Facing) && CanUnDuck)
                        {
                            Ducking = false;
                            wallSlideDir = (int)Facing;
                        }

                        if (wallSlideDir != 0)
                        {
                            if (wallSlideTimer > WallSlideTime * .5f && ClimbBlocker.Check(level, this, Position + Vector2.UnitX * wallSlideDir))
                                wallSlideTimer = WallSlideTime * .5f;

                            max = MathHelper.Lerp(MaxFall, WallSlideStartMax, wallSlideTimer / WallSlideTime);
                            if (wallSlideTimer / WallSlideTime > .65f)
                                CreateWallSlideParticles(wallSlideDir);
                        }
                    }

                    float mult = (Math.Abs(Speed.Y) < HalfGravThreshold && (Input.Jump.Check || AutoJump)) ? .5f : 1f;

                    if (level.InSpace)
                        mult *= SpacePhysicsMult;

                    Speed.Y = Calc.Approach(Speed.Y, max, Gravity * mult * Engine.DeltaTime);
                }

                //Variable Jumping
                if (varJumpTimer > 0)
                {
                    if (AutoJump || Input.Jump.Check)
                        Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
                    else
                        varJumpTimer = 0;
                }

                //Jumping
                if (Input.Jump.Pressed)
                {
                    Water water = null;
                    if (jumpGraceTimer > 0)
                    {
                        Jump();
                    }
                    else if (CanUnDuck)
                    {
                        bool canUnduck = CanUnDuck;
                        if (canUnduck && WallJumpCheck(1))
                        {
                            if (Facing == Facings.Right && Input.Grab.Check && Stamina > 0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * WallJumpCheckDist))
                                ClimbJump();
                            else if (DashAttacking && DashDir.X == 0 && DashDir.Y == -1)
                                SuperWallJump(-1);
                            else
                                WallJump(-1);
                        }
                        else if (canUnduck && WallJumpCheck(-1))
                        {
                            if (Facing == Facings.Left && Input.Grab.Check && Stamina > 0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -WallJumpCheckDist))
                                ClimbJump();
                            else if (DashAttacking && DashDir.X == 0 && DashDir.Y == -1)
                                SuperWallJump(1);
                            else
                                WallJump(1);
                        }
                        else if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2)) != null)
                        {
                            Jump();
                            water.TopSurface.DoRipple(Position, 1);
                        }
                    }
                }
            }

            return StNormal;
        }

        public void CreateWallSlideParticles(int dir)
        {
            if (Scene.OnInterval(.01f))
            {
                Vector2 at = Center;
                if (dir == 1)
                    at += new Vector2(5, 4);
                else
                    at += new Vector2(-5, 4);
                Dust.Burst(at, Calc.Up, 1);
            }
        }

        #endregion

        #region Climb State

        private bool IsTired
        {
            get
            {
                return CheckStamina < ClimbTiredThreshold;
            }
        }

        private float CheckStamina
        {
            get
            {
                if (wallBoostTimer > 0)
                    return Stamina + ClimbJumpCost;
                else
                    return Stamina;
            }
        }

        private void PlaySweatEffectDangerOverride(string state)
        {
            if (Stamina <= ClimbTiredThreshold)
                sweatSprite.Play("danger");
            else
                sweatSprite.Play(state);
        }

        private FMOD.Studio.EventInstance conveyorLoopSfx;

        private void ClimbBegin()
        {
            AutoJump = false;
            Speed.X = 0;
            Speed.Y *= ClimbGrabYMult;
            wallSlideTimer = WallSlideTime;
            climbNoMoveTimer = ClimbNoMoveTime;
            wallBoostTimer = 0;
            lastClimbMove = 0;
            
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            for (int i = 0; i < ClimbCheckDist; i++)
                if (!CollideCheck<Solid>(Position + Vector2.UnitX * (int)Facing))
                    Position += Vector2.UnitX * (int)Facing;
                else
                    break;

            // tell the thing we grabbed it
            var platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Position + Vector2.UnitX * (int)Facing, temp));
            if (platform != null)
                Play(Sfxs.char_mad_grab, SurfaceIndex.Param, platform.GetWallSoundIndex(this, (int)Facing));
        }

        private void ClimbEnd()
        {
            if (conveyorLoopSfx != null)
            {
                conveyorLoopSfx.setParameterValue("end", 1);
                conveyorLoopSfx.release();
                conveyorLoopSfx = null;
            }
            wallSpeedRetentionTimer = 0;
            if (sweatSprite != null && sweatSprite.CurrentAnimationID != "jump")
                sweatSprite.Play("idle");
        }

        private const float WallBoosterSpeed = -160f;
        private const float WallBoosterLiftSpeed = -80;
        private const float WallBoosterAccel = 600f;
        private const float WallBoostingHopHSpeed = 100f;
        private const float WallBoosterOverTopSpeed = -180f;
        private const float IceBoosterSpeed = 40;
        private const float IceBoosterAccel = 300f;
        private bool wallBoosting;

        private int ClimbUpdate()
        {
            climbNoMoveTimer -= Engine.DeltaTime;
            
            //Refill stamina on ground
            if (onGround)
                Stamina = ClimbMaxStamina;

            //Wall Jump
            if (Input.Jump.Pressed && (!Ducking || CanUnDuck))
            {
                if (moveX == -(int)Facing)
                    WallJump(-(int)Facing);
                else
                    ClimbJump();

                return StNormal;
            }

            //Dashing
            if (CanDash)
            {
                Speed += LiftBoost;
                return StartDash();
            }

            //Let go
            if (!Input.Grab.Check)
            {
                Speed += LiftBoost;
                Play(Sfxs.char_mad_grab_letgo);
                return StNormal;
            }

            //No wall to hold
            if (!CollideCheck<Solid>(Position + Vector2.UnitX * (int)Facing))
            {
                //Climbed over ledge?
                if (Speed.Y < 0)
                {
                    if (wallBoosting)
                    {
                        Speed += LiftBoost;
                        Play(Sfxs.char_mad_grab_letgo);
                    }
                    else
                        ClimbHop();
                }

                return StNormal;
            }

            var booster = WallBoosterCheck();
            if (climbNoMoveTimer <= 0 && booster != null)
            {
                //Wallbooster
                wallBoosting = true;

                if (conveyorLoopSfx == null)
                    conveyorLoopSfx = Audio.Play(Sfxs.game_09_conveyor_activate, Position, "end", 0);
                Audio.Position(conveyorLoopSfx, Position);
                
                Speed.Y = Calc.Approach(Speed.Y, WallBoosterSpeed, WallBoosterAccel * Engine.DeltaTime);
                LiftSpeed = Vector2.UnitY * Math.Max(Speed.Y, WallBoosterLiftSpeed);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }
            else
            {
                wallBoosting = false;

                if (conveyorLoopSfx != null)
                {
                    conveyorLoopSfx.setParameterValue("end", 1);
                    conveyorLoopSfx.release();
                    conveyorLoopSfx = null;
                }

                //Climbing
                float target = 0;
                bool trySlip = false;
                if (climbNoMoveTimer <= 0)
                {
                    if (ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * (int)Facing))
                        trySlip = true;
                    else if (Input.MoveY.Value == -1)
                    {
                        target = ClimbUpSpeed;

                        //Up Limit
                        if (CollideCheck<Solid>(Position - Vector2.UnitY) || (ClimbHopBlockedCheck() && SlipCheck(-1)))
                        {
                            if (Speed.Y < 0)
                                Speed.Y = 0;
                            target = 0;
                            trySlip = true;
                        }
                        else if (SlipCheck())
                        {
                            //Hopping
                            ClimbHop();
                            return StNormal;
                        }
                    }
                    else if (Input.MoveY.Value == 1)
                    {
                        target = ClimbDownSpeed;

                        if (onGround)
                        {
                            if (Speed.Y > 0)
                                Speed.Y = 0;
                            target = 0;
                        }
                        else
                            CreateWallSlideParticles((int)Facing);
                    }
                    else
                        trySlip = true;
                }
                else
                    trySlip = true;
                lastClimbMove = Math.Sign(target);

                //Slip down if hands above the ledge and no vertical input
                if (trySlip && SlipCheck())
                    target = ClimbSlipSpeed;

                //Set Speed
                Speed.Y = Calc.Approach(Speed.Y, target, ClimbAccel * Engine.DeltaTime);
            }

            //Down Limit
            if (Input.MoveY.Value != 1 && Speed.Y > 0 && !CollideCheck<Solid>(Position + new Vector2((int)Facing, 1)))
                Speed.Y = 0;

            //Stamina
            if (climbNoMoveTimer <= 0)
            {
                if (lastClimbMove == -1)
                {
                    Stamina -= ClimbUpCost * Engine.DeltaTime;

                    if (Stamina <= ClimbTiredThreshold)
                        sweatSprite.Play("danger");
                    else if (sweatSprite.CurrentAnimationID != "climbLoop")
                        sweatSprite.Play("climb");
                    if (Scene.OnInterval(.2f))
                        Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                }
                else
                {
                    if (lastClimbMove == 0)
                        Stamina -= ClimbStillCost * Engine.DeltaTime;

                    if (!onGround)
                    {
                        PlaySweatEffectDangerOverride("still");
                        if (Scene.OnInterval(.8f))
                            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                    }
                    else
                        PlaySweatEffectDangerOverride("idle");
                }
            }
            else
                PlaySweatEffectDangerOverride("idle");

            //Too tired           
            if (Stamina <= 0)
            {
                Speed += LiftBoost;
                return StNormal;
            }

            return StClimb;
        }

        private WallBooster WallBoosterCheck()
        {
            if (ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * (int)Facing))
                return null;

            foreach (WallBooster booster in Scene.Tracker.GetEntities<WallBooster>())
                if (booster.Facing == Facing && CollideCheck(booster))
                    return booster;

            return null;
        }

        private void ClimbHop()
        {
            climbHopSolid = CollideFirst<Solid>(Position + Vector2.UnitX * (int)Facing);
            playFootstepOnLand = 0.5f;

            if (climbHopSolid != null)
            {
                climbHopSolidPosition = climbHopSolid.Position;
                hopWaitX = (int)Facing;
                hopWaitXSpeed = (int)Facing * ClimbHopX;
            }
            else
            {
                hopWaitX = 0;
                Speed.X = (int)Facing * ClimbHopX;
            }

            Speed.Y = Math.Min(Speed.Y, ClimbHopY);
            forceMoveX = 0;
            forceMoveXTimer = ClimbHopForceTime;
            fastJump = false;
            noWindTimer = ClimbHopNoWindTime;
            Play(Sfxs.char_mad_climb_ledge);
        }

        private bool SlipCheck(float addY = 0)
        {
            Vector2 at;
            if (Facing == Facings.Right)
                at = TopRight + Vector2.UnitY * (4 + addY);
            else
                at = TopLeft - Vector2.UnitX + Vector2.UnitY * (4 + addY);

            return !Scene.CollideCheck<Solid>(at) && !Scene.CollideCheck<Solid>(at + Vector2.UnitY * (-4 + addY));
        }

        private bool ClimbHopBlockedCheck()
        {
            //If you have a cassette note, you're blocked
            foreach (Follower follower in Leader.Followers)
                if (follower.Entity is StrawberrySeed)
                    return true;

            //If you hit a ledge blocker, you're blocked
            foreach (LedgeBlocker blocker in Scene.Tracker.GetComponents<LedgeBlocker>())
                if (blocker.HopBlockCheck(this))
                    return true;

            //If there's a solid in the way, you're blocked
            if (CollideCheck<Solid>(Position - Vector2.UnitY * 6))
                return true;

            return false;
        }

        private bool JumpThruBoostBlockedCheck()
        {
            foreach (LedgeBlocker blocker in Scene.Tracker.GetComponents<LedgeBlocker>())
                if (blocker.JumpThruBoostCheck(this))
                    return true;
            return false;
        }

        #endregion

        #region Dash State

        public int StartDash()
        {
            wasDashB = Dashes == 2;
            Dashes = Math.Max(0, Dashes - 1);
            Input.Dash.ConsumeBuffer();
            return StDash;
        }

        public bool DashAttacking
        {
            get
            {
                return dashAttackTimer > 0 || StateMachine.State == StRedDash;
            }
        }

        private Vector2 beforeDashSpeed;
        private bool wasDashB;

        public bool CanDash
        {
            get
            {
                return Input.Dash.Pressed && dashCooldownTimer <= 0 && Dashes > 0 &&
                    (TalkComponent.PlayerOver == null || !Input.Talk.Pressed);
            }
        }

        public bool StartedDashing
        {
            get; private set;
        }

        private void CallDashEvents()
        {
            if (!calledDashEvents)
            {
                calledDashEvents = true;
                if (CurrentBooster == null)
                {
                    // Increment Counter
                    SaveData.Instance.TotalDashes++;
                    level.Session.Dashes++;
                    Stats.Increment(Stat.DASHES);

                    // Sfxs
                    {
                        var rightDashSound = DashDir.Y < 0 || (DashDir.Y == 0 && DashDir.X > 0);
                        if (DashDir == Vector2.Zero)
                            rightDashSound = Facing == Facings.Right;

                        if (rightDashSound)
                        {
                            if (wasDashB)
                                Play(Sfxs.char_mad_dash_pink_right);
                            else
                                Play(Sfxs.char_mad_dash_red_right);
                        }
                        else
                        {
                            if (wasDashB)
                                Play(Sfxs.char_mad_dash_pink_left);
                            else
                                Play(Sfxs.char_mad_dash_red_left);
                        }

                        if (SwimCheck())
                            Play(Sfxs.char_mad_water_dash_gen);
                    }

                    //Dash Listeners
                    foreach (DashListener dl in Scene.Tracker.GetComponents<DashListener>())
                        if (dl.OnDash != null)
                            dl.OnDash(DashDir);
                }
                else
                {
                    //Booster
                    CurrentBooster.PlayerBoosted(this, DashDir);
                    CurrentBooster = null;
                }
            }
        }

        private void DashBegin()
        {
            calledDashEvents = false;
            dashStartedOnGround = onGround;
            launched = false;

            if (Engine.TimeRate > 0.25f)
                Celeste.Freeze(.05f);
            dashCooldownTimer = DashCooldown;
            dashRefillCooldownTimer = DashRefillCooldown;
            StartedDashing = true;
            wallSlideTimer = WallSlideTime;
            dashTrailTimer = 0;

            level.Displacement.AddBurst(Center, .4f, 8, 64, .5f, Ease.QuadOut, Ease.QuadOut);

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            dashAttackTimer = DashAttackTime;
            beforeDashSpeed = Speed;
            Speed = Vector2.Zero;
            DashDir = Vector2.Zero;

            if (!onGround && Ducking && CanUnDuck)
                Ducking = false;
        }

        private void DashEnd()
        {
            CallDashEvents();
        }

        private int DashUpdate()
        {
            StartedDashing = false;

            //Trail
            if (dashTrailTimer > 0)
            {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0)
                    CreateTrail();
            }

            //Grab Holdables
            if (Holding == null && Input.Grab.Check && !IsTired && CanUnDuck)
            {
                //Grabbing Holdables
                foreach (Holdable hold in Scene.Tracker.GetComponents<Holdable>())
                    if (hold.Check(this) && Pickup(hold))
                        return StPickup;
            }

            if (DashDir.Y == 0)
            {
                //JumpThru Correction
                foreach (JumpThru jt in Scene.Tracker.GetEntities<JumpThru>())
                    if (CollideCheck(jt) && Bottom - jt.Top <= DashHJumpThruNudge)
                        MoveVExact((int)(jt.Top - Bottom));

                //Super Jump
                if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0)
                {
                    SuperJump();
                    return StNormal;
                }
            }

            if (DashDir.X == 0 && DashDir.Y == -1)
            {
                if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        SuperWallJump(-1);
                        return StNormal;
                    }
                    else if (WallJumpCheck(-1))
                    {
                        SuperWallJump(1);
                        return StNormal;
                    }
                }
            }
            else
            {
                if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        WallJump(-1);
                        return StNormal;
                    }
                    else if (WallJumpCheck(-1))
                    {
                        WallJump(1);
                        return StNormal;
                    }
                }
            }

            if (Speed != Vector2.Zero && level.OnInterval(0.02f))
                level.ParticlesFG.Emit(wasDashB ? P_DashB : P_DashA, Center + Calc.Random.Range(Vector2.One *-2, Vector2.One * 2), DashDir.Angle());
            return StDash;
        }

        private IEnumerator DashCoroutine()
        {
            yield return null;

            var dir = lastAim;
            if (OverrideDashDirection.HasValue)
                dir = OverrideDashDirection.Value;

            var newSpeed = dir * DashSpeed;
            if (Math.Sign(beforeDashSpeed.X) == Math.Sign(newSpeed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(newSpeed.X))
                newSpeed.X = beforeDashSpeed.X;
            Speed = newSpeed;

            if (CollideCheck<Water>())
                Speed *= SwimDashSpeedMult;

            DashDir = dir;
            SceneAs<Level>().DirectionalShake(DashDir, .2f);

            if (DashDir.X != 0)
                Facing = (Facings)Math.Sign(DashDir.X);

            CallDashEvents();

            //Feather particles
            if (StateMachine.PreviousState == StStarFly)
                level.Particles.Emit(FlyFeather.P_Boost, 12, Center, Vector2.One * 4, (-dir).Angle());

            //Dash Slide
            if (onGround && DashDir.X != 0 && DashDir.Y > 0 && Speed.Y > 0 
                && (!Inventory.DreamDash || !CollideCheck<DreamBlock>(Position + Vector2.UnitY)))
            {
                DashDir.X = Math.Sign(DashDir.X);
                DashDir.Y = 0;
                Speed.Y = 0;
                Speed.X *= DodgeSlideSpeedMult;
                Ducking = true;
            }
            
            SlashFx.Burst(Center, DashDir.Angle());
            CreateTrail();
            dashTrailTimer = .08f;

            //Swap Block check
            if (DashDir.X != 0 && Input.Grab.Check)
            {
                var swapBlock = CollideFirst<SwapBlock>(Position + Vector2.UnitX * Math.Sign(DashDir.X));
                if (swapBlock != null && swapBlock.Direction.X == Math.Sign(DashDir.X))
                {
                    StateMachine.State = StClimb;
                    Speed = Vector2.Zero;
                    yield break;
                }
            }

            //Stick to Swap Blocks
            Vector2 swapCancel = Vector2.One;
            foreach (SwapBlock swapBlock in Scene.Tracker.GetEntities<SwapBlock>())
            {
                if (CollideCheck(swapBlock, Position + Vector2.UnitY))
                {
                    if (swapBlock != null && swapBlock.Swapping)
                    {
                        if (DashDir.X != 0 && swapBlock.Direction.X == Math.Sign(DashDir.X))
                            Speed.X = swapCancel.X = 0;
                        if (DashDir.Y != 0 && swapBlock.Direction.Y == Math.Sign(DashDir.Y))
                            Speed.Y = swapCancel.Y = 0;
                    }
                }
            }

            yield return DashTime;

            CreateTrail();

            AutoJump = true;
            AutoJumpTimer = 0;
            if (DashDir.Y <= 0)
            {
                Speed = DashDir * EndDashSpeed;
                Speed.X *= swapCancel.X;
                Speed.Y *= swapCancel.Y;
            }
            if (Speed.Y < 0)
                Speed.Y *= EndDashUpMult;
            StateMachine.State = StNormal;
        }

        #endregion

        #region Swim State

        private const float SwimYSpeedMult = .5f;    
        private const float SwimMaxRise = -60;
        private const float SwimVDeccel = 600f;
        private const float SwimMax = 80f;
        private const float SwimUnderwaterMax = 60f;
        private const float SwimAccel = 600f;
        private const float SwimReduce = 400f;
        private const float SwimDashSpeedMult = .75f;
        
        private bool SwimCheck()
        {
            return CollideCheck<Water>(Position + Vector2.UnitY * -8) && CollideCheck<Water>(Position);
        }

        private bool SwimUnderwaterCheck()
        {
            return CollideCheck<Water>(Position + Vector2.UnitY * -9);
        }

        private bool SwimJumpCheck()
        {
            return !CollideCheck<Water>(Position + Vector2.UnitY * -14);
        }

        private bool SwimRiseCheck()
        {
            return !CollideCheck<Water>(Position + Vector2.UnitY * -18);
        }

        private bool UnderwaterMusicCheck()
        {
            return CollideCheck<Water>(Position) && CollideCheck<Water>(Position + Vector2.UnitY * -12);
        }

        private void SwimBegin()
        {
            if (Speed.Y > 0)
                Speed.Y *= SwimYSpeedMult;

            Stamina = ClimbMaxStamina;


        }

        private int SwimUpdate()
        {
            if (!SwimCheck())
                return StNormal;

            //Never duck if possible
            if (CanUnDuck)
                Ducking = false;

            //Dashing
            if (CanDash)
            {
                //Dashes = Math.Max(0, Dashes - 1);
                Input.Dash.ConsumeBuffer();
                return StDash;
            }

            bool underwater = SwimUnderwaterCheck();

            //Climbing
            if (!underwater && Speed.Y >= 0 && Input.Grab.Check && !IsTired && CanUnDuck)
            {
                //Climbing
                if (Math.Sign(Speed.X) != -(int)Facing && ClimbCheck((int)Facing))
                {
                    if (!MoveVExact(-1))
                    {
                        Ducking = false;
                        return StClimb;
                    }
                }
            }

            //Movement
            {
                Vector2 move = Input.Aim.Value;
                move = move.SafeNormalize();

                float maxX = underwater ? SwimUnderwaterMax : SwimMax;
                float maxY = SwimMax;

                if (Math.Abs(Speed.X) > SwimMax && Math.Sign(Speed.X) == Math.Sign(move.X))
                    Speed.X = Calc.Approach(Speed.X, maxX * move.X, SwimReduce * Engine.DeltaTime);  //Reduce back from beyond the max speed
                else
                    Speed.X = Calc.Approach(Speed.X, maxX * move.X, SwimAccel * Engine.DeltaTime);   //Approach the max speed

                if (move.Y == 0 && SwimRiseCheck())
                {
                    //Rise if close to the surface and not pressing a direction
                    Speed.Y = Calc.Approach(Speed.Y, SwimMaxRise, SwimAccel * Engine.DeltaTime);
                }
                else if (move.Y >= 0 || SwimUnderwaterCheck())
                {
                    if (Math.Abs(Speed.Y) > SwimMax && Math.Sign(Speed.Y) == Math.Sign(move.Y))
                        Speed.Y = Calc.Approach(Speed.Y, maxY * move.Y, SwimReduce * Engine.DeltaTime);  //Reduce back from beyond the max speed
                    else
                        Speed.Y = Calc.Approach(Speed.Y, maxY * move.Y, SwimAccel * Engine.DeltaTime);   //Approach the max speed
                }
            }

            //Popping up onto ledge
            const int ledgePopDist = -3;
            if (!underwater && moveX != 0
                && CollideCheck<Solid>(Position + Vector2.UnitX * moveX)
                && !CollideCheck<Solid>(Position + new Vector2(moveX, ledgePopDist)))
            {
                ClimbHop();
            }

            //Jumping
            if (Input.Jump.Pressed && SwimJumpCheck())
            {
                Jump();
                return StNormal;
            }

            return StSwim;
        }

        #endregion

        #region Boost State

        private Vector2 boostTarget;
        private bool boostRed;

        public void Boost(Booster booster)
        {
            StateMachine.State = StBoost;
            Speed = Vector2.Zero;
            boostTarget = booster.Center;
            boostRed = false;
            lastBooster = CurrentBooster = booster;
        }

        public void RedBoost(Booster booster)
        {
            StateMachine.State = StBoost;
            Speed = Vector2.Zero;
            boostTarget = booster.Center;
            boostRed = true;
            lastBooster = CurrentBooster = booster;
        }

        private void BoostBegin()
        {
            RefillDash();
            RefillStamina();
        }

        private void BoostEnd()
        {
            var to = (boostTarget - Collider.Center).Floor();
            MoveToX(to.X);
            MoveToY(to.Y);
        }

        private int BoostUpdate()
        {
            Vector2 targetAdd = Input.Aim.Value * 3;
            var to = Calc.Approach(ExactPosition, boostTarget - Collider.Center + targetAdd, BoostMoveSpeed * Engine.DeltaTime);
            MoveToX(to.X);
            MoveToY(to.Y);

            if (Input.Dash.Pressed)
            {
                Input.Dash.ConsumePress();
                if (boostRed)
                    return StRedDash;
                else
                    return StDash;
            }

            return StBoost;
        }

        private IEnumerator BoostCoroutine()
        {
            yield return BoostTime;

            if (boostRed)
                StateMachine.State = StRedDash;
            else
                StateMachine.State = StDash;
        }

        #endregion

        #region Red Dash State

        private void RedDashBegin()
        {
            calledDashEvents = false;
            dashStartedOnGround = false;

            Celeste.Freeze(.05f);
            Dust.Burst(Position, Calc.Angle(-DashDir), 8);
            dashCooldownTimer = DashCooldown;
            dashRefillCooldownTimer = DashRefillCooldown;
            StartedDashing = true;

            level.Displacement.AddBurst(Center, .5f, 0, 80, .666f, Ease.QuadOut, Ease.QuadOut);
            
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            dashAttackTimer = DashAttackTime;
            Speed = Vector2.Zero;

            if (!onGround && Ducking && CanUnDuck)
                Ducking = false;
        }

        private void RedDashEnd()
        {
            CallDashEvents();
        }

        private int RedDashUpdate()
        {
            StartedDashing = false;

            if (CanDash)
                return StartDash();

            if (DashDir.Y == 0)
            {
                //JumpThru Correction
                foreach (JumpThru jt in Scene.Tracker.GetEntities<JumpThru>())
                    if (CollideCheck(jt) && Bottom - jt.Top <= DashHJumpThruNudge)
                        MoveVExact((int)(jt.Top - Bottom));

                //Super Jump
                if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0)
                {
                    SuperJump();
                    return StNormal;
                }
            }
            else if (DashDir.X == 0 && DashDir.Y == -1)
            {
                if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        SuperWallJump(-1);
                        return StNormal;
                    }
                    else if (WallJumpCheck(-1))
                    {
                        SuperWallJump(1);
                        return StNormal;
                    }
                }
            }
            else
            {
                if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        WallJump(-1);
                        return StNormal;
                    }
                    else if (WallJumpCheck(-1))
                    {
                        WallJump(1);
                        return StNormal;
                    }
                }
            }

            return StRedDash;
        }

        private IEnumerator RedDashCoroutine()
        {
            yield return null;

            Speed = lastAim * DashSpeed;
            DashDir = lastAim;
            SceneAs<Level>().DirectionalShake(DashDir, .2f);

            CallDashEvents();
        }

        #endregion

        #region Hit Squash State

        private const float HitSquashNoMoveTime = .1f;
        private const float HitSquashFriction = 800;

        private float hitSquashNoMoveTimer;

        private void HitSquashBegin()
        {
            hitSquashNoMoveTimer = HitSquashNoMoveTime;
        }

        private int HitSquashUpdate()
        {
            Speed.X = Calc.Approach(Speed.X, 0, HitSquashFriction * Engine.DeltaTime);
            Speed.Y = Calc.Approach(Speed.Y, 0, HitSquashFriction * Engine.DeltaTime);

            if (Input.Jump.Pressed)
            {
                if (onGround)
                    Jump();
                else if (WallJumpCheck(1))
                    WallJump(-1);
                else if (WallJumpCheck(-1))
                    WallJump(1);
                else
                    Input.Jump.ConsumeBuffer();

                return StNormal;
            }

            if (CanDash)
                return StartDash();

            if (Input.Grab.Check && ClimbCheck((int)Facing))
                return StClimb;

            if (hitSquashNoMoveTimer > 0)
                hitSquashNoMoveTimer -= Engine.DeltaTime;
            else
                return StNormal;

            return StHitSquash;
        }

        #endregion

        #region Launch State

        private float? launchApproachX;

        public Vector2 ExplodeLaunch(Vector2 from, bool snapUp = true)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(.1f);
            launchApproachX = null;

            Vector2 normal = (Center - from).SafeNormalize(-Vector2.UnitY);
            var dot = Vector2.Dot(normal, Vector2.UnitY);

            if (snapUp && dot <= -.7f)
            {
                //If close to up, make it up
                normal.X = 0;
                normal.Y = -1;
            }
            else if (dot <= .55f && dot >= -.55f)
            {
                //If partially down, make it sideways
                normal.Y = 0;
                normal.X = Math.Sign(normal.X);
            }

            Speed = LaunchSpeed * normal;
            if (Speed.Y <= 50f)
            {
                Speed.Y = Math.Min(-150f, Speed.Y);
                AutoJump = true;
            }

            if (Input.MoveX.Value == Math.Sign(Speed.X))
                Speed.X *= 1.2f;

            SlashFx.Burst(Center, Speed.Angle());
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            dashCooldownTimer = DashCooldown;
            StateMachine.State = StLaunch;

            return normal;
        }

        public void FinalBossPushLaunch(int dir)
        {
            launchApproachX = null;
            Speed.X = .9f * dir * LaunchSpeed;
            Speed.Y = -150;
            AutoJump = true;

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SlashFx.Burst(Center, Speed.Angle());
            RefillDash();
            RefillStamina();
            dashCooldownTimer = .28f;
            StateMachine.State = StLaunch;
        }

        public void BadelineBoostLaunch(float atX)
        {
            launchApproachX = atX;
            Speed.X = 0;
            Speed.Y = -330f;
            AutoJump = true;

            SlashFx.Burst(Center, Speed.Angle());
            RefillDash();
            RefillStamina();
            dashCooldownTimer = DashCooldown;
            StateMachine.State = StLaunch;
        }

        private void LaunchBegin()
        {
            launched = true;
        }

        private int LaunchUpdate()
        {
            //Approach X
            if (launchApproachX.HasValue)
                MoveTowardsX(launchApproachX.Value, 60 * Engine.DeltaTime);

            //Dashing
            if (CanDash)
                return StartDash();

            //Decceleration
            if (Speed.Y < 0)
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * .5f * Engine.DeltaTime);
            else
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * .25f * Engine.DeltaTime);
            Speed.X = Calc.Approach(Speed.X, 0, RunAccel * .2f * Engine.DeltaTime);

            if (Speed.Length() < LaunchCancelThreshold)
                return StNormal;
            else
                return StLaunch;
        }

        #endregion

        #region Summit Launch State

        private float summitLaunchTargetX;
        private float summitLaunchParticleTimer;

        public void SummitLaunch(float targetX)
        {
            summitLaunchTargetX = targetX;

            StateMachine.State = StSummitLaunch;
        }

        private void SummitLaunchBegin()
        {
            wallBoostTimer = 0;
            Sprite.Play("launch");
            Speed = -Vector2.UnitY * DashSpeed;
            summitLaunchParticleTimer = .4f;
        }

        private int SummitLaunchUpdate()
        {
            summitLaunchParticleTimer -= Engine.DeltaTime;
            if (summitLaunchParticleTimer > 0 && Scene.OnInterval(.03f))
                level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, Center, Vector2.One * 4);

            Facing = Facings.Right;
            MoveTowardsX(summitLaunchTargetX, 20 * Engine.DeltaTime);
            Speed = -Vector2.UnitY * DashSpeed;
            if (level.OnInterval(0.2f))
                level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, Calc.Down, Color.White));
            return StSummitLaunch;
        }

        public void StopSummitLaunch()
        {
            StateMachine.State = StNormal;
            Speed.Y = BounceSpeed;
            AutoJump = true;
            varJumpSpeed = Speed.Y;
        }

        #endregion

        #region Pickup State

        private IEnumerator PickupCoroutine()
        {
            Play(Sfxs.char_mad_crystaltheo_lift);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            Vector2 oldSpeed = Speed;
            float varJump = varJumpTimer;
            Speed = Vector2.Zero;

            Vector2 begin = Holding.Entity.Position - Position;
            Vector2 end = CarryOffsetTarget;
            Vector2 control = new Vector2(begin.X + Math.Sign(begin.X) * 2, CarryOffsetTarget.Y - 2);
            SimpleCurve curve = new SimpleCurve(begin, end, control);

            carryOffset = begin;
            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, .16f, true);
            tween.OnUpdate = (t) =>
            {
                carryOffset = curve.GetPoint(t.Eased);
            };
            Add(tween);
            yield return tween.Wait();

            Speed = oldSpeed;
            Speed.Y = Math.Min(Speed.Y, 0);
            varJumpTimer = varJump;
            StateMachine.State = StNormal;
        }

        #endregion

        #region Dream Dash State

        private DreamBlock dreamBlock;
        private SoundSource dreamSfxLoop;
        private bool dreamJump;

        private void DreamDashBegin()
        {
            if (dreamSfxLoop == null)
                Add(dreamSfxLoop = new SoundSource());

            Speed = DashDir * DreamDashSpeed;
            TreatNaive = true;
            Depth = Depths.PlayerDreamDashing;
            dreamDashCanEndTimer = DreamDashMinTime;
            Stamina = ClimbMaxStamina;
            dreamJump = false;

            Play(Sfxs.char_mad_dreamblock_enter);
            Loop(dreamSfxLoop, Sfxs.char_mad_dreamblock_travel);
        }

        private void DreamDashEnd()
        {
            Depth = Depths.Player;
            if (!dreamJump)
            {
                AutoJump = true;
                AutoJumpTimer = 0;
            }
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            TreatNaive = false;

            if (dreamBlock != null)
            {
                if (DashDir.X != 0)
                {
                    jumpGraceTimer = JumpGraceTime;
                    dreamJump = true;
                }
                else
                    jumpGraceTimer = 0;

                dreamBlock.OnPlayerExit(this);
                dreamBlock = null;
            }

            Stop(dreamSfxLoop);
            Play(Sfxs.char_mad_dreamblock_exit);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
        }

        private int DreamDashUpdate()
        {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

            var oldPos = Position;
            NaiveMove(Speed * Engine.DeltaTime);
            if (dreamDashCanEndTimer > 0)
                dreamDashCanEndTimer -= Engine.DeltaTime;

            var block = CollideFirst<DreamBlock>();          
            if (block == null)
            {
                if (DreamDashedIntoSolid())
                {
                    if (SaveData.Instance.AssistMode && SaveData.Instance.Assists.Invincible)
                    {
                        Position = oldPos;
                        Speed *= -1;
                        Play(Sfxs.game_assist_dreamblockbounce);
                    }
                    else
                        Die(Vector2.Zero);
                }
                else if (dreamDashCanEndTimer <= 0)
                {
                    Celeste.Freeze(.05f);

                    if (Input.Jump.Pressed && DashDir.X != 0)
                    {
                        dreamJump = true;
                        Jump();
                    }
                    else
                    {
                        bool left = ClimbCheck(-1);
                        bool right = ClimbCheck(1);

                        if (Input.Grab.Check && (DashDir.Y >= 0 || DashDir.X != 0) && ((moveX == 1 && right) || (moveX == -1 && left)))
                        {
                            Facing = (Facings)moveX;
                            return StClimb;
                        }
                    }

                    return StNormal;
                }
            }
            else
            {
                dreamBlock = block;

                if (Scene.OnInterval(0.1f))
                    CreateTrail();

                //Displacement effect
                if (level.OnInterval(0.04f))
                {
                    var displacement = level.Displacement.AddBurst(Center, .3f, 0f, 40f);
                    displacement.WorldClipCollider = dreamBlock.Collider;
                    displacement.WorldClipPadding = 2;
                }
            }

            return StDreamDash;
        }

        private bool DreamDashedIntoSolid()
        {
            if (CollideCheck<Solid>())
            {
                for (int x = 1; x <= DreamDashEndWiggle; x++)
                {
                    for (int xm = -1; xm <= 1; xm += 2)
                    {
                        for (int y = 1; y <= DreamDashEndWiggle; y++)
                        {
                            for (int ym = -1; ym <= 1; ym += 2)
                            {
                                Vector2 add = new Vector2(x * xm, y * ym);
                                if (!CollideCheck<Solid>(Position + add))
                                {
                                    Position += add;
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            else
                return false;
        }

        #endregion

        #region Star Fly State

        private const float StarFlyTransformDeccel = 1000f;
        private const float StarFlyTime = 2f;
        private const float StarFlyStartSpeed = 250f;
        private const float StarFlyTargetSpeed = 140f;
        private const float StarFlyMaxSpeed = 190f;
        private const float StarFlyMaxLerpTime = 1f;
        private const float StarFlySlowSpeed = StarFlyTargetSpeed * .65f;
        private const float StarFlyAccel = 1000f;
        private const float StarFlyRotateSpeed = 320 * Calc.DtR;
        private const float StarFlyEndX = 160f;
        private const float StarFlyEndXVarJumpTime = .1f;
        private const float StarFlyEndFlashDuration = 0.5f;
        private const float StarFlyEndNoBounceTime = .2f;
        private const float StarFlyWallBounce = -.5f;
        private const float StarFlyMaxExitY = 0f;
        private const float StarFlyMaxExitX = 140;
        private const float StarFlyExitUp = -100;

        private Color starFlyColor = Calc.HexToColor("ffd65c");
        private BloomPoint starFlyBloom;

        private float starFlyTimer;
        private bool starFlyTransforming;
        private float starFlySpeedLerp;
        private Vector2 starFlyLastDir;

        private SoundSource starFlyLoopSfx;
        private SoundSource starFlyWarningSfx;

        public bool StartStarFly()
        {
            RefillStamina();

            if (StateMachine.State == StReflectionFall)
                return false;

            if (StateMachine.State == StStarFly)
            {
                starFlyTimer = StarFlyTime;
                Sprite.Color = starFlyColor;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
                StateMachine.State = StStarFly;

            return true;
        }

        private void StarFlyBegin()
        {
            Sprite.Play(PlayerSprite.StartStarFly);

            starFlyTransforming = true;
            starFlyTimer = StarFlyTime;
            starFlySpeedLerp = 0;
            jumpGraceTimer = 0;

            if (starFlyBloom == null)
                Add(starFlyBloom = new BloomPoint(new Vector2(0, -6), 0f, 16));
            starFlyBloom.Visible = true;
            starFlyBloom.Alpha = 0f;

            Collider = starFlyHitbox;
            hurtbox = starFlyHurtbox;

            if (starFlyLoopSfx == null)
            {
                Add(starFlyLoopSfx = new SoundSource());
                starFlyLoopSfx.DisposeOnTransition = false;
                Add(starFlyWarningSfx = new SoundSource());
                starFlyWarningSfx.DisposeOnTransition = false;
            }
            starFlyLoopSfx.Play(Sfxs.game_06_feather_state_loop, "feather_speed", 1);
            starFlyWarningSfx.Stop();
        }
        
        private void StarFlyEnd()
        {
            Play(Sfxs.game_06_feather_state_end);

            starFlyWarningSfx.Stop();
            starFlyLoopSfx.Stop();
            Hair.DrawPlayerSpriteOutline = false;
            Sprite.Color = Color.White;
            level.Displacement.AddBurst(Center, 0.25f, 8, 32);
            starFlyBloom.Visible = false;
            Sprite.HairCount = startHairCount;

            StarFlyReturnToNormalHitbox();

            if (StateMachine.State != StDash)
                level.Particles.Emit(FlyFeather.P_Boost, 12, Center, Vector2.One * 4, (-Speed).Angle());
        }

        private void StarFlyReturnToNormalHitbox()
        {
            Collider = normalHitbox;
            hurtbox = normalHurtbox;

            if (CollideCheck<Solid>())
            {
                Vector2 start = Position;

                //Try moving up
                Y -= (normalHitbox.Bottom - starFlyHitbox.Bottom);
                if (CollideCheck<Solid>())
                    Position = start;
                else
                    return;

                //Try ducking and moving up
                Ducking = true;
                Y -= (duckHitbox.Bottom - starFlyHitbox.Bottom);
                if (CollideCheck<Solid>())
                    Position = start;
                else
                    return;

                throw new Exception("Could not get out of solids when exiting Star Fly State!");
            }
        }

        private IEnumerator StarFlyCoroutine()
        {
            while (Sprite.CurrentAnimationID == PlayerSprite.StartStarFly)
                yield return null;

            while (Speed != Vector2.Zero)
                yield return null;

            yield return .1f;

            Sprite.Color = starFlyColor;
            Sprite.HairCount = 7;
            Hair.DrawPlayerSpriteOutline = true;

            level.Displacement.AddBurst(Center, 0.25f, 8, 32);
            starFlyTransforming = false;
            starFlyTimer = StarFlyTime;

            RefillDash();
            RefillStamina();

            //Speed boost
            {
                var dir = Input.Aim.Value;
                if (dir == Vector2.Zero)
                    dir = Vector2.UnitX * (int)Facing;
                Speed = dir * StarFlyStartSpeed;
                starFlyLastDir = dir;

                level.Particles.Emit(FlyFeather.P_Boost, 12, Center, Vector2.One * 4, (-dir).Angle());
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(starFlyLastDir);

            while (starFlyTimer > StarFlyEndFlashDuration)
                yield return null;

            starFlyWarningSfx.Play(Sfxs.game_06_feather_state_warning);
        }

        private int StarFlyUpdate()
        {
            starFlyBloom.Alpha = Calc.Approach(starFlyBloom.Alpha, 0.7f, Engine.DeltaTime * 2f);

            if (starFlyTransforming)
            {
                Speed = Calc.Approach(Speed, Vector2.Zero, StarFlyTransformDeccel * Engine.DeltaTime);
            }
            else
            {
                //Movement
                {
                    Vector2 aim = Input.Aim.Value;
                    bool slow = false;
                    if (aim == Vector2.Zero)
                    {
                        slow = true;
                        aim = starFlyLastDir;
                    }

                    //Figure out direction
                    Vector2 currentDir = Speed.SafeNormalize(Vector2.Zero);
                    if (currentDir == Vector2.Zero)
                        currentDir = aim;
                    else
                        currentDir = currentDir.RotateTowards(aim.Angle(), StarFlyRotateSpeed * Engine.DeltaTime);
                    starFlyLastDir = currentDir;

                    //Figure out max speed
                    float maxSpeed;
                    if (slow)
                    {
                        starFlySpeedLerp = 0;
                        maxSpeed = StarFlySlowSpeed;
                    }
                    else if (currentDir != Vector2.Zero && Vector2.Dot(currentDir, aim) >= .45f)
                    {
                        starFlySpeedLerp = Calc.Approach(starFlySpeedLerp, 1, Engine.DeltaTime / StarFlyMaxLerpTime);
                        maxSpeed = MathHelper.Lerp(StarFlyTargetSpeed, StarFlyMaxSpeed, starFlySpeedLerp);
                    }
                    else
                    {
                        starFlySpeedLerp = 0;
                        maxSpeed = StarFlyTargetSpeed;
                    }

                    starFlyLoopSfx.Param("feather_speed", slow ? 0 : 1);

                    //Approach max speed
                    float currentSpeed = Speed.Length();
                    currentSpeed = Calc.Approach(currentSpeed, maxSpeed, StarFlyAccel * Engine.DeltaTime);

                    //Set speed
                    Speed = currentDir * currentSpeed;

                    //Particles
                    if (level.OnInterval(.02f))
                        level.Particles.Emit(FlyFeather.P_Flying, 1, Center, Vector2.One * 2, (-Speed).Angle());
                }

                //Jump cancelling
                if (Input.Jump.Pressed)
                {
                    if (OnGround(3))
                    {
                        Jump();
                        return StNormal;
                    }
                    else if (WallJumpCheck(-1))
                    {
                        WallJump(1);
                        return StNormal;
                    }
                    else if (WallJumpCheck(1))
                    {
                        WallJump(-1);
                        return StNormal;
                    }
                }

                //Grab cancelling
                if (Input.Grab.Check)
                {
                    if (Input.MoveX.Value != -1 && ClimbCheck(1))
                    {
                        Facing = Facings.Right;
                        return StClimb;
                    }
                    else if (Input.MoveX.Value != 1 && ClimbCheck(-1))
                    {
                        Facing = Facings.Left;
                        return StClimb;
                    }
                }

                //Dash cancelling
                if (CanDash)
                    return StartDash();

                //Timer
                starFlyTimer -= Engine.DeltaTime;
                if (starFlyTimer <= 0)
                {
                    if (Input.MoveY.Value == -1)
                        Speed.Y = StarFlyExitUp;

                    if (Input.MoveY.Value < 1)
                    {
                        varJumpSpeed = Speed.Y;
                        AutoJump = true;
                        AutoJumpTimer = 0;
                        varJumpTimer = VarJumpTime;
                    }

                    if (Speed.Y > StarFlyMaxExitY)
                        Speed.Y = StarFlyMaxExitY;

                    if (Math.Abs(Speed.X) > StarFlyMaxExitX)
                        Speed.X = StarFlyMaxExitX * Math.Sign(Speed.X);

                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                    return StNormal;
                }

                // Flicker at end
                if (starFlyTimer < StarFlyEndFlashDuration && Scene.OnInterval(0.05f))
                {
                    if (Sprite.Color == starFlyColor)
                        Sprite.Color = NormalHairColor;
                    else
                        Sprite.Color = starFlyColor;
                }
            }

            return StStarFly;
        }

        #endregion

        #region Cassette Fly State

        private SimpleCurve cassetteFlyCurve;
        private float cassetteFlyLerp;

        public void StartCassetteFly(Vector2 targetPosition, Vector2 control)
        {
            StateMachine.State = StCassetteFly;
            cassetteFlyCurve = new SimpleCurve(Position, targetPosition, control);
            cassetteFlyLerp = 0;
            Speed = Vector2.Zero;
        }

        private void CassetteFlyBegin()
        {
            Sprite.Play("bubble");
            Sprite.Y += 5;
        }

        private void CassetteFlyEnd()
        {

        }

        private int CassetteFlyUpdate()
        {
            return StCassetteFly;
        }

        private IEnumerator CassetteFlyCoroutine()
        {
            level.CanRetry = false;
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 0.5f;
            Sprite.Scale = Vector2.One * 1.25f;
            Depth = Depths.FormationSequences;
            yield return 0.4f;

            while (cassetteFlyLerp < 1f)
            {
                if (level.OnInterval(.03f))
                    level.Particles.Emit(P_CassetteFly, 2, Center, Vector2.One * 4);

                cassetteFlyLerp = Calc.Approach(cassetteFlyLerp, 1f, 1.6f * Engine.DeltaTime);
                Position = cassetteFlyCurve.GetPoint(Ease.SineInOut(cassetteFlyLerp));
                level.Camera.Position = CameraTarget;
                yield return null;
            }

            Position = cassetteFlyCurve.End;
            Sprite.Scale = Vector2.One * 1.25f;
            Sprite.Y -= 5;
            Sprite.Play(PlayerSprite.FallFast);
            yield return 0.2f;

            level.CanRetry = true;
            level.FormationBackdrop.Display = false;
            level.FormationBackdrop.Alpha = 0.5f;
            StateMachine.State = StNormal;
            Depth = Depths.Player;
            yield break;
        }

        #endregion

        #region Attract State

        private Vector2 attractTo;

        public void StartAttract(Vector2 attractTo)
        {
            this.attractTo = Calc.Round(attractTo);
            StateMachine.State = StAttract;
        }

        private void AttractBegin()
        {
            Speed = Vector2.Zero;
        }

        private void AttractEnd()
        {
 
        }

        private int AttractUpdate()
        {
            if (Vector2.Distance(attractTo, ExactPosition) <= 1.5f)
            {
                Position = attractTo;
                ZeroRemainderX();
                ZeroRemainderY();
            }
            else
            {
                Vector2 at = Calc.Approach(ExactPosition, attractTo, 200 * Engine.DeltaTime);
                MoveToX(at.X);
                MoveToY(at.Y);
            }

            return StAttract;
        }

        public bool AtAttractTarget
        {
            get
            {
                return StateMachine.State == StAttract && ExactPosition == attractTo;
            }
        }

        #endregion

        #region Dummy State

        public bool DummyMoving = false;
        public bool DummyGravity = true;
        public bool DummyFriction = true;
        public bool DummyMaxspeed = true;

        private void DummyBegin()
        {
            DummyMoving = false;
            DummyGravity = true;
            DummyAutoAnimate = true;
        }

        private int DummyUpdate()
        {
            if (CanUnDuck)
                Ducking = false;

            // gravity
            if (!onGround && DummyGravity)
            {
                float mult = (Math.Abs(Speed.Y) < HalfGravThreshold && (Input.Jump.Check || AutoJump)) ? .5f : 1f;

                if (level.InSpace)
                    mult *= SpacePhysicsMult;

                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * mult * Engine.DeltaTime);
            }

            // variable jumping
            if (varJumpTimer > 0)
            {
                if (AutoJump || Input.Jump.Check)
                    Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
                else
                    varJumpTimer = 0;
            }

            if (!DummyMoving)
            {
                if (Math.Abs(Speed.X) > MaxRun && DummyMaxspeed)
                    Speed.X = Calc.Approach(Speed.X, MaxRun * Math.Sign(Speed.X), RunAccel * 2.5f * Engine.DeltaTime);
                if (DummyFriction)
                    Speed.X = Calc.Approach(Speed.X, 0, RunAccel * Engine.DeltaTime);
            }

            //Sprite
            if (DummyAutoAnimate)
            {
                if (onGround)
                {
                    if (Speed.X == 0)
                        Sprite.Play("idle");
                    else
                        Sprite.Play(PlayerSprite.Walk);
                }
                else
                {
                    if (Speed.Y < 0)
                        Sprite.Play(PlayerSprite.JumpSlow);
                    else
                        Sprite.Play(PlayerSprite.FallSlow);
                }
                    
            }

            return StDummy;
        }

        public IEnumerator DummyWalkTo(float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false)
        {
            StateMachine.State = StDummy;

            if (Math.Abs(X - x) > 4 && !Dead)
            {
                DummyMoving = true;

                if (walkBackwards)
                {
                    Sprite.Rate = -1;
                    Facing = (Facings)Math.Sign(X - x);
                }
                else
                {
                    Facing = (Facings)Math.Sign(x - X);
                }

                while (Math.Abs(x - X) > 4 && Scene != null && (keepWalkingIntoWalls || !CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(x - X))))
                {
                    Speed.X = Calc.Approach(Speed.X, Math.Sign(x - X) * WalkSpeed * speedMultiplier, RunAccel * Engine.DeltaTime);
                    yield return null;
                }

                Sprite.Rate = 1;
                Sprite.Play(PlayerSprite.Idle);
                DummyMoving = false;
            }
        }

        public IEnumerator DummyWalkToExact(int x, bool walkBackwards = false, float speedMultiplier = 1f)
        {
            StateMachine.State = StDummy;

            if (X != x)
            {
                DummyMoving = true;

                if (walkBackwards)
                {
                    Sprite.Rate = -1;
                    Facing = (Facings)Math.Sign(X - x);
                }
                else
                {
                    Facing = (Facings)Math.Sign(x - X);
                }

                var last = Math.Sign(X - x);
                while (X != x && !CollideCheck<Solid>(Position + new Vector2((int)Facing, 0)))
                {
                    Speed.X = Calc.Approach(Speed.X, Math.Sign(x - X) * WalkSpeed * speedMultiplier, RunAccel * Engine.DeltaTime);

                    // handle case where we overstep
                    var next = Math.Sign(X - x);
                    if (next != last)
                    {
                        X = x;
                        break;
                    }
                    last = next;

                    yield return null;
                }

                Speed.X = 0;
                Sprite.Rate = 1;
                Sprite.Play(PlayerSprite.Idle);
                DummyMoving = false;
            }
        }

        public IEnumerator DummyRunTo(float x, bool fastAnim = false)
        {
            StateMachine.State = StDummy;
            
            if (Math.Abs(X - x) > 4)
            {
                DummyMoving = true;
                if (fastAnim)
                    Sprite.Play(PlayerSprite.RunFast);
                else if (!Sprite.LastAnimationID.StartsWith("run"))
                    Sprite.Play(PlayerSprite.RunSlow);
                Facing = (Facings)Math.Sign(x - X);

                while (Math.Abs(X - x) > 4)
                {
                    Speed.X = Calc.Approach(Speed.X, Math.Sign(x - X) * MaxRun, RunAccel * Engine.DeltaTime);
                    yield return null;
                }

                Sprite.Play(PlayerSprite.Idle);
                DummyMoving = false;
            }
        }

        #endregion

        #region Frozen State
        
        private int FrozenUpdate()
        {
            return StFrozen;
        }

        #endregion

        #region Temple Fall State
        
        private int TempleFallUpdate()
        {
            Facing = Facings.Right;

            if (!onGround)
            {
                var center = level.Bounds.Left + 160;
                int mX;
                if (Math.Abs(center - X) > 4)
                    mX = Math.Sign(center - X);
                else
                    mX = 0;

                Speed.X = Calc.Approach(Speed.X, MaxRun * .6f * mX, RunAccel * .5f * AirMult * Engine.DeltaTime);
            }
            if (!onGround && DummyGravity)
                Speed.Y = Calc.Approach(Speed.Y, MaxFall * 2, Gravity * 0.25f * Engine.DeltaTime);

            return StTempleFall;
        }

        private IEnumerator TempleFallCoroutine()
        {
            Sprite.Play(PlayerSprite.FallFast);

            while (!onGround)
                yield return null;

            Play(Sfxs.char_mad_mirrortemple_landing);
            if (Dashes <= 1)
                Sprite.Play(PlayerSprite.LandInPose);
            else
                Sprite.Play(PlayerSprite.Idle);
            Sprite.Scale.Y = 0.7f;

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(new Vector2(0, 1f), 0.5f);
            Speed.X = 0;

            level.Particles.Emit(P_SummitLandA, 12, BottomCenter, Vector2.UnitX * 3, Calc.Up);
            level.Particles.Emit(P_SummitLandB, 8, BottomCenter - Vector2.UnitX * 2, Vector2.UnitX * 2, Calc.Left + 15 * Calc.DtR);
            level.Particles.Emit(P_SummitLandB, 8, BottomCenter + Vector2.UnitX * 2, Vector2.UnitX * 2, Calc.Right - 15 * Calc.DtR);

            for (var p = 0f; p < 1; p += Engine.DeltaTime)
                yield return null;

            StateMachine.State = StNormal;
        }

        #endregion

        #region Reflection Fall State

        private void ReflectionFallBegin()
        {
            IgnoreJumpThrus = true;
        }

        private void ReflectionFallEnd()
        {
            FallEffects.Show(false);
            IgnoreJumpThrus = false;
        }

        private int ReflectionFallUpdate()
        {
            Facing = Facings.Right;

            if (Scene.OnInterval(.05f))
            {
                wasDashB = true;
                CreateTrail();
            }

            // fall
            if (CollideCheck<Water>())
                Speed.Y = Calc.Approach(Speed.Y, -20f, 400f * Engine.DeltaTime);
            else
                Speed.Y = Calc.Approach(Speed.Y, MaxFall * 2, Gravity * 0.25f * Engine.DeltaTime);

            // remove feathers
            var feathers = Scene.Tracker.GetEntities<FlyFeather>();
            foreach (var feather in feathers)
                feather.RemoveSelf();

            // smash crystal spinners
            var hit = Scene.CollideFirst<CrystalStaticSpinner>(new Rectangle((int)(X - 6), (int)(Y - 6), 12, 12));
            if (hit != null)
            {
                hit.Destroy();
                level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Celeste.Freeze(0.01f);
            }

            return StReflectionFall;
        }

        private IEnumerator ReflectionFallCoroutine()
        {
            Sprite.Play(PlayerSprite.FallBig);
            level.StartCutscene(OnReflectionFallSkip);

            // wait a bit before entering
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                Speed.Y = 0f;
                yield return null;
            }

            // start falling at max speed
            FallEffects.Show(true);
            Speed.Y = MaxFall * 2f;

            // wait for waiter
            while (!CollideCheck<Water>())
                yield return null;

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            FallEffects.Show(false);
            Sprite.Play("bigFallRecover");
            level.Session.Audio.Music.Event = Sfxs.music_reflection_main;
            level.Session.Audio.Apply();
            level.EndCutscene();

            yield return 1.2f;

            StateMachine.State = StNormal;
        }

        private void OnReflectionFallSkip(Level level)
        {
            level.OnEndOfFrame += () =>
            {
                level.Remove(this);
                level.UnloadLevel();
                level.Session.Level = "00";
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom));
                level.LoadLevel(IntroTypes.None);
                
                FallEffects.Show(false);

                level.Session.Audio.Music.Event = Sfxs.music_reflection_main;
                level.Session.Audio.Apply();
            };
        }

        #endregion

        #region Intro Walk State

        private Facings IntroWalkDirection;

        public IEnumerator IntroWalkCoroutine()
        {
            var start = Position;
            if (IntroWalkDirection == Facings.Right)
            {
                X = level.Bounds.Left - 16;
                Facing = Facings.Right;
            }
            else
            {
                X = level.Bounds.Right + 16;
                Facing = Facings.Left;
            }

            yield return .3f;

            Sprite.Play(PlayerSprite.RunSlow);
            while (Math.Abs(X - start.X) > 2 && !CollideCheck<Solid>(Position + new Vector2((int)Facing, 0)))
            {
                MoveTowardsX(start.X, 64 * Engine.DeltaTime);
                yield return null;
            }

            Position = start;
            Sprite.Play(PlayerSprite.Idle);
            yield return .2f;

            StateMachine.State = StNormal;
        }

        #endregion

        #region Intro Jump State

        private IEnumerator IntroJumpCoroutine()
        {
            var start = Position;
            var wasSummitJump = StateMachine.PreviousState == StSummitLaunch;

            Depth = Depths.Top;
            Facing = Facings.Right;

            if (!wasSummitJump)
            {
                Y = level.Bounds.Bottom + 16;
                yield return .5f;
            }
            else
            {
                start.Y = level.Bounds.Bottom - 24;
                MoveToX((int)Math.Round(X / 8) * 8);
            }

            // move up
            {
                if (!wasSummitJump)
                    Sprite.Play(PlayerSprite.JumpSlow);
                while (Y > start.Y - 8)
                {
                    Y += -120 * Engine.DeltaTime;
                    yield return null;
                }
                Speed.Y = -100;
            }

            // slow down
            {
                while (Speed.Y < 0)
                {
                    Speed.Y += Engine.DeltaTime * 800f;
                    yield return null;
                }
                
                Speed.Y = 0;
                if (wasSummitJump)
                {
                    yield return 0.2f;
                    Play(Sfxs.char_mad_summit_areastart);
                    Sprite.Play("launchRecover");
                    yield return 0.1f;
                }
                else
                    yield return 0.1f;
            }

            // fall down
            {
                if (!wasSummitJump)
                    Sprite.Play(PlayerSprite.FallSlow);
                
                while (!onGround)
                {
                    Speed.Y += Engine.DeltaTime * 800f;
                    yield return null;
                }
            }

            // land
            {
                if (StateMachine.PreviousState != StSummitLaunch)
                    Position = start;

                Depth = Depths.Player;
                level.DirectionalShake(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                
            }

            if (wasSummitJump)
            {
                level.Particles.Emit(P_SummitLandA, 12, BottomCenter, Vector2.UnitX * 3, Calc.Up);
                level.Particles.Emit(P_SummitLandB, 8, BottomCenter - Vector2.UnitX * 2, Vector2.UnitX * 2, Calc.Left + 15 * Calc.DtR);
                level.Particles.Emit(P_SummitLandB, 8, BottomCenter + Vector2.UnitX * 2, Vector2.UnitX * 2, Calc.Right - 15 * Calc.DtR);
                level.ParticlesBG.Emit(P_SummitLandC, 30, BottomCenter, Vector2.UnitX * 5);

                yield return 0.35f;
                for (int i = 0; i < Hair.Nodes.Count; i++)
                    Hair.Nodes[i] = new Vector2(0, 2 + i);
            }

            StateMachine.State = StNormal;
        }

        #endregion

        #region Intro Wake Up State

        private IEnumerator IntroWakeUpCoroutine()
        {
            Sprite.Play("asleep");

            yield return .5f;
            yield return Sprite.PlayRoutine("wakeUp");
            yield return .2f;

            StateMachine.State = StNormal;
        }

        #endregion

        #region Intro Respawn State

        private Tween respawnTween;

        private void IntroRespawnBegin()
        {
            Play(Sfxs.char_mad_revive);

            Depth = Depths.Top;
            introEase = 1f;

            Vector2 from = Position;
            const float pad = 40;
            from.X = MathHelper.Clamp(from.X, level.Bounds.Left + pad, level.Bounds.Right - pad);
            from.Y = MathHelper.Clamp(from.Y, level.Bounds.Top + pad, level.Bounds.Bottom - pad);
            deadOffset = from;
            from = from - Position;

            respawnTween = Tween.Create(Tween.TweenMode.Oneshot, null, .6f, true);
            respawnTween.OnUpdate = (t) =>
            {
                deadOffset = Vector2.Lerp(from, Vector2.Zero, t.Eased);
                introEase = 1 - t.Eased; 
            };
            respawnTween.OnComplete = (t) =>
            {
                if (StateMachine.State == StIntroRespawn)
                {
                    StateMachine.State = StNormal;
                    Sprite.Scale = new Vector2(1.5f, .5f);
                }
            };
            Add(respawnTween);
        }

        private void IntroRespawnEnd()
        {
            Depth = Depths.Player;
            deadOffset = Vector2.Zero;
            Remove(respawnTween);
            respawnTween = null;
        }

        #endregion

        #region Bird Dash Tutorial

        private void BirdDashTutorialBegin()
        {
            DashBegin();
            Play(Sfxs.char_mad_dash_red_right);
            Sprite.Play(PlayerSprite.Dash);
        }

        private int BirdDashTutorialUpdate()
        {
            return StBirdDashTutorial;
        }

        private IEnumerator BirdDashTutorialCoroutine()
        {
            yield return null;

            CreateTrail();
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, CreateTrail, 0.08f, true));
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, CreateTrail, DashTime, true));

            var aim = new Vector2(1, -1).SafeNormalize();
            Facing = Facings.Right;

            Speed = aim * DashSpeed;
            DashDir = aim;
            SceneAs<Level>().DirectionalShake(DashDir, .2f);
            SlashFx.Burst(Center, DashDir.Angle());

            for (float time = 0; time < DashTime; time += Engine.DeltaTime)
            {
                if (Speed != Vector2.Zero && level.OnInterval(0.02f))
                    level.ParticlesFG.Emit(P_DashA, Center + Calc.Random.Range(Vector2.One * -2, Vector2.One * 2), DashDir.Angle());
                yield return null;
            }

            AutoJump = true;
            AutoJumpTimer = 0;
            if (DashDir.Y <= 0)
                Speed = DashDir * EndDashSpeed;
            if (Speed.Y < 0)
                Speed.Y *= EndDashUpMult;

            Sprite.Play(PlayerSprite.FallFast);

            var climbing = false;
            while (!OnGround() && !climbing)
            {
                Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * Engine.DeltaTime);
                if (CollideCheck<Solid>(Position + new Vector2(1, 0)))
                    climbing = true;
                if (Top > level.Bounds.Bottom)
                {
                    level.CancelCutscene();
                    Die(Vector2.Zero);
                }
                yield return null;
            }

            if (climbing)
            {
                Sprite.Play(PlayerSprite.WallSlide);
                Dust.Burst(Position + new Vector2(4, -6), Calc.Angle(new Vector2(-4, 0)), 1);
                Speed.Y = 0;
                yield return 0.2f;
                
                Sprite.Play(PlayerSprite.ClimbUp);
                while (CollideCheck<Solid>(Position + new Vector2(1, 0)))
                {
                    Y += ClimbUpSpeed * Engine.DeltaTime;
                    yield return null;
                }
                
                Play(Sfxs.char_mad_climb_ledge);
                Sprite.Play(PlayerSprite.JumpFast);
                Speed.Y = JumpSpeed;

                while (!OnGround())
                {
                    Speed.Y = Calc.Approach(Speed.Y, MaxFall, Gravity * Engine.DeltaTime);
                    Speed.X = 20f;
                    yield return null;
                }

                Speed.X = 0;
                Speed.Y = 0;

                Sprite.Play(PlayerSprite.Walk);
                for (float time = 0f; time < 0.5f; time += Engine.DeltaTime)
                {
                    X += 32 * Engine.DeltaTime;
                    yield return null;
                }
                
                Sprite.Play(PlayerSprite.Tired);
            }
            else
            {
                Sprite.Play(PlayerSprite.Tired);
                Speed.Y = 0;

                while (Speed.X != 0)
                {
                    Speed.X = Calc.Approach(Speed.X, 0, 240 * Engine.DeltaTime);
                    if (Scene.OnInterval(0.04f))
                        Dust.Burst(BottomCenter + new Vector2(0, -2), Calc.UpLeft);
                    yield return null;
                }
            }
        }

        #endregion

        #region Chaser State Tracking

        public FMOD.Studio.EventInstance Play(string sound, string param = null, float value = 0)
        {
            AddChaserStateSound(sound, param, value);
            return Audio.Play(sound, Center, param, value);
        }

        public void Loop(SoundSource sfx, string sound)
        {
            AddChaserStateSound(sound, null, 0, ChaserStateSound.Actions.Loop);
            sfx.Play(sound);
        }

        public void Stop(SoundSource sfx)
        {
            if (sfx.Playing)
            {
                AddChaserStateSound(sfx.EventName, null, 0, ChaserStateSound.Actions.Stop);
                sfx.Stop();
            }
        }

        private void AddChaserStateSound(string sound, ChaserStateSound.Actions action)
        {
            AddChaserStateSound(sound, null, 0, action);
        }

        private void AddChaserStateSound(string sound, string param = null, float value = 0, ChaserStateSound.Actions action = ChaserStateSound.Actions.Oneshot)
        {
            string eventName = null;
            Sfxs.MadelineToBadelineSound.TryGetValue(sound, out eventName);

            if (eventName != null)
                activeSounds.Add(new ChaserStateSound() { Event = eventName, Parameter = param, ParameterValue = value, Action = action });
        }

        public struct ChaserStateSound
        {
            public enum Actions
            {
                Oneshot,
                Loop,
                Stop
            }

            public string Event;
            public string Parameter;
            public float ParameterValue;
            public Actions Action;
        }

        public struct ChaserState
        {
            public Vector2 Position;
            public float TimeStamp;
            public string Animation;
            public Facings Facing;
            public bool OnGround;
            public Color HairColor;
            public int Depth;

            private ChaserStateSound sound0;
            private ChaserStateSound sound1;
            private ChaserStateSound sound2;
            private ChaserStateSound sound3;
            private ChaserStateSound sound4;
            public int Sounds;

            public ChaserState(Player player)
            {
                Position = player.Position;
                TimeStamp = player.Scene.TimeActive;
                Animation = player.Sprite.CurrentAnimationID;
                Facing = player.Facing;
                OnGround = player.onGround;
                HairColor = player.Hair.Color;
                Depth = player.Depth;

                var sounds = player.activeSounds;
                Sounds = Math.Min(5, sounds.Count);

                sound0 = Sounds > 0 ? sounds[0] : default(ChaserStateSound);
                sound1 = Sounds > 1 ? sounds[1] : default(ChaserStateSound);
                sound2 = Sounds > 2 ? sounds[2] : default(ChaserStateSound);
                sound3 = Sounds > 3 ? sounds[3] : default(ChaserStateSound);
                sound4 = Sounds > 4 ? sounds[4] : default(ChaserStateSound);
            }

            public ChaserStateSound this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return sound0;
                        case 1: return sound1;
                        case 2: return sound2;
                        case 3: return sound3;
                        case 4: return sound4;
                    }

                    return new ChaserStateSound();
                }
            }
        }

        #endregion

    }
}

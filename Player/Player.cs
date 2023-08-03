using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Player : Node2D
    {
        //References to existing nodes
        public Level CurrentLevel;
        public Grid Grid = null;
        public Sprite PlayerSprite;
        public AnimationPlayer PlayerAnimation;
        public AnimationPlayer PlayerFX;

        private Curve CurrentMovementCurve;
        [Export]
        public Curve JumpCurve;
        [Export]
        public Curve DoubleJumpCurve;
        [Export]
        public Curve SwimCurve;
        [Export]
        public Curve DiveCurve;
        [Export]
        public Curve BounceCurve;
        [Export]
        public Curve SwimDistanceRationCurve;

        //Signals
        [Signal]
        public delegate void GoalReached();
        [Signal]
        public delegate void IncrementLevel();
        [Signal]
        public delegate void ScoreUpdated(int levelScore);
        [Signal]
        public delegate void HopCompleted(int hopsRemaining);
        [Signal]
        public delegate void TileChanged(Type NewType);
        [Signal]
        public delegate void HopsExhausted();
        [Signal]
        public delegate void Pause();
        [Signal]
        public delegate void MoveBehind();
        [Signal]
        public delegate void MoveToTop();
        [Signal]
        public delegate void Restart();
        [Signal]
        public delegate void BackToMap();
        [Signal]
        public delegate void Quit();
        [Signal]
        public delegate void PlayFailSound();

        //Player general parameters
        public int HopsRemaining { get; set; } = 3;
        public int TotalScore { get; set; } = 0;
        public int LevelScore { get; set; } = 0;
        public bool Active { get; private set; } = false;
        public bool RestartingLevel = false;

        private Vector2 _GridPosition;
        private Tile AnimationEndTile;
        public Queue<Vector2> MoveInputQueue;
        private Queue<AnimationNode> AnimationQueue;
        public AnimationNode CurrentAnimationNode { get; private set; }
        public float AnimationTimeElapsed = 0;
    
        public Vector2 GridPosition
        {
            get
            {
                return _GridPosition;
            }
            set
            {
                _GridPosition = value;
                Position = _GridPosition * Grid.TileSize + Grid.RectPosition;
            }
        }
        
        private Tile CurrentTile
        { 
            get
            {
                if (Grid == null) return null;
                return Grid.GetTile(GridPosition);   
            } 
        }

        public bool CurrentAnimationNearEnd { get; private set; }

        public override void _Ready()
        {
            Name = "Player";
        }

        public void Init(Level currentLevel, bool replay) //Called each time a new level starts
        {
            Deactivate();
            PlayerSprite = GetNode<Sprite>("PlayerSprite");
            PlayerAnimation = GetNode<AnimationPlayer>("PlayerSprite/AnimationPlayer");
            PlayerFX = GetNode<AnimationPlayer>("FXSprite/AnimationPlayer");

            Visible = false;
            AnimationEndTile = null;
            CurrentAnimationNode = null;
            AnimationTimeElapsed = 0;

            RestartingLevel = false;
            CurrentLevel = currentLevel;
            Grid = CurrentLevel.Grid;            
            GridPosition = CurrentLevel.PlayerStartPosition;
            HopsRemaining = CurrentLevel.StartingHops;
            LevelScore = CurrentLevel.ScoreRequired;

            MoveInputQueue = new Queue<Vector2>();

            EmitSignal(nameof(HopCompleted), HopsRemaining);
            EmitMoveToTop();
            ResetAnimation();
        }

        private void CalculateMovement(Vector2 Movement)
        {
            Queue<MovementNode> MovementNodes = new Queue<MovementNode>();
            bool Submerged = false;
            MovementNodes.Enqueue(new MovementNode(CurrentTile, Movement, Submerged));

            Vector2 JumpVector = GetJumpDistance(Movement);
            Vector2 JumpTargetPosition = GridPosition + JumpVector;
            AnimationEndTile = Grid.GetTile(Grid.LimitToBounds(JumpTargetPosition));
            
            do
            {
                if (AnimationEndTile.Type == Type.Bounce)
                {
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                    AnimationEndTile = Grid.GetTile(AnimationEndTile.GridPosition + Movement);
                }
                else if (AnimationEndTile.Type == Type.Direct)
                {
                    Movement = AnimationEndTile.BounceDirection;
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                    Vector2 NextTileLocation = AnimationEndTile.GridPosition + AnimationEndTile.BounceDirection;
                    //GD.Print($"Old bounce direction: {AnimationEndTile.BounceDirection}");
                    AnimationEndTile.BounceDirection = AnimationEndTile.BounceDirection.Rotated(Mathf.Pi / 2).Round();
                    //GD.Print($"New bounce direction: {AnimationEndTile.BounceDirection}");
                    /* GD.Print($"New bounce direction {AnimationEndTile.BounceDirection}");
                    GD.Print(AnimationEndTile.BounceDirection.Angle());
                    GD.Print(Mathf.PosMod(AnimationEndTile.BounceDirection.Angle() + Mathf.Tau, Mathf.Tau));
                    GD.Print((int)(Mathf.PosMod(AnimationEndTile.BounceDirection.Angle() + Mathf.Tau, Mathf.Tau) / Mathf.Tau * 4)); */
                    AnimationEndTile = Grid.GetTile(NextTileLocation);

                }
                else if ((AnimationEndTile.Type == Type.Rock))
                {
                    Movement = -Movement;
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                    do 
                    {
                        AnimationEndTile = Grid.GetTile(AnimationEndTile.GridPosition + Movement);
                    } while (AnimationEndTile.Type == Type.Rock);
                }
                else if (AnimationEndTile.Type == Type.Water)
                {
                    Submerged = true;
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                    if ((Grid.GetTile(AnimationEndTile.GridPosition + Movement).Type == Type.Rock)) 
                    {
                        MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                        Movement = -Movement;
                    }
                    else
                    {
                        Vector2 SwimTargetPosition = AnimationEndTile.GridPosition + Movement;
                        while (MovementNodes.Count < 100)
                        {
                            if (!Grid.WithinGrid(SwimTargetPosition) || Grid.GetTile(SwimTargetPosition).Type == Type.Rock)
                            {
                                MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition - Movement), Movement, Submerged));
                                MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition - Movement), -Movement, Submerged));
                                Movement = -Movement;
                            }
                            else if (Grid.GetTile(SwimTargetPosition).Type == Type.Water)
                            {
                                if (Grid.ViableLandingPoint(SwimTargetPosition + Movement))
                                {
                                    MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition), Movement, Submerged));
                                    Submerged = false;
                                    AnimationEndTile = Grid.GetTile(SwimTargetPosition + Movement);
                                    break;
                                }
                            }
                            SwimTargetPosition += Movement;
                        }
                    }
                }
                else
                {
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement, Submerged));
                    break;
                }
                if (MovementNodes.Count >= 100)
                {
                    GD.Print("Movement nodes exhausted.");
                    break;
                }
            } while (true);


            PrintNodes(MovementNodes);
            if (MovementNodes.Count >= 100)
            {
                EmitSignal(nameof(HopsExhausted));
            }
            else
            {
                CreateAnimationSequence(MovementNodes);
            }
        }

        private void CreateAnimationSequence(Queue<MovementNode> movementNodes)
        {
            AnimationQueue = new Queue<AnimationNode>();
            MovementNode current = null;
            MovementNode next = null;

            while (movementNodes.Count > 0)
            {
                if (current == null)
                {
                    current = movementNodes.Dequeue();
                } 
                else
                {
                    current = next;
                }
                next = movementNodes.Dequeue();

                AnimationNode nextAnimationNode = NextAnimationNode(current, next);
                if (nextAnimationNode != null)
                {
                    AnimationQueue.Enqueue(nextAnimationNode);
                }
                else
                {
                    GD.Print($"Error: Animation not found.");
                }
            }

            PrintAnimationSequence(AnimationQueue);

            if (CurrentTile.Type == Type.Jump)
            {
                CurrentTile.LilyAnimation.Play("Spring");
            }
            else
            {
                CurrentTile.LilyAnimation.Play("Jump");
            }
            CurrentTile.SplashAnimation.Play("Jump");

            UpdateHopsRemaining(-1);
            PlayNextAnimation();
        }

        private AnimationNode NextAnimationNode(MovementNode current, MovementNode next)
        {
            string Goal = null;
            string Length = null;
            string Movement = null;
            string Direction = null;
            string Suffix = null;

            Curve movementCurve;

            Type fromType = current.Tile.Type;
            Type toType = next.Tile.Type;

            // Goal part
            if (toType == Type.Goal && next.Tile.Activated) Goal = "Goal";

            // Movement part
            if (current.Submerged && next.Submerged)
            {
                Movement = "Swim";
                movementCurve = SwimCurve;
            }
            else if (fromType == Type.Bounce || fromType == Type.Rock || fromType == Type.Direct)
            {
                Movement = "Bounce";
                movementCurve = BounceCurve;
            }
            else
            {
                Movement = "Jump";
                movementCurve = JumpCurve;
            }

            // Length part
            if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() == 2 &&
                 Movement != "Swim")
            {   
                Length = "Double";
                movementCurve = DoubleJumpCurve;
            }

            // Suffix part
            if (Movement == "Swim" && next.MovementDirection == -current.MovementDirection)
            {
                Suffix = "Turn";
            }
            else if (!current.Submerged && next.Submerged)
            {
                Suffix = "Splash";
                movementCurve = DiveCurve;
            }
            else if (current.Submerged && !next.Submerged)
            {
                Suffix = "Exit";
            }

            // Direction part
            if (Suffix == "Turn")
            {
                Direction = MovementString(current.MovementDirection);
            }
            else
            {
                Direction = MovementString(current.MovementDirection);
            }            

            // Build the string and return the node (or null)
            //GD.Print($"{Goal}{Length}{Movement}{Direction}{Suffix}");
            Animation animation = PlayerAnimation.GetAnimation($"{Goal}{Length}{Movement}{Direction}{Suffix}");

            AnimationNode node = null;
            if (animation != null)
            {
                //GD.Print($"Animation name = '{animation.ResourceName}'");
                node = new AnimationNode(animation, next.Tile.GridPosition - current.Tile.GridPosition, movementCurve);
            }

            return node;
        }

        internal void ClearQueues()
        {
            MoveInputQueue.Clear();
            AnimationQueue.Clear();
        }

        private void PrintAnimationSequence(Queue<AnimationNode> animationQueue)
        {
            int i = 1;
            foreach (AnimationNode n in animationQueue)
            {
                //GD.Print($"Node {i} - {n.Animation.ResourceName} - {n.Movement} - {n.Curve.ResourceName}");
                i++;
            }
        }

        private void PrintNodes(Queue<MovementNode> movementNodes)
        {
            int i = 1;
            foreach (MovementNode n in movementNodes)
            {
                GD.Print($"Node {i} - {n.Tile.GridPosition} - {n.MovementDirection} - {n.Submerged}");
                i++;
            }
        }

        private Vector2 GetJumpDistance(Vector2 movementDirection)
        {
            if (CurrentTile.Type == Type.Jump) return movementDirection * CurrentTile.JumpLength;
            return movementDirection;
        }

        private string MovementString(Vector2 movement)
        {
            if      (movement == Vector2.Left)   return "Left";  
            else if (movement == Vector2.Right)  return "Right";
            else if (movement == Vector2.Up)     return "Up";
            return "Down";
        }

        public void TriggerLandAnimation()
        {
            if (AnimationEndTile.IsInsideTree() && AnimationEndTile.Type != Type.Rock) 
            {
                AnimationEndTile.SplashAnimation.Play("Land");
                if (AnimationEndTile.Type != Type.Water) AnimationEndTile.LilyAnimation.Play("Land");
                if (AnimationEndTile.Type == Type.Score) AnimationEndTile.CrunchPartilces();
                if (AnimationEndTile.Type == Type.Direct)
                {
                    /* GD.Print($"Bounce direction: {AnimationEndTile.BounceDirection}");
                    GD.Print($"Bounce angle: {AnimationEndTile.BounceDirection.Angle()}");
                    GD.Print($"360 angle: {Mathf.PosMod(AnimationEndTile.BounceDirection.Angle() + Mathf.Tau, Mathf.Tau)}"); */

                    //Implicitly adds a frame by adding Hframes (rather than Hframes - 1)
                    /* GD.Print($"Start frame: {AnimationEndTile.LilySprite.Frame}");
                    GD.Print($"Start frame + 3 + 1: {AnimationEndTile.LilySprite.Frame + AnimationEndTile.LilySprite.Hframes}");
                    GD.Print($"Remainder when divided by 3: {(AnimationEndTile.LilySprite.Frame + AnimationEndTile.LilySprite.Hframes + 1) % (AnimationEndTile.LilySprite.Hframes)}"); */
                    AnimationEndTile.LilySprite.Frame = (AnimationEndTile.LilySprite.Frame + AnimationEndTile.LilySprite.Hframes + 1) % (AnimationEndTile.LilySprite.Hframes);
                }
            }
        }

        public void TriggerSplashAnimation()
        {
            if (AnimationEndTile != null) AnimationEndTile.SplashAnimation.Play("Splash");
        }

        public void TriggerExitSplashAnimation()
        {
            CurrentTile.SplashAnimation.Play("Exit");
        }

        public void SkipToNextQueuedAnimation()
        {
            if (AnimationQueue.Count > 0 || MoveInputQueue.Count > 0)
/*                (AnimationEndTile.Type == Type.Rock ||
                AnimationEndTile.Type == Type.Bounce ||
                AnimationEndTile.Type == Type.Direct)) */
            {
                AfterAnimation(CurrentAnimationNode.Animation.ResourceName);
            }
        }

        public void AfterAnimation(string animationName) //TODO: Need to review this part of the procedure
        {
            AnimationTimeElapsed = 0;
            if (AnimationEndTile != null) GridPosition = AnimationEndTile.GridPosition;

            if (animationName.Contains("Goal"))
            {
                CheckGoal();
                LoadNextLevel();
            }
            else if (CurrentAnimationNode != null)
            {                
                if (AnimationQueue.Count > 0)
                {
                    PlayNextAnimation();
                }
                else
                {
                    UpdateScore();
                    if (!CheckGoal())
                    {
                        CheckHopsRemaining();
                    }
                    else
                    {
                        GD.Print("Goal reached when it shouldn't be or something.");
                    }
                    AnimationEndTile = null;
                    CurrentAnimationNode = null;
                }
            }
        }

        private void LoadNextLevel()
        {
            EmitSignal(nameof(IncrementLevel));
        }

        private void PlayNextAnimation()
        {
            if (HopsRemaining > -1)
            {
                CurrentAnimationNode = AnimationQueue.Dequeue();
                CurrentAnimationNearEnd = false;
                CurrentMovementCurve = CurrentAnimationNode.Curve;
                AnimationEndTile = Grid.GetTile(CurrentTile.GridPosition + CurrentAnimationNode.Movement);

                PlayerAnimation.Stop();
                PlayerAnimation.PlaybackSpeed = 1f;

                if (CurrentAnimationNode.Animation.ResourceName.Contains("Goal")) Deactivate();
                PlayerAnimation.Play(PlayerAnimation.FindAnimation(CurrentAnimationNode.Animation));
                //GD.Print($"Node - {CurrentAnimationNode.Animation.ResourceName} - {CurrentAnimationNode.Movement} - {CurrentAnimationNode.Curve.ResourceName}");
            }
        }

        public void UpdateHopsRemaining(int addedHops)
        {
            HopsRemaining += addedHops;
            if (HopsRemaining > CurrentLevel.MaximumHops)
            {
                HopsRemaining = CurrentLevel.MaximumHops;
            }
            EmitSignal(nameof(HopCompleted), HopsRemaining);
        }

        public void UpdateScore()
        {
            if (CurrentTile.PointValue > 0)
            {
                if (CurrentTile.Type == Type.Score) 
                {
                    CurrentTile.BugSprite.Visible = false;
                    CurrentTile.PointValue = 0;
                    CurrentTile.Label.Visible = false;
                    LevelScore -= 1;
                    EmitSignal(nameof(ScoreUpdated), LevelScore);
                }
            }
        }

        private bool CheckGoal()
        {
            if (CurrentTile.Type == Type.Goal && CurrentTile.Activated)
            {
                Deactivate();
                EmitSignal(nameof(GoalReached));
                return true;
            }
            return false;
        }

        private void CheckHopsRemaining()
        {
            if (HopsRemaining <= 0) Smoke();
        }

        public void ResetAnimation()
        {
            PlayerAnimation.Play("IdleDown");
        }

        public void Appear()
        {
            PlayerFX.Play("Appear");
        }

        private void Smoke()
        {
            Deactivate();
            PlayerFX.Play("Smoke");
            if (HopsRemaining <= 0) EmitSignal(nameof(PlayFailSound));
        }

        public void SendRestartSignal()
        {
            Deactivate();
            if (HopsRemaining <= 0)
            {
                EmitSignal(nameof(HopsExhausted));
            }
            EmitSignal(nameof(Restart));
        }

        public override void _Process(float delta)
        {
            if (Active)
            {
                if(CurrentAnimationNode == null && MoveInputQueue.Count > 0)
                {
                    CalculateMovement(MoveInputQueue.Dequeue());
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            AnimationNode an = CurrentAnimationNode;
            float MaxAnimationSpeed = 3f;

            if (!RestartingLevel && 
                CurrentTile != null && 
                AnimationEndTile != null && 
                an != null)
            {
                if (CurrentAnimationNode.Animation.ResourceName.Left(4) == "Goal")
                {
                    PlayerAnimation.PlaybackSpeed = 1f;
                    //GD.Print("Set goal speed.");
                }
                else if (MoveInputQueue.Count > 0)
                {
                    PlayerAnimation.PlaybackSpeed = Mathf.Clamp(PlayerAnimation.PlaybackSpeed + delta * 10, 1f , MaxAnimationSpeed);
                } 
                else
                {
                    PlayerAnimation.PlaybackSpeed = Mathf.Clamp(PlayerAnimation.PlaybackSpeed - delta * 10, 1f , MaxAnimationSpeed);
                } 

                AnimationTimeElapsed += delta;

                float animationLength;
                if (an.Animation.ResourceName.Left(4) == "Swim" &&
                    an.Animation.ResourceName.Right(an.Animation.ResourceName.Length - 4) != "Turn")
                {
                    animationLength = SwimDistanceRationCurve.Interpolate((an.Movement.Length()) / 6);
                }
                else
                {
                    animationLength = an.Animation.Length / PlayerAnimation.PlaybackSpeed;
                }
                
                float totalDistance = (AnimationEndTile.GlobalPosition - CurrentTile.GlobalPosition).Length();
                float distanceRemaining = (AnimationEndTile.GlobalPosition - GlobalPosition).Length();
                float distanceTravelled = totalDistance - distanceRemaining;
                float timeRatio = AnimationTimeElapsed / animationLength;
                float pixelsToMove = (totalDistance * CurrentMovementCurve.Interpolate(timeRatio)) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(AnimationEndTile.GlobalPosition, pixelsToMove);
                
                if (timeRatio > 0.9) CurrentAnimationNearEnd = true;

                //GD.Print($"{an.Animation.ResourceName} -AniLen {animationLength} -TotDis {totalDistance} -DisTra {distanceTravelled} -DisRem {distanceRemaining} -Pix {pixelsToMove} -TimRat {timeRatio} -Interp {CurrentMovementCurve.Interpolate(timeRatio)}");
                //GD.Print($"-DisTra {distanceTravelled} -DisRem {distanceRemaining} -Pix {pixelsToMove} -TimRat {timeRatio} -Interp {CurrentMovementCurve.Interpolate(timeRatio)}");
                //GD.Print($"DR{distanceRemaining}_   TR {timeRatio}");
                
                if (GlobalPosition == AnimationEndTile.GlobalPosition &&
                    (an.Curve == SwimCurve) &&
                    an.Animation.ResourceName.Right(an.Animation.ResourceName.Length - 4) != "Turn")
                {
                    if (AnimationEndTile.IsInsideTree() == false) AnimationEndTile.QueueFree();
                    AfterAnimation(an.Animation.ResourceName);
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Active && @event.IsActionPressed("ui_cancel"))
            {   
                Deactivate();
                EmitSignal(nameof(Pause));
            }
            else if (Active && @event.IsActionPressed("ui_restart"))
            {
                RestartPressed();
            }
            else if (Active && @event.IsActionPressed("ui_quit"))
            {
                EmitSignal(nameof(Quit));
            }
            else if (Active && @event.IsActionPressed("ui_map"))
            {
                EmitSignal(nameof(BackToMap));
            }

            string currentResourceName = CurrentAnimationNode == null ? "" : CurrentAnimationNode.Animation.ResourceName;
            if ((Active && MoveInputQueue.Count <= HopsRemaining) && 
                (
                   (CurrentAnimationNearEnd) ||
                   (!currentResourceName.Contains("Swim") &&
                    !currentResourceName.Contains("Splash") &&
                    !currentResourceName.Contains("Bounce"))
                ))
            {
                if      (@event.IsActionPressed("ui_left"))  MoveInputQueue.Enqueue(Vector2.Left);
                else if (@event.IsActionPressed("ui_right")) MoveInputQueue.Enqueue(Vector2.Right);
                else if (@event.IsActionPressed("ui_up"))    MoveInputQueue.Enqueue(Vector2.Up);
                else if (@event.IsActionPressed("ui_down"))  MoveInputQueue.Enqueue(Vector2.Down);

/*                 if (@event.IsActionPressed("ui_left")  ||
                    @event.IsActionPressed("ui_right") ||
                    @event.IsActionPressed("ui_up")   ||
                    @event.IsActionPressed("ui_down"))
                    {
                        if (@event is InputEventKey eventKey) GD.Print(OS.GetScancodeString(eventKey.PhysicalScancode));
                    } */
            }
        }

        public void RestartPressed()
        {
            if (CurrentAnimationNode != null)
            {
                if (CurrentAnimationNode.Animation.ResourceName.Left(4) == "Swim" ||
                    CurrentAnimationNode.Animation.ResourceName.Left(5) == "Splash")
                {
                    SendRestartSignal();
                }
                else
                {
                    Smoke();
                }
            }
            else
            {
                Smoke();
            }
        }

        public void Activate()      
        { 
            SetProcessInput(true);
            Active = true; 
        }
        public void Deactivate()    
        { 
            SetProcessInput(false);
            Active = false; 
        }

        public void EmitMoveToTop()     { EmitSignal(nameof(MoveToTop)); }
        public void EmitMoveBehind()    { EmitSignal(nameof(MoveBehind)); }
    }
}
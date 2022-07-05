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
        public delegate void ScoreUpdated(int totalScore, int levelScore = 0, int minScore = 0);
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

        public override void _Ready()
        {
            Name = "Player";
        }

        public void Init(Level currentLevel, bool replay) //Called each time a new level starts
        {
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
            LevelScore = 0;

            MoveInputQueue = new Queue<Vector2>();

            EmitSignal(nameof(HopCompleted), HopsRemaining);
            EmitMoveToTop();
            ResetAnimation();
        }

        private void CalculateMovement(Vector2 Movement)
        {
            Queue<MovementNode> MovementNodes = new Queue<MovementNode>();
            MovementNodes.Enqueue(new MovementNode(CurrentTile, Movement));

            Vector2 JumpVector = GetJumpDistance(Movement);
            Vector2 JumpTargetPosition = GridPosition + JumpVector;
            AnimationEndTile = Grid.GetTile(Grid.LimitToBounds(JumpTargetPosition));
            
            do
            {
                if (AnimationEndTile.Type == Type.Bounce)
                {
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));
                    AnimationEndTile = Grid.GetTile(AnimationEndTile.GridPosition + Movement);
                }
                else if ((AnimationEndTile.Type == Type.Rock))
                {
                    Movement = -Movement;
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));
                    do 
                    {
                        AnimationEndTile = Grid.GetTile(AnimationEndTile.GridPosition + Movement);
                    } while (AnimationEndTile.Type == Type.Rock);
                    
                }
                else if (AnimationEndTile.Type == Type.Water &&
                        (Grid.GetTile(AnimationEndTile.GridPosition + Movement).Type == Type.Rock)) 
                {
                    MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));
                    Movement = -Movement;
                }
                MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));

                if (AnimationEndTile.Type == Type.Water)
                {
                    Vector2 SwimTargetPosition = AnimationEndTile.GridPosition + Movement;
                    while (MovementNodes.Count < 20)
                    {
                        if (!Grid.WithinGrid(SwimTargetPosition) || Grid.GetTile(SwimTargetPosition).Type == Type.Rock)
                        {
                            MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition - Movement), Movement));
                            MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition - Movement), -Movement));
                            Movement = -Movement;
                        }
                        else if (Grid.GetTile(SwimTargetPosition).Type == Type.Water)
                        {
                            if (Grid.ViableLandingPoint(SwimTargetPosition + Movement))
                            {
                                MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition), Movement));
                                AnimationEndTile = Grid.GetTile(SwimTargetPosition + Movement);
                                MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));
                                break;
                            }
                        }
                        SwimTargetPosition += Movement;
                    }
                }

            } while (AnimationEndTile.Type == Type.Bounce || AnimationEndTile.Type == Type.Rock);


            PrintNodes(MovementNodes);
            if (MovementNodes.Count >= 20)
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

                AnimationNode nextAnimationNode = NextAnimationNode(movementNodes, current, next);
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

            PrintAnimationSequence(AnimationQueue);
            UpdateHopsRemaining(-1);
            PlayNextAnimation();
        }

        private AnimationNode NextAnimationNode(Queue<MovementNode> movementNodes, MovementNode current, MovementNode next) //FIXME: need to remove reference to movementNodes here
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
            if (fromType == Type.Water && toType == Type.Water)
            {
                Movement = "Swim";
                movementCurve = SwimCurve;
            }
            else if (fromType == Type.Bounce || fromType == Type.Rock)
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
            else if (toType == Type.Water && fromType != Type.Water)
            {
                Suffix = "Splash";
                //movementCurve = DiveCurve;
            }
            else if (toType != Type.Water && fromType == Type.Water)
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

            AnimationNode node = null;
            // Build the string and return the node (or null)
            Animation animation = PlayerAnimation.GetAnimation($"{Goal}{Length}{Movement}{Direction}{Suffix}");

            GD.Print($"Built string = '{Goal}{Length}{Movement}{Direction}{Suffix}'");

            if (animation != null)
            {
                //GD.Print($"Animation name = '{animation.ResourceName}'");
                node = new AnimationNode(animation, next.Tile.GridPosition - current.Tile.GridPosition, movementCurve);
            }

            return node;


            /* if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() == 2)
            {
                if (toType == Type.Rock)
                {
                    Prefix += "DoubleBounce";
                    movementCurve = BounceCurve;
                }
                else
                {
                    Prefix += "DoubleJump";
                    movementCurve = DoubleJumpCurve;
                }
            }
            else
            {
                if (toType == Type.Rock || toType == Type.Bounce)
                {
                    Prefix += "Bounce";
                    movementCurve = JumpCurve; //FIXME: testing with jumpcurve
                }
                else
                {
                    Prefix += "Jump";
                    movementCurve = JumpCurve;
                }
            } 

            if (toType == Type.Water) Suffix = "Splash";

            return new AnimationNode(
                PlayerAnimation.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"),
                next.Tile.GridPosition - current.Tile.GridPosition,
                movementCurve);

            if (movementNodes.Count > 0) //FIXME: remove this
            {
                if (next.MovementDirection == -current.MovementDirection)
                {
                    if (toType == Type.Water)
                    {
                        return new AnimationNode(
                            PlayerAnimation.GetAnimation($"Swim{MovementString(current.MovementDirection)}Turn"),
                            Vector2.Zero, SwimCurve);
                    }
                }

                do  //FIXME: This is a return
                {
                    current = next;
                    next = movementNodes.Dequeue();
                }
                while (current.Tile.GridPosition == next.Tile.GridPosition &&
                       current.MovementDirection == next.MovementDirection);


                //This is the swimming (or bounce into water) part
                while (movementNodes.Count >= 1) //FIXME: remove this
                {
                    if (fromType == Type.Rock)
                    {
                        movementCurve = JumpCurve;
                        Prefix = "Jump"; Suffix = "Splash";
                    }
                    else
                    {
                        if (Suffix == "Splash")
                            movementCurve = DiveCurve;
                        else
                            movementCurve = SwimCurve;
                        Prefix = "Swim"; Suffix = null;
                    }

                    return new AnimationNode(
                        PlayerAnimation.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"),
                        next.Tile.GridPosition - current.Tile.GridPosition, movementCurve);

                    if (next.MovementDirection == -current.MovementDirection)
                    {
                        return new AnimationNode(
                            PlayerAnimation.GetAnimation($"Swim{MovementString(current.MovementDirection)}Turn"),
                            Vector2.Zero, SwimCurve);  //FIXME: This is a return
                    }

                    do //FIXME: This is a return
                    {
                        current = next;
                        next = movementNodes.Dequeue();
                    }
                    while (current.Tile.GridPosition == next.Tile.GridPosition &&
                        current.MovementDirection == next.MovementDirection);

                }

                Prefix = "";
                if (toType == Type.Goal && next.Tile.Activated) Prefix = "Goal";

                //This is the exit/return bounce leap
                if (movementNodes.Count == 0)
                {
                    if (fromType == Type.Water)
                    {
                        Prefix += "Jump"; Suffix = "Exit";
                        movementCurve = JumpCurve;
                    }
                    else if (fromType == Type.Rock || fromType == Type.Bounce)
                    {
                        if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() >= 2)
                        {
                            Prefix += "DoubleJump";
                            movementCurve = DoubleJumpCurve;
                        }
                        else
                        {
                            Prefix += "Jump";
                            movementCurve = JumpCurve;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException(); //Shouldn't happen
                    }

                    return new AnimationNode(
                        PlayerAnimation.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"),
                        next.Tile.GridPosition - current.Tile.GridPosition, movementCurve);
                }
            }*/
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
                GD.Print($"Node {i} - {n.Animation.ResourceName} - {n.Movement} - {n.Curve.ResourceName}");
                i++;
            }
        }

        private void PrintNodes(Queue<MovementNode> movementNodes)
        {
            int i = 1;
            foreach (MovementNode n in movementNodes)
            {
                GD.Print($"Node {i} - {n.Tile.GridPosition} - {n.MovementDirection}");
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
            if (AnimationEndTile.IsInsideTree()) 
            {
                AnimationEndTile.SplashAnimation.Play("Land");
                if (AnimationEndTile.Type != Type.Water) AnimationEndTile.LilyAnimation.Play("Land");
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
            if (MoveInputQueue.Count > 0)
            {
                AfterAnimation(CurrentAnimationNode.Animation.ResourceName);
            }
/*             else if (AnimationQueue.Peek().Animation.ResourceName.Contains("Bounce"))
            {
                //FIXME: maybe here to make the bounce quicker
            } */
        }

        public void AfterAnimation(string animationName) //TODO: Need to review this part of the procedure
        {
            AnimationTimeElapsed = 0;
            if (AnimationEndTile != null) GridPosition = AnimationEndTile.GridPosition;

            if (animationName.Left(4) == "Goal")
            {
                CheckGoal();
                EmitSignal(nameof(IncrementLevel)); //FIXME: this happens at the end of the set of levels
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
                    AnimationEndTile = null;
                    CurrentAnimationNode = null;
                }
            }
        }

        private void PlayNextAnimation()
        {
            if (HopsRemaining > -1)
            {
                CurrentAnimationNode = AnimationQueue.Dequeue();
                CurrentMovementCurve = CurrentAnimationNode.Curve;
                AnimationEndTile = Grid.GetTile(CurrentTile.GridPosition + CurrentAnimationNode.Movement);
                PlayerAnimation.Stop();
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
                LevelScore += CurrentTile.PointValue;
                TotalScore += CurrentTile.PointValue;
                if (CurrentTile.Type == Type.Score) 
                {
                    CurrentTile.BugSprite.Visible = false;
                    CurrentTile.PointValue = 0;
                    CurrentTile.Label.Visible = false;
                }
                EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore, CurrentLevel.ScoreRequired);
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
            if (!RestartingLevel && 
                CurrentTile != null && 
                AnimationEndTile != null && 
                CurrentAnimationNode != null)
            {
                AnimationTimeElapsed += delta;

                float animationLength;
                if (CurrentAnimationNode.Animation.ResourceName.Left(4) == "Swim" &&
                    CurrentAnimationNode.Animation.ResourceName.Right(CurrentAnimationNode.Animation.ResourceName.Length-4) != "Turn")
                {
                    animationLength = SwimDistanceRationCurve.Interpolate((CurrentAnimationNode.Movement.Length())/6);
                }
                else
                {
                    animationLength = CurrentAnimationNode.Animation.Length;
                }
                
                float totalDistance = (AnimationEndTile.GlobalPosition - CurrentTile.GlobalPosition).Length();
                float distanceRemaining = (AnimationEndTile.GlobalPosition - GlobalPosition).Length();
                float distanceTravelled = totalDistance - distanceRemaining;
                float timeRatio = AnimationTimeElapsed/animationLength;
                float pixelsToMove = (totalDistance * CurrentMovementCurve.Interpolate(timeRatio)) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(AnimationEndTile.GlobalPosition, pixelsToMove);
                
                //GD.Print($"{CurrentAnimationNode.Animation.ResourceName} -AniLen {animationLength} -TotDis {totalDistance} -DisTra {distanceTravelled} -DisRem {distanceRemaining} -Pix {pixelsToMove} -TimRat {timeRatio} -Interp {CurrentMovementCurve.Interpolate(timeRatio)}");
                //GD.Print($"-DisTra {distanceTravelled} -DisRem {distanceTravelled} -Pix {pixelsToMove} -TimRat {timeRatio} -Interp {CurrentMovementCurve.Interpolate(timeRatio)}");

                if (GlobalPosition == AnimationEndTile.GlobalPosition &&
                    (CurrentAnimationNode.Curve == SwimCurve ||
                    CurrentAnimationNode.Curve == DiveCurve) &&
                    CurrentAnimationNode.Animation.ResourceName.Right(CurrentAnimationNode.Animation.ResourceName.Length-4) != "Turn")
                {
                    if (AnimationEndTile.IsInsideTree() == false) AnimationEndTile.QueueFree();
                    AfterAnimation(CurrentAnimationNode.Animation.ResourceName);
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Active && @event.IsActionPressed("ui_cancel"))
            {   
                EmitSignal(nameof(Pause));
            }
            else if (Active && @event.IsActionPressed("ui_restart"))
            {
                if (CurrentAnimationNode != null)
                {
                    //GD.Print(CurrentAnimationNode.Animation.ResourceName.Left(4));
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
            else if (Active && @event.IsActionPressed("ui_quit"))
            {
                EmitSignal(nameof(Quit));
            }

            if (Active && MoveInputQueue.Count <= HopsRemaining) //FIXME: world is not null??
            {
                if (@event.IsActionPressed("ui_left")) MoveInputQueue.Enqueue(Vector2.Left);
                else if (@event.IsActionPressed("ui_right")) MoveInputQueue.Enqueue(Vector2.Right);
                else if (@event.IsActionPressed("ui_up")) MoveInputQueue.Enqueue(Vector2.Up);
                else if (@event.IsActionPressed("ui_down")) MoveInputQueue.Enqueue(Vector2.Down);
            }
        }

        public void Activate()      { Active = true; }
        public void Deactivate()    { Active = false; }

        public void EmitMoveToTop()     { EmitSignal(nameof(MoveToTop)); }
        public void EmitMoveBehind()    { EmitSignal(nameof(MoveBehind)); }
    }
}
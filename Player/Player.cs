using System;
using Godot;
using System.Collections.Generic;


namespace Hopper
{
    public class Player : Node2D
    {
        //References to existing nodes
        public Level CurrentLevel;
        public Grid Grid;
        private Sprite PlayerSprite;
        public AnimationPlayer AnimationPlayer;

        //Player general parameters
        public int HopsRemaining { get; set; } = 3;
        public int TotalScore { get; set; } = 0;
        public int LevelScore { get; set; } = 0;
        public bool Active = false;

        private Vector2 _GridPosition;
        public Vector2 GridPosition
        {
            get
            {
                return _GridPosition;
            }
            set
            {
                _GridPosition = value;
                Position = _GridPosition * Grid.TileSize + Grid.RectPosition;// + Grid.TileSize / 2;
            }
        }
        
        private Tile CurrentTile 
        { 
            get
            {
                return Grid.GetTile(GridPosition);   
            } 
        }

        private Tile AnimationEndTile;
        public Queue<Vector2> MoveInputQueue;
        private Queue<AnimationNode> AnimationQueue;
        public AnimationNode CurrentAnimationNode { get; private set; } = null;
        public float AnimationTimeElapsed = 0;
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
        public delegate void ScoreUpdated(int totalScore, int levelScore = 0);
        [Signal]
        public delegate void HopCompleted(int hopsRemaining);
        [Signal]
        public delegate void TileChanged(Type NewType);
        [Signal]
        public delegate void HopsExhausted();
        [Signal]
        public delegate void QuitToMenu();
        [Signal]
        public delegate void MoveBehind();
        [Signal]
        public delegate void MoveToTop();

        public override void _Ready()
        {
            Name = "Player";
        }

        public void Init(Level currentLevel, bool replay) //Called each time a new level starts
        {
            PlayerSprite = GetNode<Sprite>("PlayerSprite");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            CurrentLevel = currentLevel;
            Grid = CurrentLevel.Grid;            
            GridPosition = CurrentLevel.PlayerStartPosition;
            HopsRemaining = CurrentLevel.StartingHops;
            LevelScore = 0;

            MoveInputQueue = new Queue<Vector2>();

            EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            EmitSignal(nameof(HopCompleted), HopsRemaining);
            
            ResetAnimation();
            
            if (replay)
            {
                Active = true;
            }
            else
            {
                Active = false;
            }
        }

        private void CalculateMovement(Vector2 Movement)
        {
            Queue<MovementNode> MovementNodes = new Queue<MovementNode>();
            MovementNodes.Enqueue(new MovementNode(CurrentTile, Movement));

            Vector2 JumpVector = GetJumpDistance(Movement);
            Vector2 JumpTargetPosition = GridPosition + JumpVector;

            AnimationEndTile = Grid.GetTile(Grid.LimitToBounds(JumpTargetPosition));
            if ((AnimationEndTile.Type == Type.Rock))
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
                        Movement = -Movement;
                        MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition + Movement), Movement));
                    }
                    else if (Grid.GetTile(SwimTargetPosition).Type == Type.Water)
                    {
                        if (Grid.ViableLandingPoint(SwimTargetPosition + Movement))
                        {
                            MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition), Movement));
                            MovementNodes.Enqueue(new MovementNode(Grid.GetTile(SwimTargetPosition + Movement), Movement));
                            break;
                        }
                    }
                    SwimTargetPosition += Movement;
                }
            }

            PrintNodes(MovementNodes);
            if (MovementNodes.Count >= 20)
            {
                GD.Print("Impossible sequence");
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
            MovementNode current = movementNodes.Dequeue();
            MovementNode next = movementNodes.Dequeue();

            string Prefix = null, Suffix = null;
            Curve movementCurve;

            //This is the jumping part
            if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() == 2)
            {
                if (next.Tile.Type == Type.Rock)
                {
                    Prefix = "DoubleBounce";
                    movementCurve = BounceCurve;
                }
                else
                {
                    Prefix = "DoubleJump";
                    movementCurve = DoubleJumpCurve;
                }
            }
            else
            {
                if (next.Tile.Type == Type.Rock)
                {
                    Prefix = "Bounce";
                    movementCurve = JumpCurve; //FIXME: testing with jumpcurve
                }
                else
                {
                    Prefix = "Jump";
                    movementCurve = JumpCurve;
                }
            }

            if (next.Tile.Type == Type.Water) Suffix = "Splash";

            AnimationQueue.Enqueue(new AnimationNode(
                AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                next.Tile.GridPosition - current.Tile.GridPosition, 
                movementCurve));

            if (movementNodes.Count > 0)
            {
                if (next.MovementDirection == -current.MovementDirection)
                {
                    if (next.Tile.Type == Type.Water)
                    {
                        AnimationQueue.Enqueue(new AnimationNode(
                            AnimationPlayer.GetAnimation($"Swim{MovementString(current.MovementDirection)}Turn"), 
                            Vector2.Zero, SwimCurve));
                    }
                }

                do
                {
                    current = next;
                    next = movementNodes.Dequeue();
                }
                while (current.Tile.GridPosition == next.Tile.GridPosition && 
                       current.MovementDirection == next.MovementDirection);

                
                //This is the swimming (or bounce into water) part
                while (movementNodes.Count >= 1)
                {
                    if (current.Tile.Type == Type.Rock)
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

                    AnimationQueue.Enqueue(new AnimationNode(
                        AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                        next.Tile.GridPosition - current.Tile.GridPosition, movementCurve));
                    
                    if (next.MovementDirection == -current.MovementDirection)
                    {
                        AnimationQueue.Enqueue(new AnimationNode(
                            AnimationPlayer.GetAnimation($"Swim{MovementString(current.MovementDirection)}Turn"), 
                            Vector2.Zero, SwimCurve));
                    }

                    do
                    {
                        current = next;
                        next = movementNodes.Dequeue();
                    }
                    while (current.Tile.GridPosition == next.Tile.GridPosition && 
                        current.MovementDirection == next.MovementDirection);

                }

                //This is the exit/return bounce leap
                if (movementNodes.Count == 0)
                {
                    if (current.Tile.Type == Type.Water)
                    {
                        Prefix = "Jump"; Suffix = "Exit";
                        movementCurve = JumpCurve;
                    }
                    else if (current.Tile.Type == Type.Rock)
                    {
                        if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() >= 2)
                        {
                            Prefix = "DoubleJump";
                            movementCurve = DoubleJumpCurve;
                        }
                        else
                        {
                            Prefix = "Jump";
                            movementCurve = JumpCurve;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException(); //Shouldn't happen
                    }
                    AnimationQueue.Enqueue(new AnimationNode(
                        AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                        next.Tile.GridPosition - current.Tile.GridPosition, movementCurve));
                    
                }
            }

            //End extras and playing
            //PrintAnimationSequence(AnimationQueue);

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

            PlayNextAnimation();
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
                //GD.Print($"Node {i} - {n.Tile.GridPosition} - {n.MovementDirection}");
                //if (n.Tile != null) GD.Print($"Node {i} - {n.Tile.GridPosition} - {n.MovementDirection}");
                //else GD.Print($"Node {i} - outside - {n.MovementDirection}");
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

        public void AfterAnimation(string animationName) //TODO: Need to review this part of the procedure
        {
            if (animationName == "LevelComplete")
            {
                EmitSignal(nameof(IncrementLevel));
            }
            else if (CurrentAnimationNode != null)
            {
                AnimationTimeElapsed = 0;
                GridPosition = AnimationEndTile.GridPosition;
                
                if (AnimationQueue.Count > 0)
                {
                    PlayNextAnimation();
                }
                else
                {
                    UpdateHopsRemaining(-1);
                    UpdateScore();
                    if (!CheckGoal())
                    {
                        CheckHopsRemaining();
                    }
                    else
                    {
                        AnimationPlayer.Play("LevelComplete");
                    }
                    AnimationEndTile = null;
                    CurrentAnimationNode = null;
                }
            }
        }

        private void PlayNextAnimation()
        {
            CurrentAnimationNode = AnimationQueue.Dequeue();
            CurrentMovementCurve = CurrentAnimationNode.Curve;
            
            //SetAnimationTiles();
            AnimationEndTile = Grid.GetTile(CurrentTile.GridPosition + CurrentAnimationNode.Movement);

            AnimationPlayer.Play(AnimationPlayer.FindAnimation(CurrentAnimationNode.Animation));
            //GD.Print($"Node - {CurrentAnimationNode.Animation.ResourceName} - {CurrentAnimationNode.Movement} - {CurrentAnimationNode.Curve.ResourceName}");
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
                    EmitSignal(nameof(TileChanged), Type.Lily);
                }
                EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            }
        }

        private bool CheckGoal()
        {
            if (CurrentTile.Type == Type.Goal && CurrentTile.Activated)
            {
                EmitSignal(nameof(GoalReached));
                //UpdateHopsRemaining(CurrentLevel.StartingHops); //FIXME: Hops added should come from next level
                return true;
            }
            return false;
        }

        private void CheckHopsRemaining()
        {
            if (HopsRemaining <= 0)
            {
                EmitSignal(nameof(HopsExhausted));
            }
        }

        public void ResetAnimation()
        {
            AnimationPlayer.Play("IdleDown");
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
            if (AnimationEndTile != null && CurrentAnimationNode != null)
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
            if (@event.IsActionPressed("ui_cancel"))
            {   
                EmitSignal(nameof(QuitToMenu));
            }

            if (Active) //FIXME: world is not null??
            {
                if (@event.IsActionPressed("ui_left")) MoveInputQueue.Enqueue(Vector2.Left);
                else if (@event.IsActionPressed("ui_right")) MoveInputQueue.Enqueue(Vector2.Right);
                else if (@event.IsActionPressed("ui_up")) MoveInputQueue.Enqueue(Vector2.Up);
                else if (@event.IsActionPressed("ui_down")) MoveInputQueue.Enqueue(Vector2.Down);
            }
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void Activate()
        {
            Active = true;
        }

        public void EmitMoveBehind()
        {
            EmitSignal(nameof(MoveBehind));
        }
        
        public void EmitMoveToTop()
        {
            EmitSignal(nameof(MoveToTop));
        }
    }
}
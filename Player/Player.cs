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
        
        private Tile _CurrentTile;
        private Tile CurrentTile 
        { 
            get
            {
                return Grid.GetTile(GridPosition);   
            } 
        }


        //Movement and animation
        //private Queue<MovementNode> MovementNodes;

        private Tile AnimationEndTile;


        //private Queue<Tile> TurnTiles;
        public Queue<Vector2> MoveInputQueue;
        private Queue<AnimationNode> AnimationQueue;
        public Animation CurrentAnimation = null;
        public AnimationNode CurrentAnimationNode { get; private set; } = null;
        public float AnimationTimeElapsed = 0;
        private Curve currentJumpCurve;
        [Export]
        public Curve JumpCurve;
        [Export]
        public Curve DoubleJumpCurve;
        

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

        public override void _Ready()
        {
            Name = "Player";
            //CallDeferred("Init");
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

            //Vector2 MovementVector;
            Vector2 JumpVector = GetJumpDistance(Movement);
            Vector2 JumpTargetPosition = GridPosition + JumpVector;

            if ((Grid.WithinGrid(JumpTargetPosition) && Grid.GetTile(JumpTargetPosition).Type == Type.Rock) ||
                 !Grid.WithinGrid(JumpTargetPosition))
            {
                Movement = -Movement;
                AnimationEndTile = Grid.GetTile(Grid.LimitToBounds(JumpTargetPosition + Movement));
                // If the direction switches after the 1st one, it will always be a bounce
            }
            else
            {
                AnimationEndTile = Grid.GetTile(Grid.LimitToBounds(GridPosition + JumpVector));
                if (AnimationEndTile.Type == Type.Water &&
                    (!Grid.WithinGrid(AnimationEndTile.GridPosition + Movement) ||
                     Grid.GetTile(AnimationEndTile.GridPosition + Movement).Type == Type.Rock)) 
                {
                    Movement = -Movement;
                }
            }
            MovementNodes.Enqueue(new MovementNode(AnimationEndTile, Movement));

            if (AnimationEndTile.Type == Type.Water)
            {
                Vector2 SwimTargetPosition = AnimationEndTile.GridPosition + Movement;
                while (true)
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
            //can add an else if here for other types that affect movement at a later point
            PrintNodes(MovementNodes);
            //maybe clean array of duplicates?
            CreateAnimationSequence(MovementNodes);
        }

        private void CreateAnimationSequence(Queue<MovementNode> movementNodes)
        {
            Queue<AnimationNode> AnimationQueue = new Queue<AnimationNode>();
            MovementNode current = movementNodes.Dequeue();
            MovementNode next = movementNodes.Peek();

            string Prefix = null, Suffix = null;


            //This is the jumping part
            if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() == 2)
            {
                Prefix = "DoubleJump";
                currentJumpCurve = DoubleJumpCurve;
            }
            else if (current.MovementDirection != next.MovementDirection)
            {
                if ((next.Tile.GridPosition - current.Tile.GridPosition).Length() == 0)
                {
                    Prefix = "Bounce";
                    currentJumpCurve = JumpCurve;
                }
                else
                {
                    Prefix = "DoubleBounce";
                    currentJumpCurve = DoubleJumpCurve;
                }
            }
            else
            {
                Prefix = "Jump";
                currentJumpCurve = JumpCurve;
            }

            if (next.Tile.Type == Type.Water) Suffix = "Splash";

            AnimationQueue.Enqueue(new AnimationNode(
                AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                next.Tile.GridPosition - current.Tile.GridPosition));

            Suffix = null;

            if (movementNodes.Count > 1)
            {
                //This bit needs to be a simple function
                current = movementNodes.Dequeue();
                next = movementNodes.Peek();

                while (current.Tile.GridPosition == next.Tile.GridPosition && 
                    current.MovementDirection == next.MovementDirection)
                {
                    current = movementNodes.Dequeue();
                    next = movementNodes.Peek();
                }

                
                //This is the swimming part
                while (movementNodes.Count >= 2)
                {
                    Prefix = "Swim"; Suffix = null;

                    AnimationQueue.Enqueue(new AnimationNode(
                        AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                        next.Tile.GridPosition - current.Tile.GridPosition));
                    
                    //This bit needs to be a simple function
                    current = movementNodes.Dequeue();
                    next = movementNodes.Peek();

                    while (current.Tile.GridPosition == next.Tile.GridPosition && 
                        current.MovementDirection == next.MovementDirection)
                    {
                        current = movementNodes.Dequeue();
                        next = movementNodes.Peek();
                    }
                }

                //This is the exit leap
                if (movementNodes.Count == 1)
                {
                    Prefix = "Jump"; Suffix = "Exit";
                    AnimationQueue.Enqueue(new AnimationNode(
                        AnimationPlayer.GetAnimation($"{Prefix}{MovementString(current.MovementDirection)}{Suffix}"), 
                        next.Tile.GridPosition - current.Tile.GridPosition));
                }
            }


            //End extras and playing
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

            CurrentAnimationNode = AnimationQueue.Dequeue();
            AnimationPlayer.Play(AnimationPlayer.FindAnimation(CurrentAnimationNode.Animation));
        }

        private void PrintAnimationSequence(Queue<AnimationNode> animationQueue)
        {
            int i = 1;
            foreach (AnimationNode n in animationQueue)
            {
                GD.Print($"Node {i} - {n.Animation.ResourceName} - {n.Movement}");
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

        public void ExitAnimation()
        {
/*             CurrentAnimation = null;
            SplashTile = null;
            GlobalPosition = ExitTile.GlobalPosition;
            GridPosition = ExitTile.GridPosition;
            Animate((LandTile.GridPosition - ExitTile.GridPosition), "Jump", "Exit"); */
        }

        private string MovementString(Vector2 movement)
        {
            if      (movement == Vector2.Left)   return "Left";  
            else if (movement == Vector2.Right)  return "Right";
            else if (movement == Vector2.Up)     return "Up";
            return "Down";
        }

        private void Animate(Vector2 movement, string jumpPrefix, string jumpSuffix = "")
        {
/*             if      (movement == Vector2.Left)   CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Left{jumpSuffix}");  
            else if (movement == Vector2.Right)  CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Right{jumpSuffix}");
            else if (movement == Vector2.Up)     CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Up{jumpSuffix}");
            else if (movement == Vector2.Down)   CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Down{jumpSuffix}");
            if (CurrentTile.Type == Type.Jump)
            {
                CurrentTile.LilyAnimation.Play("Spring");
            }
            else
            {
                CurrentTile.LilyAnimation.Play("Jump");
            }
            CurrentTile.SplashAnimation.Play("Jump");
            AnimationPlayer.Play(AnimationPlayer.FindAnimation(CurrentAnimation)); */
        }

        public void TriggerLandAnimation()
        {
            AnimationEndTile.SplashAnimation.Play("Land");
            if (AnimationEndTile.Type != Type.Water) AnimationEndTile.LilyAnimation.Play("Land");
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
            if (CurrentAnimationNode != null)
            {
                AnimationTimeElapsed = 0;

                if (animationName == "LevelComplete")
                {
                    EmitSignal(nameof(IncrementLevel));
                }

                if (AnimationQueue != null)
                {
                    CurrentAnimationNode = AnimationQueue.Dequeue();
                }
                else
                {
                    GridPosition = AnimationEndTile.GridPosition;
                    UpdateHopsRemaining(-1);
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
                if(CurrentAnimationNode == null && 
                   MoveInputQueue.Count > 0)
                {
                    CalculateMovement(MoveInputQueue.Dequeue());
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (CurrentAnimationNode != null)
            {
                //GD.Print(GlobalPosition);
                AnimationEndTile = Grid.GetTile(CurrentTile.GridPosition + CurrentAnimationNode.Movement);
                AnimationTimeElapsed += delta;
                float animationLength = CurrentAnimationNode.Animation.Length;
                float totalDistance = (AnimationEndTile.GridPosition - GridPosition).Length() * AnimationEndTile.Size.x;
                float distanceTravelled = totalDistance - (AnimationEndTile.GlobalPosition - GlobalPosition).Length();
                float timeRatio = AnimationTimeElapsed/animationLength;
                float pixelsToMove = totalDistance * currentJumpCurve.Interpolate(timeRatio) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(AnimationEndTile.GlobalPosition, pixelsToMove);
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
    }
}
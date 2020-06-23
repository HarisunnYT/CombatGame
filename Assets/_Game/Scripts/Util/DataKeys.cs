using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataKeys
{
    public class VariableKeys
    {
        //movement keys -------------------------------------------------------------------------------------- \\\\\\\\\\\\\\\\\\\\
        public const string MaxHorizontalSpeed = "max_horizontal_speed";
        public const string MaxVerticalSpeed = "max_vertical_speed";
        public const string HorizontalAcceleration = "horizontal_acceleration";
        public const string VerticalAcceleration = "vertical_acceleration";
        public const string GravityScale = "gravity_scale";
        public const string GroundedLinearDrag = "grounded_linear_drag";
        public const string FlyingLinearDrag = "flying_linear_drag";
        public const string JumpForce = "jump_force";
        public const string JumpRequiresGrounded = "jump_requires_grounded";
        public const string JumpsBeforeGrounding = "jumps_before_grounding";
        public const string TimeBetweenJump = "time_between_jump";
        public const string ResetVerticalVelocityOnJump = "reset_vertical_velocity_on_jump";
        public const string RotateTowardsDirection = "rotate_towards_direction";
        public const string RotationSpeed = "rotation_speed";
        public const string ConstantMovement = "constant_movement";

        //combat keys -------------------------------------------------------------------------------------- \\\\\\\\\\\\\\\\\\\\\\
        public const string AttackSpeedDamper = "attack_speed_damper";

        //technical keys -------------------------------------------------------------------------------------- \\\\\\\\\\\\\\\\\\\
        public const string AttackingButtonResetDelay = "attacking_button_reset_delay";
        public const string FlipScaleDamper = "flip_scale_damper";

        //game keys -------------------------------------------------------------------------------------- \\\\\\\\\\\\\\\\\\\
        public const string CashPerKill = "cash_per_kill";
        public const string FirstPlaceCash = "first_place_cash";
        public const string SecondPlaceCash = "second_place_cash";
        public const string ThirdPlaceCash = "third_place_cash";
        public const string FourthPlaceCash = "fourth_place_cash";
    }

    public class SpawnableKeys
    {
        //spawnable object keys -------------------------------------------------------------------------------------- \\\\\\\\\\\\\\\\\\\
        public const string WorldSpaceText = "world_space_text";
    }
}

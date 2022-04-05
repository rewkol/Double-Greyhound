using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Throwing all FlightInstruction sets into their own files so as not to clutter up the main controllers
//I could save these out to files that get parsed or whatever, but honestly who cares and this will
//Get compiled down into binary (I could also do some fun bitwise stuff manually, but this isn't 1985 anymore)
//
//I wrote the above before actually programming one of these, and might I say I am glad to have done it in here with
//syntax highlighting and mathematical operations.
public class PlayerInstructionSets
{
    private static float DEFAULT_HURTBOX_X = 1.408832f / 6.0f;
    private static float DEFAULT_HURTBOX_Y = 3.9772f / 6.0f;

    //The Jumping Attack flight program
    public static List<FlightInstruction> GetJumpInstructions(bool facingLeft)
    {
        List<FlightInstruction> instructions = new List<FlightInstruction>();

        float directionModifier = 1.0f;
        if(facingLeft)
        {
            directionModifier = -1.0f;
        }

        //Use comments to denote what each part of the instruction set is doing

        //Stay stationary through crouch animation/Shrink hurtbox
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * -0.75f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y * 0.75f));
        for (int i = 0; i < 7; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }
        //Regrow hurtbox then jump into air
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * 0.75f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y)
            .AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.40f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.40f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.39f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.39f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.38f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.37f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.35f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.020f, 0.32f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.021f, 0.29f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.022f, 0.25f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.023f, 0.21f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.024f, 0.17f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.025f, 0.14f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.027f, 0.11f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.027f, 0.08f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.024f, 0.06f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.021f, 0.05f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.018f, 0.05f, 0.0f)));

        //Shrink hurtbox again and start flattening out at apex of jump
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * 0.75f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y * 0.75f)
            .AddInstruction("Move", new Vector3(directionModifier * 0.013f, 0.03f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.010f, 0.03f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.006f, 0.02f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.003f, 0.01f, 0.0f)));

        //Wait until kick
        for (int i = 0; i < 5; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }

        //Shrink hurtbox further and create hitbox/move down
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * 1.2f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y * 0.40f)
            .AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.6f, 0.0f))
            .AddInstruction("Hitbox", new Vector3(directionModifier * 0.175f, -0.8f, 0.0f), 0.45f, 2.0f, 110)
            .AddInstruction("Hitbox", new Vector3(directionModifier * -0.25f, -0.2f, 0.0f), 1.1f, 1.3f, 110));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.60f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.62f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.64f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.66f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.68f, 0.0f)));

        //Move hurtbox down to crouching position and create shortlived impact hitbox
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * -3.15f, 0.0f), DEFAULT_HURTBOX_X * 1.3f, DEFAULT_HURTBOX_Y * 0.6f)
            .AddInstruction("Hitbox2", new Vector3(0.0f, DEFAULT_HURTBOX_Y * -0.33f, 0.0f), DEFAULT_HURTBOX_X * 1.5f, 1.2f, 20)
            .AddInstruction("Move", new Vector3(directionModifier * 0.07f, -0.70f, 0.0f)));

        //Wait until standing up
        for (int i = 0; i < 7; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }

        //Adjust hurtbox for standing up pose
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * 0.45f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y * 0.85f));

        //Wait until finished
        for (int i = 0; i < 2; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }

        //Return hurtbox to normal
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", new Vector3(0.0f, DEFAULT_HURTBOX_Y * 0.75f, 0.0f), DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y ));

        return instructions;
    }

    //The Knockback flight program
    public static List<FlightInstruction> GetKnockbackInstructions(bool facingLeft)
    {
        List<FlightInstruction> instructions = new List<FlightInstruction>();

        float directionModifier = -1.0f;
        if (facingLeft)
        {
            directionModifier = 1.0f;
        }

        //Shrink the hurtbox to 0,0 so no one can hurt him
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", DEFAULT_HURTBOX_X * 0.0f, DEFAULT_HURTBOX_Y * 0.0f)
            .AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.35f, 0.0f))
            .AddInstruction("Ignore"));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.30f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.25f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.21f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.17f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.13f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.10f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.07f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.05f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.03f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.02f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.02f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.01f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.00f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.00f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, 0.0f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.01f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.03f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.04f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.12f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.18f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.33f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.47f, 0.0f)));
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.08f, -0.60f, 0.0f)));

        //Biggest drop because this masks the transition to the grounded state
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * 0.0f, -0.78f, 0.0f)));

        //Force the player to appreciate the fact they've been hit
        for (int i = 0; i < 12; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }

        return instructions;
    }

    //Standing up flight program. Mostly just important for fixing the hurtbox after knockback
    public static List<FlightInstruction> GetStandingInstructions()
    {
        List<FlightInstruction> instructions = new List<FlightInstruction>();
        instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(0.0f, 0.85f, 0.0f)));

        //Wait for the animation to play
        for (int i = 0; i < 14; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Wait"));
        }

        //Finally, return his hurtbox to normal
        instructions.Add(new FlightInstruction().AddInstruction("Hurtbox", DEFAULT_HURTBOX_X, DEFAULT_HURTBOX_Y).AddInstruction("PayAttention"));

        return instructions;
    }

    //Stun mini blowback instruction set
    public static List<FlightInstruction> GetStunInstructions(bool facingLeft)
    {
        List<FlightInstruction> instructions = new List<FlightInstruction>();

        float directionModifier = 1.0f;
        if (facingLeft)
        {
            directionModifier = -1.0f;
        }

        for (int i = 0; i < 5; i++)
        {
            instructions.Add(new FlightInstruction().AddInstruction("Move", new Vector3(directionModifier * -0.05f, 0.0f, 0.0f)));
        }

        return instructions;
    }
}
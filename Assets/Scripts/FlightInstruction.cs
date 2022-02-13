using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightInstruction
{
    private int instructionCount;
    private List<InstructionElement> instructionList;

    public FlightInstruction()
    {
        instructionCount = 0;
        instructionList = new List<InstructionElement>();
    }

    public int GetCount()
    {
        return this.instructionCount;
    }

    public string GetCommand(int index)
    {
        return this.instructionList[index].GetCommand();
    }

    public Vector3 GetVector(int index)
    {
        return this.instructionList[index].GetVector();
    }

    public float GetX(int index)
    {
        return this.instructionList[index].GetX();
    }

    public float GetY(int index)
    {
        return this.instructionList[index].GetY();
    }

    public int GetTime(int index)
    {
        return this.instructionList[index].GetTime();
    }

    //Functions to create new instruction elements, which return the Instruction itself so they can act like a builder
    public FlightInstruction AddInstruction(string command)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, Vector3 vector)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetVector(vector);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, float x, float y)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetX(x);
        el.SetY(y);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, int time)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetTime(time);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, Vector3 vector, float x, float y)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetVector(vector);
        el.SetX(x);
        el.SetY(y);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, float x, float y, int time)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetX(x);
        el.SetY(y);
        el.SetTime(time);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, Vector3 vector, int time)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetVector(vector);
        el.SetTime(time);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }

    public FlightInstruction AddInstruction(string command, Vector3 vector, float x, float y, int time)
    {
        InstructionElement el = new InstructionElement();
        el.SetCommand(command);
        el.SetVector(vector);
        el.SetX(x);
        el.SetY(y);
        el.SetTime(time);
        this.instructionList.Add(el);
        instructionCount++;
        return this;
    }


    //All setting/getting of InstructionElements will be done through public methods of the FlightInstruction class
    private class InstructionElement
    {
        /*Need instructions that can contain the information required to do the following actions:
          Name of instruction
          Move the object
          Change object's hurtbox size
          Create new hitbox
        */
        private string command;
        private Vector3 vector;
        private float x;
        private float y;
        private int time;

        public InstructionElement()
        {
            command = "Null Command";
            vector = new Vector3(0.0f, 0.0f, 0.0f);
            x = 0.0f;
            y = 0.0f;
            time = 0;
        }

        public string GetCommand()
        {
            return this.command;
        }

        public void SetCommand(string command)
        {
            this.command = command;
        }

        public Vector3 GetVector()
        {
            return this.vector;
        }

        public void SetVector(Vector3 vector)
        {
            this.vector = vector;
        }

        public float GetX()
        {
            return this.x;
        }

        public void SetX(float x)
        {
            this.x = x;
        }

        public float GetY()
        {
            return this.y;
        }

        public void SetY(float y)
        {
            this.y = y;
        }

        public int GetTime()
        {
            return this.time;
        }

        public void SetTime(int time)
        {
            this.time = time;
        }
    }
}

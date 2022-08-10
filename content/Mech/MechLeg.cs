namespace TC2.Base.Components;



public static partial class MechLeg
{
    [IComponent.Data(Net.SendType.Reliable)]
    public partial struct Data : IComponent
    {
        public float length_a = 4;
        public float length_b = 4;
        public Vector2 destination = Vector2.Zero;
        public bool move = true;
        public bool atDestination = false;
        public float precision = 0.5f;

        public Data()
        {

        }
    }

    [IComponent.Data(Net.SendType.Unreliable)]
    public partial struct State : IComponent
    {
        public float angle_a = 0;
        public float angle_b = 0;

        public State()
        {

        }
    }

    [ISystem.Add(ISystem.Mode.Single)]
    public static void Add(Entity ent_leg, [Source.Parent] ref Attachment.Data slot, [Source.Parent] ref Mech.Data mech, [Source.Parent] ref Mech.State mechState,
        [Source.Owned] ref MechLeg.Data leg)
    {
        for (int i = 0; i < mechState.legs.Length; i++)
        {
            if (mechState.legs[i].entity == Entity.None)
            {
                mechState.legs[i].Set(ent_leg);
                mechState.numLegs ++;
                break;
            }
        }
    }

    [ISystem.Remove(ISystem.Mode.Single)]
    public static void Remove(Entity ent_leg, [Source.Parent] ref Attachment.Data slot, [Source.Parent] ref Mech.Data mech, [Source.Parent] ref Mech.State mechState,
    [Source.Owned] ref MechLeg.Data leg)
    {
        for (int i = 0; i < mechState.legs.Length; i++)
        {
            if (mechState.legs[i].entity == ent_leg)
            {
                mechState.legs[i].Set(Entity.None);
                mechState.numLegs--;
                break;
            }
        }
    }



    [ISystem.Update(ISystem.Mode.Single)]
    public static void Update(Entity ent_joint, [Source.Owned] ref Joint.Base joint, [Source.Owned, Original] ref Joint.Gear legGear, [Source.Owned] ref Joint.Pivot pivot,
        [Source.Owned] ref MechLeg.Data leg, [Source.Parent, Original] ref Joint.Gear cabGear, [Source.Parent] ref Mech.Data mech, [Source.Owned] ref Transform.Data transform,
        [Source.Owned] ref MechLeg.State state, [Source.Parent] ref Joint.Base parentJoint)
    {
        if (leg.move)
        {
            IK.Resolve2x(new Vector2(leg.length_a, leg.length_b), transform.LocalToWorld(parentJoint.offset_b), leg.destination, new Vector2(state.angle_a, state.angle_b), out Vector2 newAngles, false);
            state.angle_a = newAngles.X;
            state.angle_b = newAngles.Y;

            cabGear.rotation = state.angle_a;
            legGear.rotation = state.angle_b;

        }
    }
}
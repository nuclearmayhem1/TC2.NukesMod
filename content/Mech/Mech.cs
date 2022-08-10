namespace TC2.Base.Components;
using Nuke;

public static partial class Mech
{
    [IComponent.Data(Net.SendType.Reliable)]
    public partial struct Data : IComponent
    {
        public float leg_reach = 10;
        public float angle = 45;
        public float max_angle = 85;
        public float max_normal = 140;
        public float increment = 5;
        public Vector2 targetPos = Vector2.Zero;

        public Data()
        {

        }
    }

    [IComponent.Data(Net.SendType.Unreliable)]
    public partial struct State : IComponent
    {
        public FixedArray8<EntRef<MechLeg.Data>> legs = new FixedArray8<EntRef<MechLeg.Data>>();
        public int currentLeg = 0;
        public int numLegs = -1;

        public State()
        {

        }
    }

    [ISystem.Update(ISystem.Mode.Single)]
    public static void Update(Entity ent_mech, ISystem.Info info, [Source.Owned] ref Control.Data control, [Source.Owned] ref Vehicle.Data vehicle, 
        [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Mech.Data mech, [Source.Owned] ref Mech.State state)
    {
        bool move = false;
        if (control.keyboard.GetKey(Keyboard.Key.MoveRight))
        {
            mech.angle = 45;
            mech.max_angle = 85;
            mech.max_normal = 45;
            mech.increment = 5;
            move = true;
        }
        else if (control.keyboard.GetKey(Keyboard.Key.MoveLeft))
        {
            mech.angle = Maths.OppositeAngle(45 * Maths.deg2rad) * Maths.rad2deg;
            mech.max_angle = Maths.OppositeAngle(85 * Maths.deg2rad) * Maths.rad2deg;
            mech.max_normal = 45;
            mech.increment = -5;
            move = true;
        }
        else
        {
            goto NoInput;
        }
        float angle = mech.angle;
        bool hit = false;

        do
        {
            if (mech.increment > 0 ? angle > mech.max_angle : angle < mech.max_angle || mech.increment == 0)
            {
                break;
            }

            hit = info.GetRegion().TryLinecast(transform.position, transform.position + MathN.VectorByAngleDeg(angle, mech.leg_reach), 0, out LinecastResult result, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static);

            if (hit && result.normal.GetAngleDegrees() > -90 - mech.max_normal && result.normal.GetAngleDegrees() < -90 + mech.max_normal)
            {
                mech.targetPos = result.world_position;
            }
            else if (hit)
            {
                hit = false;
            }

            angle += mech.increment;
        } while (!hit);
    NoInput:

        ref MechLeg.Data leg =  ref state.legs[state.currentLeg].GetValueOrNullRef();
        if (!leg.IsNull())
        {


            leg.destination = mech.targetPos;


            if (leg.atDestination)
            {
                state.currentLeg++;
                if (state.currentLeg > state.numLegs)
                {
                    state.currentLeg = 0;
                }
            }
        }
    }

#if CLIENT
    public struct DebugUI : IGUICommand
    {
        public Entity ent_mech;
        public Transform.Data transform;
        public Control.Data control;
        public Mech.Data data;

        public void Draw()
        {
            using (var window = GUI.Window.Interaction("Mech", this.ent_mech))
            {

                float angle = data.angle;
                bool hit = false;

                do
                {
                    if (data.increment > 0 ? angle > data.max_angle : angle < data.max_angle || data.increment == 0)
                    {
                        break;
                    }

                    hit = Client.GetRegion().TryLinecast(transform.position, transform.position + MathN.VectorByAngleDeg(angle, data.leg_reach), 0, out LinecastResult result, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static);
                    
                    if (hit && result.normal.GetAngleDegrees() > -90 -data.max_normal && result.normal.GetAngleDegrees() < -90 + data.max_normal)
                    {
                        GUI.DrawLine(GUI.WorldToCanvas(transform.position), GUI.WorldToCanvas(transform.position + MathN.VectorByAngleDeg(angle, data.leg_reach)), Color32BGRA.Green, 1);
                        GUI.DrawCircle(GUI.WorldToCanvas(result.world_position), 5, Color32BGRA.Green);
                        GUI.DrawLine(GUI.WorldToCanvas(result.world_position), GUI.WorldToCanvas(result.world_position + result.normal), new Color32BGRA(255, 0, 0, 255), 1);

                        GUI.DrawText(result.normal.GetAngleDegrees().ToString(), GUI.WorldToCanvas(result.world_position - new Vector2(1, 1)));

                    }
                    else if (hit)
                    {
                        GUI.DrawLine(GUI.WorldToCanvas(transform.position), GUI.WorldToCanvas(transform.position + MathN.VectorByAngleDeg(angle, data.leg_reach)), Color32BGRA.Yellow, 1);
                        GUI.DrawCircle(GUI.WorldToCanvas(result.world_position), 5, Color32BGRA.Yellow);
                        GUI.DrawLine(GUI.WorldToCanvas(result.world_position), GUI.WorldToCanvas(result.world_position + result.normal), new Color32BGRA(255, 0, 0, 255), 1);

                        GUI.DrawText(result.normal.GetAngleDegrees().ToString(), GUI.WorldToCanvas(result.world_position - new Vector2(1, 1)));
                        hit = false;
                    }
                    else
                    {
                        GUI.DrawLine(GUI.WorldToCanvas(transform.position), GUI.WorldToCanvas(transform.position + MathN.VectorByAngleDeg(angle, data.leg_reach)), Color32BGRA.Red, 1);
                        GUI.DrawCircle(GUI.WorldToCanvas(transform.position + MathN.VectorByAngleDeg(angle, data.leg_reach)), 5, Color32BGRA.Red);
                    }

                    angle += data.increment;
                } while (!hit);
            }
        }
    }

    [ISystem.EarlyGUI(ISystem.Mode.Single)]
    public static void OnGUI(Entity ent_mech, ISystem.Info info, [Source.Owned] ref Mech.Data mech, [Source.Owned] ref Control.Data control, [Source.Owned] ref Vehicle.Data vehicle,
        [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Mech.Data data)
    {
        if (App.debug_mode)
        {
            var gui = new DebugUI()
            {
                ent_mech = ent_mech,
                transform = transform,
                control = control,
                data = data,
            };
            gui.Submit();
        }
    }
#endif
}